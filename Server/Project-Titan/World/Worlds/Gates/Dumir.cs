using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Files;
using TitanCore.Gen;
using Utils.NET.Algorithms;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Spawning;

namespace World.Worlds.Gates
{
    public class Dumir : Gate
    {
        public override string WorldName => "Dumir";

        public override ushort PreferredPortal => 0xa7a;

        public override bool LimitSight => false;

        public override int MaxPlayerCount => 30;

        protected override string DefaultMusic => "Dumir";

        private static SetPiece greenVillage = SetPiece.Load("dumir/village-green.mef");

        private static SetPiece blueVillage = SetPiece.Load("dumir/village-blue.mef");

        private static SetPiece redVillage = SetPiece.Load("dumir/village-red.mef");

        private TitanCore.Gen.Map map;

        private Int2 spawnPosition;

        private Int2 greenVillagePosition;

        private Int2 redVillagePosition;

        private Int2 blueVillagePosition;

        //private Enemy oda;

        //private Enemy beorn;

        //private Enemy yolma;

        //private Enemy raeg;

        private HashSet<Int2> bridge1 = new HashSet<Int2>();

        private HashSet<Int2> bridge2 = new HashSet<Int2>();

        protected override MapElementFile LoadMap()
        {
            int maxLandmass = 3;
            map = GenerateShape(maxLandmass);

            foreach (var point in map.EachPoint())
                if (map.Get(point) == MapElementType.Empty)
                    map.Set(point, MapElementType.Ocean);

            var islands = map.GetMasses(MapElementType.Ground).OrderByDescending(_ => _.Count).ToArray();
            var islandTag = MapElementType.Tag1;
            foreach (var island in islands)
            {
                foreach (var point in island)
                {
                    for (int y = -2; y <= 2; y++)
                        for (int x = -2; x <= 2; x++)
                        {
                            var p = point + new Int2(x, y);
                            map.Set(p, map.Get(p) | islandTag);
                        }
                }
                islandTag = (MapElementType)((int)islandTag << 1);
            }

            var islandCenters = islands.Select(_ => map.GetBiggestAreas(1, 1, new Queue<Int2>[] { _ }).First().First()).ToArray();
            for (int i = 0; i < islandCenters.Length - 1; i++)
            {
                var islandCenter = islandCenters[i];
                int nextIndex = (i == islandCenters.Length - 1) ? 0 : i + 1;
                var nextCenter = islandCenters[nextIndex];

                var startTag = (MapElementType)((int)MapElementType.Tag1 << i);
                var nextTag = (MapElementType)((int)MapElementType.Tag1 << nextIndex);

                var points = i == 0 ? bridge1 : bridge2;

                for (int y = -2; y <= 2; y++)
                    for (int x = -2; x <= 2; x++)
                    {
                        var offset = new Int2(x, y);
                        foreach (var point in Bresenham.Line(islandCenter + offset, nextCenter + offset))
                        {
                            var aD = (point - islandCenter).SqrLength;
                            var bD = (point - nextCenter).SqrLength;

                            var tag = startTag;
                            if (bD < aD)
                                tag = nextTag;

                            points.Add(point);
                            //map.Set(point, map.Get(point) | MapElementType.BossPath | tag);
                        }
                    }
            }

            var greenBiggestArea = map.GetBiggestAreas(1, 1, new Queue<Int2>[] { islands[0] }).First();
            greenVillagePosition = greenBiggestArea.First();
            WriteSetPieceGround(map, greenVillage, greenVillagePosition, true);

            var blueBiggestArea = map.GetBiggestAreas(1, 1, new Queue<Int2>[] { islands[2] }).First();
            blueVillagePosition = blueBiggestArea.First();
            WriteSetPieceGround(map, blueVillage, blueVillagePosition, true);

            var redBiggestArea = map.GetBiggestAreas(1, 1, new Queue<Int2>[] { islands[1] }).First();
            redVillagePosition = redBiggestArea.First();
            WriteSetPieceGround(map, redVillage, redVillagePosition, true);

            var time = DateTime.Now.Ticks;
            Log.Write("Generating Distance Fields...");

            var greenDistanceField = map.GetDistanceField(greenVillagePosition, out var maxDistance);
            spawnPosition = map.RandomDistancePoint(greenDistanceField, new Range(maxDistance - 3, maxDistance));

            Log.Write($"Generated Distance Fields in {(DateTime.Now.Ticks - time) / TimeSpan.TicksPerSecond} sec.");

            var shoreDistanceField = map.GenerateTypeDistanceField(4, MapElementType.Ocean);
            var oceanDistanceField = map.GenerateTypeDistanceField(6, MapElementType.Ground);

            return Rasterize(map, shoreDistanceField, oceanDistanceField, greenDistanceField, maxDistance);
        }

