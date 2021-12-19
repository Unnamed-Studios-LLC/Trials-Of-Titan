using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.Components
{
    public enum TargetingFunction
    {
        Closest,
        Random
    }

    public struct TargetingSystem
    {
        public TargetingFunction function;

        public float offset;

        public float rand;

        public Player GetPlayer(Entity entity, float searchRadius)
        {
            Player target = null;
            switch (function)
            {
                case TargetingFunction.Closest:
                    target = entity.GetClosestPlayer(searchRadius);
                    break;
                case TargetingFunction.Random:
                    target = entity.world.objects.GetRandomPlayer(entity.position.Value, searchRadius);
                    break;
            }
            return target;
        }

        public Vec2 GetTargetPosition(Entity entity, Player target)
        {
            var pos = target.position.Value;
            var vector = pos - entity.position.Value;

            var length = vector.Length;
            if (length > offset)
            {
                vector.ChangeLength(length - offset, length);
            }
            else
                vector = Vec2.zero;

            var targetPosition = entity.position.Value + vector;
            if (rand > 0)
            {
                targetPosition += Vec2.FromAngle(AngleUtils.PI_2 * Rand.FloatValue()) * (rand * Rand.FloatValue());
            }

            return targetPosition;
        }

        public bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "target":
                    function = (TargetingFunction)Enum.Parse(typeof(TargetingFunction), reader.ReadString());
                    break;
                case "offset":
                    offset = reader.ReadFloat();
                    return true;
                case "rand":
                    rand = reader.ReadFloat();
                    return true;
            }
            return false;
        }
    }
}
