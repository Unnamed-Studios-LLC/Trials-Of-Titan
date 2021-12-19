using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class HoverLeaderChaseValue
    {
        public double chaseStart;

        public Player target;

        public bool chasing = false;
    }

    public class HoverLeaderChase : LogicAction<HoverLeaderChaseValue>
    {
        private float minHover;

        private float maxHover;

        private float searchRadius = 8;

        private float chaseDuration;

        private float speed;

        private bool topMostLeader;

        /// <summary>
        /// The minimum distance to be from the player
        /// </summary>
        private float minDistance;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "minHover":
                    minHover = reader.ReadFloat();
                    break;
                case "maxHover":
                    maxHover = reader.ReadFloat();
                    break;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    break;
                case "chaseDuration":
                    chaseDuration = reader.ReadFloat();
                    break;
                case "speed":
                    speed = reader.ReadFloat();
                    break;
                case "min":
                    minDistance = reader.ReadFloat();
                    return true;
                case "topMost":
                    topMostLeader = reader.ReadBool();
                    break;
            }
            return false;
        }

        public override void Init(Entity entity, out HoverLeaderChaseValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new HoverLeaderChaseValue();
        }

        public override void Tick(Entity entity, ref HoverLeaderChaseValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (enemy.leader == null) return;

            var leader = enemy.leader;
            if (topMostLeader)
                leader = leader.GetTopmostLeader(out var topCount);

            if (time.totalTime > obj.chaseStart + chaseDuration)
                obj.chasing = false;

            if (obj.chasing)
            {
                if (obj.target.world == null)
                {
                    obj.chasing = false;
                    return;
                }

                //var vector = Vec2.FromAngle(enemy.AngleTo(obj.target));
                //enemy.MoveBy(vector * speed * (float)time.deltaTime);

                var vector = obj.target.position.Value - enemy.position.Value;
                var length = vector.Length;
                if (length < minDistance) return;
                enemy.MoveBy(vector.ChangeLength((float)time.deltaTime * speed, length));
                return;
            }

            var distanceFromLeader = enemy.DistanceTo(leader);
            if (distanceFromLeader < minHover)
            {
                var angle = leader.AngleTo(enemy);
                enemy.MoveBy(Vec2.FromAngle(angle) * speed * (float)time.deltaTime);
            }
            else if (distanceFromLeader > maxHover)
            {
                var angle = enemy.AngleTo(leader);
                enemy.MoveBy(Vec2.FromAngle(angle) * speed * (float)time.deltaTime);
            }
            else if (time.tickId % 4 == 0) // delay detection to save performance
            {
                var player = enemy.GetClosestPlayer(searchRadius);
                if (player != null)
                {
                    obj.target = player;
                    obj.chaseStart = time.totalTime;
                    obj.chasing = true;
                    return;
                }
            }
        }
    }
}
