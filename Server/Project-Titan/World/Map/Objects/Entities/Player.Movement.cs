using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Net;
using TitanCore.Net.Packets.Server;
using Utils.NET.Algorithms;
using Utils.NET.Geometry;
using Utils.NET.Logging;

namespace World.Map.Objects.Entities
{
    public partial class Player
    {
        private struct MovementData
        {
            public float angle;

            public float angleChange;

            public float distance;
        }

        private List<MovementData> predictionVectors = new List<MovementData>();

        private DateTime lastTeleport;

        private Vec2? gotoPosition;

        private Vec2 lastMovePosition;

        public Vec2 GetTickPosition()
        {
            if (gotoPosition.HasValue)
                return gotoPosition.Value;
            return position.Value;
        }

        public void MoveTo(uint time, Vec2 newPosition)
        {
            AddPrediction(position.Value, newPosition);
            position.Value = newPosition;
            lastMovePosition = newPosition;
            world.objects.UpdatePlayer(this);
        }

        public bool CanMoveTo(Vec2 position, uint time)
        {
            if (!(gameState.playerState?.AdvancePosition(position, time) ?? true))
                return false;

            if (!CanWalkOn(position.x, position.y))
                return false;

            return true;
        }

        public bool CanWalkOn(float x, float y)
        {
            if (!LineOkay(x, y))
                return false;
            if (!world.tiles.PlayerCanWalk(x, y))
                return false;
            return true;
        }

        private bool LineOkay(float x, float y)
        {
            foreach (var position in Bresenham.Line(lastMovePosition.ToInt2(), new Int2((int)x, (int)y)))
                if (world.tiles.GetCollisionType(position.x, position.y).HasFlag(CollisionType.Wall))
                    return false;
            return true;
        }

        private void AddPrediction(Vec2 position, Vec2 newPosition)
        {
            var data = new MovementData()
            {
                angle = position.AngleTo(newPosition),
                angleChange = 0,
                distance = position.DistanceTo(newPosition)
                
            };

            if (predictionVectors.Count > 0)
            {
                var last = predictionVectors[0];
                var dif = data.angle - last.angle;
                if (dif > 180f)
                    dif -= 360f;
                else if (dif < -180f)
                    dif += 360f;
                if (Math.Abs(dif) > 170f)
                    dif = 0;
                data.angleChange = dif;
            }

            predictionVectors.Insert(0, data);
            if (predictionVectors.Count > 3)
                predictionVectors.RemoveAt(3);
        }

        public Vec2 PredictPosition(float deltaTime)
        {
            if (predictionVectors.Count == 0) return position.Value;

            float averageChange = 0;
            float averageDistance = 0;

            for (int i = 0; i < predictionVectors.Count; i++)
            {
                var data = predictionVectors[i];
                averageChange += data.angleChange;
                averageDistance += data.distance;
            }

            return position.Value + Vec2.FromAngle(predictionVectors[0].angle + averageChange * 0.25f) * averageDistance * 5 * 0.5f;
        }

        public bool Teleport(GameObject gameObject)
        {
            if (!gameObject.Teleportable || (DateTime.Now - lastTeleport).TotalSeconds < 10) return false;
            lastTeleport = DateTime.Now;
            Goto(gameObject.position.Value);
            return true;
        }

        public void Goto(Vec2 position)
        {
            gotoPosition = position;
            gameState.projectileBlockTick = lastTickId - WorldManager.Ticks_Per_Second * 2;
            client.SendAsync(new TnGoto(position));
        }

        public void GotoAck(uint tickId)
        {
            uint time = NetConstants.Client_Delta * tickId;

            gameState.playerState.AddClientStatusEffect(StatusEffect.Invulnerable, time, 1000);
            gameState.playerState.DidGoto(gotoPosition.Value, time);
            lastMovePosition = gotoPosition.Value;
            position.Value = gotoPosition.Value;
            gotoPosition = null;
        }

        private void TickMovement(ref WorldTime time)
        {
            if (time.tickId % 10 != 0) return;
            var pos = position.Value.ToInt2();
            var tile = world.tiles.GetTile(pos.x, pos.y);
            var tileInfo = tile.GetTileInfo();
            if (tileInfo != null)
            {
                if (tileInfo.damage > 0)
                {
                    ServerDamage(tileInfo.damage, tileInfo);
                }
            }
        }
    }
}
