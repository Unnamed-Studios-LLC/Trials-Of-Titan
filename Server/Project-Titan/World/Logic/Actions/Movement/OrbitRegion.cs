using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class OrbitRegionValue
    {
        public object cooldownValue;

        public float angleValue;

        public float speed;

        public float radius;

        public Vec2 target;
    }

    public class OrbitRegion : LogicAction<OrbitRegionValue>
    {
        private Range speed;

        private Range angle;

        private bool hasAngle = false;

        private Range radius;

        private Cooldown calcCooldown = new Cooldown(0, 0);

        private bool ignoreCollision = false;

        private Region region;

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
                case "region":
                    Enum.TryParse(reader.ReadString(), true, out region);
                    return true;
            }
            if (calcCooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out OrbitRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new OrbitRegionValue();

            if (calcCooldown.period.min != 0 || calcCooldown.period.max != 0 || calcCooldown.delay.min != 0 || calcCooldown.delay.max != 0)
                Calc(entity, ref obj);

            calcCooldown.Init(out obj.cooldownValue);
            if (hasAngle)
                obj.angleValue = angle.GetRandom();
            else if ((entity is Enemy enemy) && enemy.leader != null)
                obj.angleValue = obj.target.AngleTo(enemy.position.Value);
        }

        public override void Tick(Entity entity, ref OrbitRegionValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return;
            if (calcCooldown.Tick(ref obj.cooldownValue, ref time))
                Calc(entity, ref obj);

            obj.angleValue += (obj.speed / obj.radius) * (float)time.deltaTime;
            var targetPos = obj.target + Vec2.FromAngle(obj.angleValue) * obj.radius;
            var vector = targetPos - enemy.position.Value;
            enemy.MoveBy(vector.ChangeLength(obj.speed * (float)time.deltaTime), 0, ignoreCollision);
        }

        private Vec2 GetRegionPoint(Entity entity)
        {
            return entity.world.GetClosestRegion(region, entity.position.Value);
        }

        private void Calc(Entity entity, ref OrbitRegionValue obj)
        {
            obj.speed = speed.GetRandom();
            obj.radius = radius.GetRandom();
            obj.target = GetRegionPoint(entity);
        }
    }
}
