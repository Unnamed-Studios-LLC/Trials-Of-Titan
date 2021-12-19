using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class HoverLeaderValue
    {
    }

    public class HoverLeader : LogicAction<HoverLeaderValue>
    {
        private float minHover;

        private float maxHover;

        private float speed;

        private bool topMostLeader;

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
                case "speed":
                    speed = reader.ReadFloat();
                    break;
                case "topMost":
                    topMostLeader = reader.ReadBool();
                    break;
            }
            return false;
        }

        public override void Init(Entity entity, out HoverLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new HoverLeaderValue();
        }

        public override void Tick(Entity entity, ref HoverLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (enemy.leader == null) return;

            var leader = enemy.leader;
            if (topMostLeader)
                leader = leader.GetTopmostLeader(out var topCount);

            var distanceFromLeader = enemy.DistanceTo(leader);
            if (distanceFromLeader < minHover)
            {
                var angle = enemy.leader.AngleTo(enemy);
                enemy.MoveBy(Vec2.FromAngle(angle) * speed * (float)time.deltaTime);
            }
            else if (distanceFromLeader > maxHover)
            {
                var angle = enemy.AngleTo(enemy.leader);
                enemy.MoveBy(Vec2.FromAngle(angle) * speed * (float)time.deltaTime);
            }
        }
    }
}
