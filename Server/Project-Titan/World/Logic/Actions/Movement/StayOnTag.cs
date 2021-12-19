using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class StayOnTagValue
    {
        public Vec2 lastPosition;
    }

    public class StayOnTag : LocationEnforcement<StayOnTagValue>
    {
        /// <summary>
        /// The tag to enforce to
        /// </summary>
        private string tag;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "tag":
                    tag = reader.ReadString();
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void InitValue(Entity enemy, out StayOnTagValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new StayOnTagValue();
            obj.lastPosition = enemy.spawn;
        }

        protected override bool ShouldEnforce(Entity entity, ref StayOnTagValue obj, ref StateContext context, ref WorldTime time, out Vec2 vector)
        {
            var newPosition = entity.position.Value;
            var tile = entity.world.tiles.GetTile((int)newPosition.x, (int)newPosition.y);
            var info = tile.GetTileInfo();

            var inTag = (info != null && tag.Equals(info.tag, StringComparison.OrdinalIgnoreCase));
            if (!inTag && newPosition.SqrDistanceTo(obj.lastPosition) > distance * distance)
            {
                vector = (obj.lastPosition - newPosition).Normalize();
                return true;
            }
            else
            {
                vector = Vec2.zero;
                if (inTag)
                    obj.lastPosition = newPosition;
                return false;
            }
        }
    }
}
