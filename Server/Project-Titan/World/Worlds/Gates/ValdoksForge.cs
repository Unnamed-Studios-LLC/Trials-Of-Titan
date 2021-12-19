using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Files;
using TitanCore.Gen;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Pathfinding;
using Utils.NET.Utils;
using World.GameState;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Spawning;

namespace World.Worlds.Gates
{
    public class ValdoksForge : Gate
    {
        public override string WorldName => "Valdok's Forge";

        public override ushort PreferredPortal => 0xa55;

        public override bool LimitSight => true;

        public override int MaxPlayerCount => 30;

        protected override string DefaultMusic => "Hammer_And_Anvil";

        private static SetPiece blacksmithSetPiece = SetPiece.Load("blacksmith.mef");

        private static SetPiece valdoksBossSetPiece = SetPiece.Load("valdoksroom.mef");

        private static SetPiece valdoksSpawnSetPiece = SetPiece.Load("valdoksspawn.mef");

        private static SetPiece bothmurRoomSetPiece = SetPiece.Load("bothmurroom.mef");

        private TitanCore.Gen.Map map;

        private Int2 spawnPosition;

        private Int2 bossPosition;

        private Int2 bothmurPosition;

        private Int2 blacksmithPosition;

        private Enemy boss;

        private HashSet<Int2> bossPath;

        private IEnumerable<Int2> bossPathSequence;

        private List<Vec2> forgePiecePositions = new List<Vec2>();

        protected override MapElementFile LoadMap()
        {
            int maxLandmass = 1;
            map = GenerateShape(maxLandmass);

            var biggestArea = map.GetBiggestAreas(1, maxLandmass).First();
            spawnPosition = biggestArea.First();
            WriteSetPieceGround(map, valdoksSpawnSetPiece, spawnPosition, true);

            var time = DateTime.Now.Ticks;
            Log.Write("Generating Distance Fields...");

            var distanceField = map.GetDistanceField(biggestArea.First(), out var maxDistance);

            Log.Write($"Generated Distance Fields in {(DateTime.Now.Ticks - time) / TimeSpan.TicksPerSecond} sec.");

            bossPosition = map.RandomDistancePoint(distanceField, new Range(maxDistance - 3, int.MaxValue));
            WriteSetPieceGround(map, valdoksBossSetPiece, bossPosition, true);

            //bothmurPosition = map.RandomDistancePoint(distanceField, new Range(maxDistance * 0.7f, maxDistance * 0.85f));
            //WriteSetPieceGround(map, bothmurRoomSetPiece, bothmurPosition, true);

            for (int i = 0; i < 2; i++)
            {
                float distance = 0.25f + i * 0.2f;
                var position = map.RandomDistancePoint(distanceField, new Range(maxDistance * distance, maxDistance * (distance + 0.2f))).ToVec2() + 0.5f;
                forgePiecePositions.Add(position);
            }

            blacksmithPosition = map.RandomDistancePoint(distanceField, new Range(maxDistance * 0.45f, maxDistance * 0.6f));
            WriteSetPieceGround(map, blacksmithSetPiece, blacksmithPosition, true);

            map.Extrude(MapElementType.Wall, map.GetMasses(MapElementType.Ground));

            var wallDistanceField = map.GenerateTypeDistanceField(3, MapElementType.Wall);

            return Rasterize(map, wallDistanceField, distanceField, maxDistance);
        }

        private TitanCore.Gen.Map GenerateShape(int maxLandmass)
        {
            CellularMap genMap;
            int sizeMin = 2000;
            int sizeMax = 4000;
            int maxBreakcount = 30;

            int count = 0;
            int bc = 0;
            do
            {
                genMap = new CellularMap(80, 80, Rand.IntValue(), 0.487f, 1, 50, 50, 2, 1, 0, 0);
                genMap.Generate();
                var groundMasses = genMap.GetLandmasses(maxLandmass);

                count = 0;
                foreach (var mass in groundMasses)
                    foreach (var point in mass)
                        count++;
            }
            while ((count < sizeMin || count > sizeMax) && ++bc < maxBreakcount);

            if (bc == maxBreakcount)
            {
                Log.Write("Failed to generate within the size limit");
            }

            var clamp = genMap.Clamp();
            var map = new TitanCore.Gen.Map(clamp.width + 4, clamp.height + 4, clamp, new Int2(2, 2));
            map = map.Scale(3);
            return new TitanCore.Gen.Map(map.width + 80, map.height + 80, map);
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
            if ((type & MapElementType.BossPath) == MapElementType.BossPath) return 0xb2b;
            if (wallDistance <= 2)
            {
                if (Rand.Next(3) < 1)
                    return 0xb2a; // ground
                else
                    return 0xb2c; // cracked
            }
            else if (Rand.Next(10) < 1)
                return 0xb2c; // cracked
            else
                return 0xb2a; // ground
        }

        private ushort GetWall(int x, int y, int width, int height, MapElementType type)
        {
            return 0xa56;
        }

        private ushort GetObject(int x, int y, int width, int height, MapElementType type, int wallDistance, int spawnDistance, int maxSpawnDistance)
        {
            var rnd = Rand.Next(1000);
            if (wallDistance <= 2)
            {
                if (wallDistance == 1)
                {
                    if (RndChance(ref rnd, 30))
                        return 0xa66; // miner
                    else if (RndChance(ref rnd, 20))
                        return 0xa67; // giant miner
                }

                if (RndChance(ref rnd, 50))
                    return 0xa63; // coal
                if (RndChance(ref rnd, 20))
                    return 0xa65; // box of coal
                if (RndChance(ref rnd, 20))
                    return 0xa62; // pickaxe
            }
            else if (spawnDistance > 24)
            {
                if (RndChance(ref rnd, 4))
                    return 0x1049; // orc bladesman
                if (RndChance(ref rnd, 4))
                    return 0x104a; // orc warrior
                if (RndChance(ref rnd, 1))
                    return 0x104b; // orc buiser
                if (RndChance(ref rnd, 1))
                    return 0x104e; // orc beastmaster
                if (RndChance(ref rnd, 1))
                    return 0x104d; // orc beastmaster
                if (RndChance(ref rnd, 5))
                    return 0xa63; // coal
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

            //ApplySetPiece(bothmurRoomSetPiece, bothmurPosition + new Int2(1, 1), true);
            ApplySetPiece(blacksmithSetPiece, blacksmithPosition + new Int2(1, 1), true);
            ApplySetPiece(valdoksSpawnSetPiece, spawnPosition + new Int2(1, 1), true);
            ApplySetPiece(valdoksBossSetPiece, bossPosition + new Int2(1, 1), true);

            foreach (var position in forgePiecePositions)
            {
                if (Rand.Next(3) != 0) continue;

                var bothmur = objects.CreateEnemy(0x1050);
                bothmur.position.Value = position + 1;
                objects.AddObject(bothmur);
            }
        }

        private static ushort[] forgeTypes = new ushort[]
        {
            //0x107a,
            //0x107b,
            0x107c
        };

        private int forgeIndex = 0;

        protected override QuestTaskSystem CreateTasks()
        {
            return new QuestTaskSystem(this, new MultiEnemyTask(forgePiecePositions.Select(_ => new MultiEnemyTask.MultiEnemyTaskDesc(forgeTypes[(forgeIndex++) % forgeTypes.Length], _ + 1f)).ToArray()), new BossTask(0x1015, bossPosition));
        }
    }
}