        private TitanCore.Gen.Map GenerateShape(int maxLandmass)
        {
            CellularMap genMap;
            int sizeMin = 1500;
            int sizeMax = 2000;
            int maxBreakcount = 30;

            int count = 0;
            int bc = 0;
            bool retry = false;
            IEnumerable<Queue<Int2>> groundMasses;
            do
            {
                retry = false;
                genMap = new CellularMap(100, 100, Rand.IntValue(), 0.486f, 10, 50, 50, 3, 3, 0, 0);
                genMap.Generate();
                groundMasses = genMap.GetLandmasses(maxLandmass);

                count = 0;

                if (groundMasses.Count() < maxLandmass)
                {
                    retry = true;
                    continue;
                }

                foreach (var mass in groundMasses)
                {
                    if (mass.Count < 200)
                    {
                        retry = true;
                        break;
                    }
                    count += mass.Count;
                }
            }
            while ((count < sizeMin || count > sizeMax || retry) && ++bc < maxBreakcount);

            if (bc == maxBreakcount)
            {
                Log.Write("Failed to generate within the size limit");
            }

            var clamp = genMap.Clamp();
            var map = new TitanCore.Gen.Map(clamp.width + 4, clamp.height + 4, clamp, new Int2(2, 2));
            map = map.Scale(4);
            map.Smooth(genMap.smoothingDirections);
            return new TitanCore.Gen.Map(map.width + 60, map.height + 60, map);
        }

