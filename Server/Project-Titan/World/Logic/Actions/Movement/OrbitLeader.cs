using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class OrbitLeaderValue
    {
        public object cooldownValue;

        public float angleValue;

        public float speed;

        public float radius;
    }

    public class OrbitLeader : LogicAction<OrbitLeaderValue>
    {
        private Range speed;

        private Range angle;

        private bool hasAngle = false;

        private Range radius;

        private Cooldown calcCooldown = new Cooldown(0, 0);

        private bool ignoreCollision = false;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "angle":
                    angle = reader.ReadAngle();
                    hasAngle = true;
                    return true;
                case "radius":
                case "radiusMin":
                    radius = reader.ReadFloat();
                    return true;
                case "radiusMax":
                    radius.max = reader.ReadFloat();
                    return true;
                case "ignoreCollision":
                    ignoreCollision = reader.ReadBool();
                    return true;
            }
            if (calcCooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out OrbitLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new OrbitLeaderValue();

            if (calcCooldown.period.min != 0 || calcCooldown.period.max != 0 || calcCooldown.delay.min != 0 || calcCooldown.delay.max != 0)
                Calc(ref obj);

            calcCooldown.Init(out obj.cooldownValue);
            if (hasAngle)
                obj.angleValue = angle.GetRandom();
            else if ((entity is Enemy enemy) && enemy.leader != null)
                obj.angleValue = enemy.leader.position.Value.AngleTo(enemy.position.Value);
        }

        public override void Tick(Entity entity, ref OrbitLeaderValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (enemy.leader == null || enemy.leader.IsDead) return;
            if (calcCooldown.Tick(ref obj.cooldownValue, ref time))
                Calc(ref obj);

            obj.angleValue += (obj.speed / obj.radius) * (float)time.deltaTime;
            var targetPos = enemy.leader.position.Value + Vec2.FromAngle(obj.angleValue) * obj.radius;
            var vector = targetPos - enemy.position.Value;
            enemy.MoveBy(vector.ChangeLength(obj.speed * (float)time.deltaTime), 0, ignoreCollision);
        }

        private void Calc(ref OrbitLeaderValue obj)
        {
            obj.speed = speed.GetRandom();
            obj.radius = radius.GetRandom();
        }
    }
}
