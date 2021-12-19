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
    public class StayNearSpawnValue
    {

    }

    public class StayNearSpawn : LocationEnforcement<StayNearSpawnValue>
    {
        protected override void InitValue(Entity enemy, out StayNearSpawnValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new StayNearSpawnValue();
        }

        protected override bool ShouldEnforce(Entity entity, ref StayNearSpawnValue obj, ref StateContext context, ref WorldTime time, out Vec2 vector)
        {
            vector = entity.spawn - entity.position.Value;
            if (vector.SqrLength > distance * distance)
            {
                vector = vector.Normalize();
                return true;
            }
            return false;
        }
    }
}
