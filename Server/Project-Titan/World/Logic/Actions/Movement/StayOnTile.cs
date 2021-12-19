using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{

    public class StayOnTileValue
    {
        public Vec2 lastPosition;
    }

    public class StayOnTile : LocationEnforcement<StayOnTileValue>
    {
        /// <summary>
        /// The tile to enforce to
        /// </summary>
        private HashSet<ushort> tileTypes = new HashSet<ushort>();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "tile":
                    var tileName = reader.ReadString();
                    var tileInfo = GameData.GetObjectByName(tileName);
                    if (tileInfo == null || tileInfo.Type != GameObjectType.Tile)
                        Log.Write("No tile named: " + tileName);
                    else
                        tileTypes.Add(tileInfo.id);
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void InitValue(Entity enemy, out StayOnTileValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new StayOnTileValue();
            obj.lastPosition = enemy.spawn;
        }

        protected override bool ShouldEnforce(Entity entity, ref StayOnTileValue obj, ref StateContext context, ref WorldTime time, out Vec2 vector)
        {
            var newPosition = entity.position.Value;
            var tile = entity.world.tiles.GetTile((int)newPosition.x, (int)newPosition.y);

            bool contains = tileTypes.Contains(tile.tileType);
            if (!contains && newPosition.SqrDistanceTo(obj.lastPosition) > distance * distance)
            {
                vector = (obj.lastPosition - newPosition).Normalize();
                return true;
            }
            else
            {
                vector = Vec2.zero;
                if (contains)
                    obj.lastPosition = newPosition;
                return false;
            }
        }
    }
}
