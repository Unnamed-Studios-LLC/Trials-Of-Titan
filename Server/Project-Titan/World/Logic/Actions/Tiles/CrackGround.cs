using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using Utils.NET.Algorithms;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Tiles
{
    public class CrackGroundValue
    {
        public int nextPosition = 0;

        public List<Int2> path;

        public float remainingTime;
    }

    public class CrackGround : LogicAction<CrackGroundValue>
    {
        private ushort tile;

        private float rate;

        private float radius;

        private int joints = 0;

        private Range jointAngle;

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
                        tile = tileInfo.id;
                    return true;
                case "rate":
                    rate = 1.0f / reader.ReadFloat();
                    return true;
                case "radius":
                    radius = reader.ReadFloat();
                    return true;
                case "joints":
                    joints = reader.ReadInt();
                    return true;
                case "jointAngle":
                    jointAngle = reader.ReadAngle();
                    return true;
                case "jointAngleMin":
                    jointAngle.min = reader.ReadAngle();
                    return true;
                case "jointAngleMax":
                    jointAngle.max = reader.ReadAngle();
                    return true;
            }
            return false;
        }
        
        public override void Init(Entity entity, out CrackGroundValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new CrackGroundValue();
            obj.path = GeneratePath(entity.position.Value.ToInt2());
        }

        private List<Int2> GeneratePath(Vec2 position)
        {
            var path = new List<Int2>();
            path.Add(position.ToInt2());
            float radiusStep = radius / (joints + 1);

            float angle = Rand.AngleValue();

            for (int i = 0; i < joints + 1; i++)
            {
                var jointPosition = position + Vec2.FromAngle(angle) * radiusStep;
                foreach (var point in Bresenham.Line(position.ToInt2(), jointPosition.ToInt2()))
                {
                    if (path[path.Count - 1] == point) continue;
                    path.Add(point);
                }
                position = jointPosition;

                angle += jointAngle.GetRandom();
            }

            return path;
        }

        public override void Tick(Entity entity, ref CrackGroundValue obj, ref StateContext context, ref WorldTime time)
        {
            obj.remainingTime -= (float)time.deltaTime;
            while (obj.remainingTime < 0)
            {
                if (obj.nextPosition >= obj.path.Count) return;
                var position = obj.path[obj.nextPosition++];
                var currentTile = entity.world.tiles.GetTile(position.x, position.y);
                var collisionType = entity.world.tiles.GetCollisionType(position.x, position.y);
                if (currentTile.tileType == 0 || collisionType.HasFlag(CollisionType.Wall))
                {
                    obj.nextPosition = obj.path.Count;
                    return;
                }

                entity.world.tiles.SetTileAndBroadcast(new MapTile((ushort)position.x, (ushort)position.y, tile, 0));
                obj.remainingTime += rate;
            }
        }
    }
}
