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
    public class OrbitPlayerValue
    {
        public object cooldownValue;

        public float angleValue;

        public float speed;

        public float radius;
    }

    public class OrbitPlayer : LogicAction<OrbitPlayerValue>
    {
        private Range speed;

        private Range angle;

        private bool hasAngle = false;

        private Range radius;

        private Cooldown calcCooldown = new Cooldown(0, 0);

        private float searchRadius = 8;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed.min = speed.max = reader.ReadFloat();
                    return true;
                case "angle":
                    angle.min = angle.max = reader.ReadAngle();
                    hasAngle = true;
                    return true;
                case "radius":
                    radius.min = radius.max = reader.ReadFloat();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (calcCooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out OrbitPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new OrbitPlayerValue();

            if (calcCooldown.period.min != 0 || calcCooldown.period.max != 0 || calcCooldown.delay.min != 0 || calcCooldown.delay.max != 0)
                Calc(ref obj);

            calcCooldown.Init(out obj.cooldownValue);
            if (hasAngle)
                obj.angleValue = angle.GetRandom();
            else if ((entity is Enemy enemy) && enemy.leader != null)
                obj.angleValue = (enemy.leader.position.Value - enemy.position.Value).Angle;
        }

        public override void Tick(Entity entity, ref OrbitPlayerValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;

            var player = enemy.GetClosestPlayer(searchRadius);
            if (player == null) return;

            if (calcCooldown.Tick(ref obj.cooldownValue, ref time))
                Calc(ref obj);

            obj.angleValue += (obj.speed / obj.radius) * (float)time.deltaTime;
            var targetPos = player.position.Value + Vec2.FromAngle(obj.angleValue) * obj.radius;
            var vector = targetPos - enemy.position.Value;
            enemy.MoveBy(vector.ChangeLength(obj.speed * (float)time.deltaTime));
        }

        private void Calc(ref OrbitPlayerValue obj)
        {
            obj.speed = speed.GetRandom();
            obj.radius = radius.GetRandom();
        }
    }
}