        private MapElementFile Rasterize(TitanCore.Gen.Map map, int[,] shoreDistanceField, int[,] oceanDistanceField, int[,] distanceField, int maxDistance)
        {
            var tiles = new MapElementFile.MapTileElement[map.width, map.height];
            foreach (var point in map.EachPoint())
            {
                var type = map.Get(point);
                ushort tileType = 0;
                ushort objectType = 0;

                var oceanDistance = oceanDistanceField[point.x, point.y];
                var shoreDistance = shoreDistanceField[point.x, point.y];
                var spawnDistance = distanceField[point.x, point.y];

                if ((type & MapElementType.Ocean) == MapElementType.Ocean && (type & MapElementType.BossPath) == 0)
                {
                    if (oceanDistance <= 6)
                    {
                        tileType = 0xb2e;
                        objectType = GetObject(point.x, point.y, width, height, type, shoreDistance, oceanDistance, spawnDistance, maxDistance);
                    }
                    else
                        tileType = 0xb31;
                }

                if ((type & MapElementType.Ground) == MapElementType.Ground)
                {
                    tileType = GetTile(point.x, point.y, map.width, map.height, type, shoreDistance, spawnDistance, maxDistance);
                    objectType = GetObject(point.x, point.y, width, height, type, shoreDistance, oceanDistance, spawnDistance, maxDistance);
                }
                else if ((type & MapElementType.BossPath) == MapElementType.BossPath)
                {
                    tileType = 0xb30;
                    objectType = GetObject(point.x, point.y, width, height, type, shoreDistance, oceanDistance, spawnDistance, maxDistance);
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

        private ushort GetTile(int x, int y, int width, int height, MapElementType type, int shoreDistance, float spawnDistance, int maxSpawnDistance)
        {
            if (type == MapElementType.Ocean)
                return 0xb2e;

            if (shoreDistance <= 4)
                return 0xb2f;
            else
                return 0xb1e;
        }

        private ushort GetWall(int x, int y, int width, int height, MapElementType type)
        {
            return 0xa56;
        }

        private ushort GetObject(int x, int y, int width, int height, MapElementType type, int shoreDistance, int oceanDistance, int spawnDistance, int maxSpawnDistance)
        {
            var rnd = Rand.Next(1000);

            if ((type & MapElementType.BossPath) == MapElementType.BossPath)
            {
                if ((type & MapElementType.Tag1) == MapElementType.Tag1)
                {
                    if (RndChance(ref rnd, 7))
                        return 0x105a;
                    if (RndChance(ref rnd, 10))
                        return 0x105b;
                }
                else if ((type & MapElementType.Tag2) == MapElementType.Tag2)
                {
                    if (RndChance(ref rnd, 8))
                        return 0x1055;
                    if (RndChance(ref rnd, 8))
                        return 0x1056;
                }
                else if ((type & MapElementType.Tag3) == MapElementType.Tag3)
                {

                }
            }
            else if ((type & MapElementType.Ocean) == MapElementType.Ocean)
            {
                if (oceanDistance <= 2)
                {
                    if ((type & MapElementType.Tag1) == MapElementType.Tag1)
                    {
                        if (RndChance(ref rnd, 25))
                            return 0xa6f;
                    }
                    else if ((type & MapElementType.Tag2) == MapElementType.Tag2)
                    {
                        if (RndChance(ref rnd, 25))
                            return 0xa78;
                    }
                    else if ((type & MapElementType.Tag3) == MapElementType.Tag3)
                    {
                        if (RndChance(ref rnd, 25))
                            return 0xa79;
                    }
                }
            }
            else
            {
                if ((type & MapElementType.Tag1) == MapElementType.Tag1 && new Int2(x, y).DistanceTo(greenVillagePosition) < 30)
                {
                    if (RndChance(ref rnd, 11))
                        return 0x105a;
                    if (RndChance(ref rnd, 14))
                        return 0x105b;
                }
                else if ((type & MapElementType.Tag2) == MapElementType.Tag2 && new Int2(x, y).DistanceTo(redVillagePosition) < 30)
                {
                    if (RndChance(ref rnd, 12))
                        return 0x1055;
                    if (RndChance(ref rnd, 12))
                        return 0x1056;
                }
                else if ((type & MapElementType.Tag3) == MapElementType.Tag3 && new Int2(x, y).DistanceTo(blueVillagePosition) < 30)
                {
                    if (RndChance(ref rnd, 12))
                        return 0x1057;
                    if (RndChance(ref rnd, 12))
                        return 0x1058;
                }

                if (Rand.Next(5) == 0 && RndChance(ref rnd, 1))
                    return 0x105c;

                if (shoreDistance <= 4)
                {
                    if ((type & MapElementType.Tag1) == MapElementType.Tag1)
                    {
                        if (RndChance(ref rnd, 10))
                            return 0xa75;
                    }
                    else if ((type & MapElementType.Tag2) == MapElementType.Tag2)
                    {
                        if (RndChance(ref rnd, 10))
                            return 0xa73;
                    }
                    else if ((type & MapElementType.Tag3) == MapElementType.Tag3)
                    {
                        if (RndChance(ref rnd, 10))
                            return 0xa74;
                    }
                }
                else
                {
                    if (RndChance(ref rnd, 5))
                        return 0xa3e; // snow rock 1
                    if (RndChance(ref rnd, 5))
                        return 0xa3f; // snow rock 2
                    if (RndChance(ref rnd, 5))
                        return 0xa40; // snow rock 3
                    if (RndChance(ref rnd, 5))
                        return 0xa41; // snow rock 4
                }
            }

            return 0;
        }

        private bool RndChance(ref int rnd, int chance)
        {
            rnd -= chance;
            return rnd < 0;
        }

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            AddRegion(Region.Spawn, spawnPosition + 1);

            AddLogicMethod("spawn_bridge_1", SpawnBridge1);
            AddLogicMethod("spawn_bridge_2", SpawnBridge2);
        }

        protected override QuestTaskSystem CreateTasks()
        {
            ApplySetPiece(greenVillage, greenVillagePosition + new Int2(1, 1), true);
            ApplySetPiece(blueVillage, blueVillagePosition + new Int2(1, 1), true);
            ApplySetPiece(redVillage, redVillagePosition + new Int2(1, 1), true);

            var odaPosition = GetRandomRegion(Region.Shop1);
            var yolmaPosition = GetRandomRegion(Region.Shop2);
            var raegPosition = GetRandomRegion(Region.Shop3);

            return new QuestTaskSystem(this, new BossTask(0x1051, odaPosition), new BossTask(0x1052, odaPosition), new BossTask(0x1053, raegPosition), new BossTask(0x1054, yolmaPosition), new BossTask(0x1010, yolmaPosition));
        }

        public void SpawnBridge1(Entity entity)
        {
            foreach (var point in bridge1)
            {
                var tile = tiles.GetTile(point.x, point.y);
                if (tile.tileType != 0xb31) continue;
                tile.tileType = 0xb30;
                tile.objectType = 0;

                var rnd = Rand.Next(1000);
                if (RndChance(ref rnd, 8))
                    tile.objectType = 0x1055;
                if (RndChance(ref rnd, 8))
                    tile.objectType = 0x1056;

                tiles.SetTileAndBroadcast(tile);
            }
        }

        public void SpawnBridge2(Entity entity)
        {
            foreach (var point in bridge2)
            {
                var tile = tiles.GetTile(point.x, point.y);
                if (tile.tileType != 0xb31) continue;
                tile.tileType = 0xb30;
                tile.objectType = 0;

                var rnd = Rand.Next(1000);
                if (RndChance(ref rnd, 8))
                    tile.objectType = 0x105a;
                if (RndChance(ref rnd, 8))
                    tile.objectType = 0x105b;

                tiles.SetTileAndBroadcast(tile);
            }
        }

        public override void Tick()
        {

            base.Tick();
        }
    }
}