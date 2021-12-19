using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using World.GameState;

namespace World.Map.Objects.Entities
{
    public abstract class NotPlayable : Entity
    {
        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (world != null)
                world.objects.NotPlayableTicked(this);
        }

        public void Chat(string text)
        {
            foreach (var player in playersSentTo)
                player.AddChat(new ChatData(gameId, text));
        }

        public void MoveBy(Vec2 vector, float collideBounds = 0, bool ignoreCollision = false)
        {
            if (HasServerEffect(StatusEffect.Slowed))
                vector *= 0.5f;

            var pos = position.Value + vector;

            if (ignoreCollision)
            {
                MoveTo(pos);
                return;
            }

            bool xOk = CanWalk(pos.x, position.Value.y) && (collideBounds == 0 || CanWalkBounds(pos.x, position.Value.y, collideBounds));
            bool yOk = CanWalk(position.Value.x, pos.y) && (collideBounds == 0 || CanWalkBounds(position.Value.x, pos.y, collideBounds));

            if (xOk && yOk)
            {
                if (CanWalk(pos.x, pos.y) && (collideBounds == 0 || CanWalkBounds(pos.x, pos.y, collideBounds)))
                {
                    MoveTo(pos);
                    return;
                }
            }

            if (xOk)
            {
                MoveTo(new Vec2(pos.x, position.Value.y));
            }
            else if (yOk)
            {
                MoveTo(new Vec2(position.Value.x, pos.y));
            }
        }

        private bool CanWalkBounds(float x, float y, float bounds)
        {
            if (!CanWalk(x - bounds, y + bounds))
                return false;
            if (!CanWalk(x, y + bounds))
                return false;
            if (!CanWalk(x + bounds, y + bounds))
                return false;

            if (!CanWalk(x - bounds, y))
                return false;
            if (!CanWalk(x + bounds, y))
                return false;

            if (!CanWalk(x - bounds, y - bounds))
                return false;
            if (!CanWalk(x, y - bounds))
                return false;
            if (!CanWalk(x + bounds, y - bounds))
                return false;

            return true;
        }

        private bool CanWalk(float x, float y)
        {
            return world.tiles.CanWalk((int)x, (int)y);
        }

        public void MoveTo(Vec2 point)
        {
            position.Value = point;
        }
    }
}
