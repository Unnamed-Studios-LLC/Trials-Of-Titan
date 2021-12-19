using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Files;
using TitanCore.Gen;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Spawning;
using static TitanCore.Files.MapElementFile;

namespace World.Worlds.Gates
{
    public class BhogninsGate : Gate
    {
        private const int Shape_Variation_Count = 6;

        public override ushort PreferredPortal => 0xaa8;

        public override string WorldName => "Bubra Barrens";

        protected override string DefaultMusic => "Desert_Gorge";

        protected override int PortalTime => 60;

        public override bool LimitSight => true;

        private static SetPiece bubraCamp = SetPiece.Load("gorge/bubra-camp.mef");

        private static SetPiece boneWormRoom = SetPiece.Load("gorge/bone-worm-room.mef");

        private static SetPiece bhogninRoom = SetPiece.Load("gorge/bone-bhognin-room.mef");

        private Int2 spawnPosition;

        private Int2 boneWormRoomPosition;

        private Int2 boneWormSpawnPosition;

        private Int2 bhogninRoomPosition;

        private Int2 bhogninSpawnPosition;

        private List<Int2> bhogninDoors = new List<Int2>();

        protected override MapElementFile LoadMap()
        {
            var shapeFile = MapElementFile.ReadFrom($"Map/Files/Gates/Bhognin/bhognin-shape-{Rand.Next(Shape_Variation_Count) + 1}.mef");

            var map = new TitanCore.Gen.Map(shapeFile.width, shapeFile.height);
            
            foreach (var point in map.EachPoint())
            {
                var tile = shapeFile.tiles[point.x, point.y];
                if (tile.tileType > 0)
                {
                    if (tile.tileType == 0xb07)
                        map.Set(point, MapElementType.Ground | MapElementType.Tag1);
                    else
                        map.Set(point, MapElementType.Ground | MapElementType.Tag2);
                }
            }

            foreach (var region in shapeFile.regions)
            {
                var point = new Int2((int)region.x, (int)region.y);
                switch (region.regionType)
                {
                    case Region.Spawn:
                        spawnPosition = point;
                        //map.Set(point, map.Get(point) | MapElementType.Spawn);
                        break;
                    case Region.Tag1:
                        boneWormRoomPosition = point;
                        //map.Set(point, map.Get(point) | MapElementType.Tag3);
                        break;
                    case Region.Tag2:
                        //map.Set(point, map.Get(point) | MapElementType.Tag4);
                        break;
                    case Region.Tag3:
                        bhogninRoomPosition = point;
                        //map.Set(point, map.Get(point) | MapElementType.Tag5);
                        break;
                }
            }

            int edgeSize = 50;
            map = new TitanCore.Gen.Map(map.width + edgeSize * 2, map.height + edgeSize * 2, map);
            spawnPosition += edgeSize;
            boneWormRoomPosition += edgeSize;
            bhogninRoomPosition += edgeSize;

            foreach (var region in boneWormRoom.file.regions)
            {
                switch (region.regionType)
                {
                    case Region.Tag1:
                        boneWormRoomPosition.x -= (int)region.x;
                        boneWormRoomPosition.y -= (int)region.y;
                        break;
                    case Region.Tag2:
                        boneWormSpawnPosition = new Int2((int)region.x, (int)region.y);
                        break;
                }
            }

            boneWormSpawnPosition += boneWormRoomPosition;

            foreach (var region in bhogninRoom.file.regions)
            {
                switch (region.regionType)
                {
                    case Region.Tag1:
                        bhogninRoomPosition.x -= (int)region.x;
                        bhogninRoomPosition.y -= (int)region.y;
                        bhogninDoors.Add(new Int2((int)region.x, (int)region.y));
                        break;
                    case Region.Tag2:
                        bhogninSpawnPosition = new Int2((int)region.x, (int)region.y);
                        break;
                    case Region.Tag4:
                        bhogninDoors.Add(new Int2((int)region.x, (int)region.y));
                        break;
                }
            }

            for (int i = 0; i < bhogninDoors.Count; i++)
            {
                var p = bhogninDoors[i];
                p += bhogninRoomPosition + 1;
                bhogninDoors[i] = p;
            }

            bhogninSpawnPosition += bhogninRoomPosition;

            WriteSetPieceGround(map, bubraCamp, spawnPosition, true);
            WriteSetPieceGround(map, boneWormRoom, boneWormRoomPosition, false);
            WriteSetPieceGround(map, bhogninRoom, bhogninRoomPosition, false);

            var distanceField = map.GetDistanceField(spawnPosition, out var maxDistance);

            map.Extrude(MapElementType.Wall, map.GetMasses(MapElementType.Ground));

            var wallDistanceField = map.GenerateTypeDistanceField(3, MapElementType.Wall);

            var file = Rasterize(map, wallDistanceField, distanceField, maxDistance);

            return file;
        }
        private MapElementFile Rasterize(TitanCore.Gen.Map map, int[,] wallDistanceField, int[,] distanceField, int maxDistance)
        {
            var tiles = new MapElementFile.MapTileElement[map.width, map.height];
            foreach (var point in map.EachPoint())
            {
                var type = map.Get(point);
                ushort tileType = 0;
                ushort objectType = 0;

                if ((type & MapElementType.Ground) == MapElementType.Ground)
                {
                    var wallDistance = wallDistanceField[point.x, point.y];
                    var spawnDistance = distanceField[point.x, point.y];

                    tileType = GetTile(point.x, point.y, map.width, map.height, type, wallDistance, spawnDistance, maxDistance);
                    objectType = GetObject(point.x, point.y, width, height, type, wallDistance, spawnDistance, maxDistance);
                }

                if ((type & MapElementType.Wall) == MapElementType.Wall)
                {
                    objectType = GetWall(point.x, point.y, map.width, map.height, type);
                }

                tiles[point.x, point.y] = new MapElementFile.MapTileElement()
                {
                    tileType = tileType,
                    objectType = objectType
                };
            }

            return new MapElementFile()
            {
                width = map.width,
                height = map.height,
                tiles = tiles,
                entities = new MapElementFile.MapEntityElement[0],
                regions = new MapElementFile.MapRegionElement[0]
            };
        }

