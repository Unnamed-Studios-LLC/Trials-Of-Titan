using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class SpringChasePlayerValue
    {
        //public Vec2 lastLeaderPosition;
        public Player player;

        public Vec2 velocity;

        public object cooldownValue;
    }

    public class SpringChasePlayer : LogicAction<SpringChasePlayerValue>
    {
        private float distance;

        private float acceleration;

        private float drag;

        private float velocityMax;

        private float searchRadius = 8;

        private Cooldown searchCd;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "distance":
                    distance = reader.ReadFloat();
                    return true;
                case "acceleration":
                    acceleration = reader.ReadFloat();
                    return true;
                case "drag":
                    drag = reader.ReadFloat();
                    return true;
                case "velocityMax":
                    velocityMax = reader.ReadFloat();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (searchCd.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out SpringChasePlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SpringChasePlayerValue();
            searchCd.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref SpringChasePlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;

            if (searchCd.Tick(ref obj.cooldownValue, ref time))
            {
                obj.player = enemy.GetClosestPlayer(searchRadius);
            }

            if (obj.player == null || obj.player.world == null) return;

            var leaderVector = obj.player.position.Value - enemy.position.Value;
            var leaderLength = leaderVector.Length;

            float dragValue = drag;
            float currentVelocityLength;
            if (leaderLength > distance)
            {
                obj.velocity += leaderVector.ChangeLength((leaderLength - distance) * acceleration, leaderLength);
                currentVelocityLength = obj.velocity.Length;
                if (currentVelocityLength > velocityMax)
                {
                    obj.velocity = obj.velocity.ChangeLength(velocityMax, currentVelocityLength);
                    currentVelocityLength = velocityMax;
                }
            }
            else
            {
                currentVelocityLength = obj.velocity.Length;
                float sqr = leaderLength / distance - 0.9f;
                dragValue = Math.Min(Math.Max(drag / (sqr * sqr), drag), float.MaxValue);
            }

            var newVelocityLength = currentVelocityLength - dragValue * currentVelocityLength * 2 * (float)time.deltaTime;
            if (newVelocityLength <= 0)
                obj.velocity = Vec2.zero;
            else
                obj.velocity = obj.velocity.ChangeLength(newVelocityLength, currentVelocityLength);

            enemy.MoveBy(obj.velocity * (float)time.deltaTime);
        }
    }
}
