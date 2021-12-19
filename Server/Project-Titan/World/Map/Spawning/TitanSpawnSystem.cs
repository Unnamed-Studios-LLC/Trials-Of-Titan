using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanDatabase.Instances;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Utils;
using World.Instances.Packets;
using World.Map.Objects;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Worlds.Gates;

namespace World.Map.Spawning
{
    public class TitanSpawnSystem
    {
#if DEBUG
        private const float Major_Spawn_Delay_Sec = 0;
        private const float Lesser_Spawn_Delay_Sec = 30;
#else
        private const float Major_Spawn_Delay_Sec = 60 * 8;
        private const float Lesser_Spawn_Delay_Sec = 60 * 5;
#endif

        private class TitanSpawnDefinition
        {
            public ushort titanType;

            public ushort[] tileTypes;

            public Type gateType;

            public int level;

            public float spawnDelay;
        }

        private static TitanSpawnDefinition mannahDefinition = new TitanSpawnDefinition
        {
            titanType = 0x1076, // mannah
            gateType = typeof(MannahsFortress),
            tileTypes = new ushort[]
            {
                0xb0d, // mountain rock
                0xb1e, // full snow
            },
            level = 160
        };

        private static TitanSpawnDefinition[] majorTitans = new TitanSpawnDefinition[]
        {
            /*
            new TitanSpawnDefinition
            {
                titanType = 0x1073, // zornan
                tileTypes = new ushort[]
                {
                    0xb24, // deep lake
                    0xb25, // lake
                },
                level = 160
            },
            */
            new TitanSpawnDefinition
            {
                titanType = 0x1074, // valdok
                gateType = typeof(ValdoksForge),
                tileTypes = new ushort[]
                {
                    0xb0d, // mountain rock
                },
                level = 160
            },
            new TitanSpawnDefinition
            {
                titanType = 0x1075, // balun
                gateType = typeof(Dumir),
                tileTypes = new ushort[]
                {
                    0xb1e, // full snow
                },
                level = 160
            }
        };

        private static TitanSpawnDefinition[] lesserTitans = new TitanSpawnDefinition[]
        {
            new TitanSpawnDefinition
            {
                titanType = 0x1071, // rictorn
                gateType = typeof(RictornsGate),
                tileTypes = new ushort[]
                {
                    0xb05, // marsh grass
                },
                level = SoulGroupDefinitions.GetLevelValue(SoulGroup.DarkForest) + 20
            },
            new TitanSpawnDefinition
            {
                titanType = 0x1072, // bhognin
                tileTypes = new ushort[]
                {
                    0xb07, // desert sand
                    0xb29, // desert sand light
                },
                level = SoulGroupDefinitions.GetLevelValue(SoulGroup.Desert) + 20
            }
        };

        private class LesserTitanEnemy : Enemy
        {
            public TitanSpawnSystem spawnSystem;

            public TitanSpawnDefinition definition;

            protected override void OnDeath(Player killer)
            {
                base.OnDeath(killer);

                spawnSystem.OnLesserDeath(this, killer);
            }
        }

        private class MajorTitanEnemy : Enemy
        {
            public TitanSpawnSystem spawnSystem;

            public TitanSpawnDefinition definition;

            protected override void OnDeath(Player killer)
            {
                base.OnDeath(killer);

                spawnSystem.OnMajorDeath(this, killer);
            }

            public override void RemoveFromWorld()
            {
                base.RemoveFromWorld();
            }
        }

        private Dictionary<ushort, Int2[]> tilePositions = new Dictionary<ushort, Int2[]>();

        private HashSet<Gate> lesserGates = new HashSet<Gate>();

        private HashSet<Type> lesserGateTypes = new HashSet<Type>();

        public World world;

        private bool waitingForMajor = true;

        private Queue<TitanSpawnDefinition> majorTitanQueue;

        private DateTime majorSpawnTime = DateTime.Now.AddSeconds(Major_Spawn_Delay_Sec * Rand.FloatValue());

        private MajorTitanEnemy currentMajorTitan;

        private Gate currentMajorGate;

        private Dictionary<TitanSpawnDefinition, LesserTitanEnemy> currentLesserTitans = new Dictionary<TitanSpawnDefinition, LesserTitanEnemy>();

        private Dictionary<TitanSpawnDefinition, DateTime> lesserCooldowns = new Dictionary<TitanSpawnDefinition, DateTime>();