        private ushort GetTile(int x, int y, int width, int height, MapElementType type, int wallDistance, float spawnDistance, int maxSpawnDistance)
        {
            if (wallDistance <= 1)
                return 0xb4a;
            else if (wallDistance <= 2 && Rand.Next(3) == 0)
                return 0xb4a;

            if (type.HasFlag(MapElementType.Tag1))
                return 0xb4b;
            else
                return 0xb29;
        }

        private ushort GetWall(int x, int y, int width, int height, MapElementType type)
        {
            return 0xaa0;
        }

        private ushort GetObject(int x, int y, int width, int height, MapElementType type, int wallDistance, int spawnDistance, int maxSpawnDistance)
        {
            var rnd = Rand.Next(1000);

            if (type.HasFlag(MapElementType.Tag1))
            {
                if (wallDistance <= 1)
                {
                    if (RndChance(ref rnd, 25))
                        return 0x1081; // falling rock

                    if (RndChance(ref rnd, 40))
                        return 0xaa7; // gorge rock
                    if (RndChance(ref rnd, 10))
                        return 0xaa9; // barrens plant 1
                    if (RndChance(ref rnd, 10))
                        return 0xaaa; // barrens plant 2
                }

                if (wallDistance <= 2)
                {
                    if (RndChance(ref rnd, 10))
                        return 0xaab; // skeletal ribs
                    if (RndChance(ref rnd, 10))
                        return 0xaac; // hula skull
                }

                if (spawnDistance > 18)
                {
                    if (RndChance(ref rnd, 4))
                        return 0x1082; // barrens golem

                    if (RndChance(ref rnd, 4))
                        return 0x1084; // barrens iron golem

                    if (RndChance(ref rnd, 7))
                        return 0x1083; // barren vulture
                }
            }
            else if (type.HasFlag(MapElementType.Tag2))
            {
                if (wallDistance <= 1)
                {
                    if (RndChance(ref rnd, 25))
                        return 0x1081; // falling rock

                    if (RndChance(ref rnd, 40))
                        return 0xaa7; // gorge rock
                    if (RndChance(ref rnd, 10))
                        return 0xaa9; // barrens plant 1
                    if (RndChance(ref rnd, 10))
                        return 0xaaa; // barrens plant 2
                }

                if (wallDistance <= 2)
                {
                    if (RndChance(ref rnd, 10))
                        return 0xa05; // cactus
                }

                if (spawnDistance > 18)
                {
                    if (RndChance(ref rnd, 3))
                        return 0x1086; // barren tortoise

                    if (RndChance(ref rnd, 3))
                        return 0x1087; // raiding shaman

                    if (RndChance(ref rnd, 6))
                        return 0x1083; // barren vulture
                }
            }

            return 0;
        }

        private bool RndChance(ref int rnd, int chance)
        {
            rnd -= chance;
            return rnd < 0;
        }

        protected override QuestTaskSystem CreateTasks()
        {
            return new QuestTaskSystem(this, new BossTask(0x107f, boneWormSpawnPosition.ToVec2() + 1.5f), new BossTask(0x1021, bhogninSpawnPosition.ToVec2() + 1.5f));
        }

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            ApplySetPiece(bubraCamp, spawnPosition + 1, true);
            ApplySetPiece(boneWormRoom, boneWormRoomPosition + 1, false);
            ApplySetPiece(bhogninRoom, bhogninRoomPosition + 1, false);

            AddLogicMethod("remove_bhognin_doors", RemoveBhogninDoors);
        }

        private void RemoveBhogninDoors(Entity sender)
        {
            foreach (var p in bhogninDoors)
            {
                var t = tiles.GetTile(p.x, p.y);
                t.objectType = 0;
                tiles.SetTileAndBroadcast(t);
            }
        }
    }
}