        public TitanSpawnSystem(World world)
        {
            this.world = world;

            AddTiles(world, GetSignificantTiles());

            majorTitanQueue = new Queue<TitanSpawnDefinition>(majorTitans.Randomize());
        }

        private HashSet<ushort> GetSignificantTiles()
        {
            var tiles = new HashSet<ushort>();
            foreach (var def in majorTitans)
                foreach (var tileType in def.tileTypes)
                    tiles.Add(tileType);
            foreach (var def in lesserTitans)
                foreach (var tileType in def.tileTypes)
                    tiles.Add(tileType);
            return tiles;
        }

        private void AddTiles(World world, HashSet<ushort> significantTiles)
        {
            var tileTypes = new Dictionary<ushort, List<Int2>>();
            for (int y = 0; y < world.height; y++)
                for (int x = 0; x < world.width; x++)
                {
                    var tile = world.tiles.GetTile(x, y);
                    if (!significantTiles.Contains(tile.tileType)) continue;
                    if (!tileTypes.TryGetValue(tile.tileType, out var list))
                    {
                        list = new List<Int2>();
                        tileTypes.Add(tile.tileType, list);
                    }
                    list.Add(new Int2(x, y));
                }
            foreach (var tileGroup in tileTypes)
                tilePositions.Add(tileGroup.Key, tileGroup.Value.ToArray());
        }

        public void Tick(ref WorldTime time)
        {
            TickMajor();
            TickLesser();
        }

        private void TickMajor()
        {
            if (!waitingForMajor) return;
            if (DateTime.Now > majorSpawnTime)
            {
                waitingForMajor = false;
                if (majorTitanQueue.Count > 0)
                {
                    SpawnMajorTitan(majorTitanQueue.Dequeue());
                }
                else
                {
                    SpawnMajorTitan(mannahDefinition);
                }
            }
        }

        private void TickLesser()
        {
            foreach (var definition in lesserTitans)
            {
                if (!lesserCooldowns.TryGetValue(definition, out var cd))
                {
                    lesserCooldowns[definition] = DateTime.Now.AddSeconds(Lesser_Spawn_Delay_Sec * Rand.FloatValue());
                    continue;
                }

                if (DateTime.Now < cd) continue;

                if (definition.gateType != null && lesserGateTypes.Contains(definition.gateType)) continue;
                if (currentLesserTitans.ContainsKey(definition)) continue;

                SpawnLesserTitan(definition);
            }
        }

        public GameObject GetQuest(Player player, out int levelRecommendation, out bool overridable)
        {
            var level = player.GetLevel();

            if (currentMajorGate != null)
            {
                if (level >= currentMajorGate.levelRecommendation)
                {
                    levelRecommendation = currentMajorGate.levelRecommendation;
                    overridable = false;
                    return currentMajorGate.portal;
                }
            }

            if (currentMajorTitan != null)
            {
                if (level >= currentMajorTitan.definition.level)
                {
                    levelRecommendation = currentMajorTitan.definition.level;
                    overridable = false;
                    return currentMajorTitan;
                }
            }

            Gate closestGate = null;
            foreach (var gate in lesserGates)
            {
                if (level >= gate.levelRecommendation)
                {
                    if (closestGate == null || gate.levelRecommendation > closestGate.levelRecommendation)
                        closestGate = gate;
                }
            }

            LesserTitanEnemy closestLesser = null;
            foreach (var lesserTitan in currentLesserTitans.Values)
            {
                if (level >= lesserTitan.definition.level)
                {
                    if (closestLesser == null || lesserTitan.definition.level > closestLesser.definition.level)
                        closestLesser = lesserTitan;
                }
            }

            if (closestGate != null && closestLesser != null)
            {
                if (closestGate.levelRecommendation < closestLesser.level)
                {
                    levelRecommendation = closestGate.levelRecommendation;
                    overridable = true;
                    return closestGate.portal;
                }
                else
                {
                    levelRecommendation = closestLesser.level;
                    overridable = true;
                    return closestLesser;
                }
            }

            if (closestLesser != null)
            {
                levelRecommendation = closestLesser.level;
                overridable = true;
                return closestLesser;
            }

            if (closestGate != null)
            {
                levelRecommendation = closestGate.levelRecommendation;
                overridable = true;
                return closestGate.portal;
            }

            levelRecommendation = 0;
            overridable = true;
            return null;
        }

        private void SpawnMajorTitan(TitanSpawnDefinition definition)
        {
            var titan = new MajorTitanEnemy();
            var info = GameData.objects[definition.titanType];
            titan.Initialize(info);
            titan.spawnSystem = this;
            titan.definition = definition;
            titan.clearable = false;

            var position = tilePositions[definition.tileTypes.Random()].Random();
            titan.position.Value = position.ToVec2() + 0.5f;

            world.objects.AddObject(titan);
            currentMajorTitan = titan;

            Log.Write("Spawned Major Titan: " + info.name, ConsoleColor.Green);
        }

        private void SpawnLesserTitan(TitanSpawnDefinition definition)
        {
            var titan = new LesserTitanEnemy();
            var info = GameData.objects[definition.titanType];
            titan.Initialize(info);
            titan.spawnSystem = this;
            titan.definition = definition;
            titan.clearable = false;

            var position = tilePositions[definition.tileTypes.Random()].Random();
            titan.position.Value = position.ToVec2() + 0.5f;

            world.objects.AddObject(titan);

            currentLesserTitans.Add(definition, titan);

            lesserCooldowns[definition] = DateTime.Now.AddSeconds(Lesser_Spawn_Delay_Sec);
        }

        private void SetWaitingForMajor()
        {
            waitingForMajor = true;
            majorSpawnTime = DateTime.Now.AddSeconds(Major_Spawn_Delay_Sec);
        }

        private void OnMajorDeath(MajorTitanEnemy titan, Player killer)
        {
            if (titan != currentMajorTitan) return;
            currentMajorTitan = null;

            var gate = CreateGate(titan.definition.gateType, titan.definition, titan.position.Value);
            if (gate == null)
            {
                SetWaitingForMajor();
            }
            else
            {
                gate.major = true;
                currentMajorGate = gate;
            }
        }

        private void OnLesserDeath(LesserTitanEnemy titan, Player killer)
        {
            currentLesserTitans.Remove(titan.definition);

            var gate = CreateGate(titan.definition.gateType, titan.definition, titan.position.Value);
            if (gate == null)
            {
                lesserCooldowns[titan.definition] = DateTime.Now.AddSeconds(Lesser_Spawn_Delay_Sec);
            }
            else
            {
                lesserGates.Add(gate);
                lesserGateTypes.Add(titan.definition.gateType);
            }
        }

        private Gate CreateGate(Type type, TitanSpawnDefinition definition, Vec2 position)
        {
            if (type == null) return null;
            var gate = (Gate)Activator.CreateInstance(type);
            gate.levelRecommendation = definition.level;
            //gate.titanSpawnSystem = this;
            gate.worldId = world.manager.GetWorldId();
            AddGate(gate, position);
            return gate;
        }

        private async void AddGate(Gate gate, Vec2 position)
        {
            await Task.Run(() =>
            {
                gate.InitWorld();
            });
            world.manager.AddWorld(gate);

            var portal = new Portal(gate.worldId);
            portal.worldName.Value = gate.WorldName;
            portal.position.Value = position;
            portal.Initialize(GameData.objects[gate.PreferredPortal]);

            gate.portal = portal;
            world.PushTickAction(() =>
            {
                world.objects.AddObject(portal);
            });
        }

        public void GateCompleted(Gate gate)
        {
            if (gate is MannahsFortress)
            {
                world.manager.module.StartShutdown();
            }
            else if (gate.major)
            {
                if (gate != currentMajorGate) return;
                Log.Write("Major Gate Completed", ConsoleColor.Green);
                SetWaitingForMajor();
                currentMajorGate = null;
            }
            else
            {
                lesserGates.Remove(gate);

                var type = gate.GetType();
                lesserGateTypes.Remove(type);

                foreach (var lesser in lesserTitans)
                    if (lesser.gateType != null && lesser.gateType.IsEquivalentTo(type))
                    {
                        lesserCooldowns[lesser] = DateTime.Now.AddSeconds(Lesser_Spawn_Delay_Sec);
                    }
            }
        }

        public void Next()
        {
            currentMajorTitan = null;
            currentMajorGate = null;

            SetWaitingForMajor();
            majorSpawnTime = DateTime.Now;
        }


        public void StartMannah()
        {
            currentMajorGate = null;
            SpawnMajorTitan(mannahDefinition);
        }
    }
}
