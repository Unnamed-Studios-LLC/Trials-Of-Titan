using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.GameState;
using World.Map.Objects.Entities;
using World.Worlds;

namespace World.Map.Spawning
{
    public class SpawnSystem
    {
        private const int Cleanup_Tick_Step = WorldManager.Ticks_Per_Second * 10;
        private const int Encounter_Tick_Step = WorldManager.Ticks_Per_Second * 1;

        private class SpawnedEnemy : Enemy
        {
            private const int Missed_Ticks_Til_Removal = WorldManager.Ticks_Per_Second * 5;

            public long positionKey;

            private TileSpawnData spawnData;

            public float spawnTime;

            public void InitSpawnData(TileSpawnData spawnData, float spawnTime, long positionKey)
            {
                this.spawnData = spawnData;
                this.spawnTime = spawnTime;
                this.positionKey = positionKey;
            }

            private bool ReadyToRemove(ulong tickId)
            {
                return (IsDead || (tickId - lastTickId) > Missed_Ticks_Til_Removal); // remove after not being updated for X ticks
            }

            public bool TryRemove(ulong tickId)
            {
                if (!ReadyToRemove(tickId)) return false;
                if (IsDead) return true;
                RemoveFromWorld();
                return true;
            }

            public override void OnRemoveFromWorld()
            {
                base.OnRemoveFromWorld();

                spawnData.OnSpawnedEnemyRemoved(positionKey);
            }
        }

        private class EncounterEnemy : Enemy
        {
            public TileSpawnData spawnData;

            protected override void OnDeath(Player killer)
            {
                base.OnDeath(killer);

                spawnData.OnEncounterKilled(this, killer, spawnData.definition.soulGroup);
            }

            public override void OnRemoveFromWorld()
            {
                base.OnRemoveFromWorld();

                spawnData.OnEncounterRemoved(this);
            }

            public void InitSpawnData(TileSpawnData spawnData)
            {
                this.spawnData = spawnData;
            }
        }

        private class TileSpawnData
        {
            private readonly SpawnSystem system;

            public readonly TileSpawnDefinition definition;

            private readonly Dictionary<long, int> spawnedEnemies = new Dictionary<long, int>();

            private readonly Dictionary<long, Queue<DateTime>> tokens = new Dictionary<long, Queue<DateTime>>();

            private readonly List<EncounterEnemy> encounters = new List<EncounterEnemy>();

            public readonly List<Int2> tiles = new List<Int2>();

            private float lastEncounterSpawn;

            public TileSpawnData(SpawnSystem system, TileSpawnDefinition definition, World world)
            {
                this.system = system;
                this.definition = definition;

                FindTiles(world);
            }

            private void FindTiles(World world)
            {
                for (int y = 0; y < world.height; y++)
                {
                    for (int x = 0; x < world.width; x++)
                    {
                        var tile = world.tiles.GetTile(x, y);
                        if (!definition.tileTypes.Contains(tile.tileType)) continue;
                        tiles.Add(new Int2(tile.x, tile.y));
                    }
                }
            }

            private bool ReadyToSpawn(long key)
            {
                if (!spawnedEnemies.TryGetValue(key, out var count))
                    count = 0;
                return count < definition.enemiesPerLanding;
            }

            public bool TryAllowSpawn(long key)
            {
                if (!ReadyToSpawn(key))
                    return false;
                if (!tokens.TryGetValue(key, out var tokenList) || tokenList.Count == 0)
                {
                    return true;
                }

                var nextTime = tokenList.Peek();
                if (DateTime.Now < nextTime)
                    return false;

                tokenList.Dequeue();
                return true;
            }

            public bool EncounterWithin(Vec2 position, float radius)
            {
                var sqr = radius * radius;
                foreach (var encounter in encounters)
                    if (encounter.position.Value.RadiusContains(position, radius))
                        return true;
                return false;
            }

            public bool ReadyToSpawnEncounter(float time)
            {
                return encounters.Count < definition.maxConcurrentEncounters && time > lastEncounterSpawn + definition.encounterSpawnRate;
            }

            public bool SpawnMob(float time, Int2 position, long key)
            {
                var type = definition.spawnables.Random();
                if (!GameData.objects.TryGetValue(type, out var info)) return false;
                if (!(info is EnemyInfo enemyInfo)) return false;
                var mob = new SpawnedEnemy();
                mob.Initialize(enemyInfo);
                mob.InitSpawnData(this, time, key);
                mob.position.Value = position.ToVec2() + 0.5f; // GetSpawnPosition(position);
                system.world.objects.AddObject(mob);
                AddMob(key);
                return true;
            }

            private void AddMob(long key)
            {
                if (spawnedEnemies.TryGetValue(key, out var count))
                    spawnedEnemies[key] = count + 1;
                else
                    spawnedEnemies.Add(key, 1);
            }

            private void RemoveMob(long key)
            {
                if (!tokens.TryGetValue(key, out var tokenList))
                {
                    tokenList = new Queue<DateTime>();
                    tokens.Add(key, tokenList);
                }

                tokenList.Enqueue(DateTime.Now.AddSeconds(definition.respawnRate));

                if (!spawnedEnemies.TryGetValue(key, out var count)) return;
                count--;
                if (count <= 0)
                    spawnedEnemies.Remove(key);
                else
                    spawnedEnemies[key] = count;
            }

            public void SpawnEncounter(float time, Int2 position)
            {
                var titanCount = system.world.overworldCycle.GetTitanCount(definition.soulGroup);
                var rndMax = definition.encounters.Length + titanCount;
                if (rndMax <= 0) return;

                var rnd = Rand.Next(rndMax);
                ushort type = 0;

                if (rnd < definition.encounters.Length)
                    type = definition.encounters.Random();
                else
                    type = system.world.overworldCycle.GetTitanSpawn(definition.soulGroup);

                if (!GameData.objects.TryGetValue(type, out var info)) return;
                if (!(info is EnemyInfo enemyInfo)) return;
                var encounter = new EncounterEnemy();
                encounter.clearable = false;
                encounter.Initialize(enemyInfo);
                encounter.InitSpawnData(this);
                encounter.position.Value = position.ToVec2() + 0.5f;
                system.world.objects.AddObject(encounter);
                encounters.Add(encounter);
                lastEncounterSpawn = time;
            }

            public Enemy SpawnMannah()
            {
                ushort type = 0x1076; // overworld mannah
                if (!GameData.objects.TryGetValue(type, out var info)) return null;
                if (!(info is EnemyInfo enemyInfo)) return null;
                var encounter = new EncounterEnemy();
                encounter.clearable = false;
                encounter.Initialize(enemyInfo);
                encounter.InitSpawnData(this);
                encounter.position.Value = tiles[Rand.Next(tiles.Count)].ToVec2() + 0.5f;
                system.world.objects.AddObject(encounter);
                return encounter;
            }

            public void GetEncounterQuest(Vec2 position, int level, ref int maxLevel, ref Enemy enemy)
            {
                var closest = encounters.Closest(_ => _.position.Value.SqrDistanceTo(position));
                if (closest == null) return;

                var enemyLevel = closest.level;
                if (enemyLevel < level)
                {
                    if (enemy == null || enemy.level < enemyLevel)
                    {
                        enemy = closest;
                        return;
                    }
                    return;
                }

                if (enemyLevel >= maxLevel) return;

                enemy = closest;
                maxLevel = enemyLevel;
            }

            /// <summary>
            /// Gets a central spawn position with emphasis towards the player
            /// </summary>
            /// <param name="definition"></param>
            /// <param name="position"></param>
            /// <returns></returns>
            private Vec2 GetSpawnPosition(Vec2 playerPosition)
            {
                var position = playerPosition.ToInt2();
                int x = position.x / definition.landingSize;
                if (x % 2 == 0)
                    position.y += definition.landingSize / 2;
                position /= definition.landingSize;
                var center = position * definition.landingSize + definition.landingSize / 2;
                return center.ToVec2() + (playerPosition - center.ToVec2()).ChangeLength(definition.landingSize * 0.25f);
            }

            public void OnSpawnedEnemyRemoved(long positionKey)
            {
                RemoveMob(positionKey);
                //spawnedEnemies.Remove(positionKey);
            }

            public void OnEncounterKilled(EncounterEnemy encounter, Player killer, SoulGroup soulGroup)
            {
                system.onEncounterKilled?.Invoke(encounter, killer, soulGroup);
            }

            public void OnEncounterRemoved(EncounterEnemy encounter)
            {
                /*
                if (encounters.Count == definition.maxConcurrentEncounters)
                {
                    lastEncounterSpawn = (float)encounter.world.time.totalTime - definition.encounterSpawnRate + 10;
                }
                */
                encounters.Remove(encounter);
            }

            public void Cleanup(ref WorldTime time)
            {
                /*
                var keys = spawnedEnemies.Keys.ToArray();
                foreach (var key in keys)
                {
                    var val = spawnedEnemies[key];
                    if ((time.totalTime - val.spawnTime) < definition.respawnRate) continue;
                    if (val.TryRemove(time.tickId))
                        spawnedEnemies.Remove(key);
                }
                */
            }
        }

        /// <summary>
        /// The world to spawn in
        /// </summary>
        private Overworld world;

        /// <summary>
        /// The spawning data for each spawn definition
        /// </summary>
        private Dictionary<TileSpawnDefinition, TileSpawnData> spawnDatas = new Dictionary<TileSpawnDefinition, TileSpawnData>();

        /// <summary>
        /// Defined zones to prevent spawning in
        /// </summary>
        private List<Circle> noSpawnZones = new List<Circle>();

        public event Action<Enemy, Player, SoulGroup> onEncounterKilled;

        private Enemy mannah;

        public SpawnSystem(Overworld world)
        {
            this.world = world;

            foreach (var definition in TileSpawnDefinition.definitions.Values)
                GetData(definition);
        }

        public void AddNoSpawnZone(Vec2 position, float radius)
        {
            AddNoSpawnZone(new Circle(position, radius));
        }

        public void AddNoSpawnZone(Circle circle)
        {
            noSpawnZones.Add(circle);
        }

        private bool OutOfBounds(Int2 position)
        {
            return position.x < 0 || position.y < 0 || position.x >= world.width || position.y >= world.height;
        }

        private bool CanSpawn(Vec2 position)
        {
            foreach (var zone in noSpawnZones)
                if (zone.Contains(position)) return false;
            foreach (var otherPlayer in world.objects.GetPlayersWithin(position.x, position.y, Sight.Player_Sight_Radius * 1.3f))
                return false;
            return true;
        }

        private bool CanSpawn(Vec2 position, Player player)
        {
            foreach (var zone in noSpawnZones)
                if (zone.Contains(position)) return false;
            foreach (var otherPlayer in world.objects.GetPlayersWithin(position.x, position.y, Sight.Player_Sight_Radius * 1.3f, _ => _ != player))
                return false;
            return true;
        }

        /// <summary>
        /// Updates the spawning system and cleans up unused enemies
        /// </summary>
        /// <param name="time"></param>
        public void Tick(ref WorldTime time)
        {
            if (time.tickId % Cleanup_Tick_Step == 0)
            {
                // Cleanup(ref time); world cleanup is already implemented
            }

            if (time.tickId % Encounter_Tick_Step == 0)
            {
                SpawnEncounters((float)time.totalTime);
            }

            foreach (var player in world.objects.players.Values)
                if ((time.tickId % 20) == player.tickGroup)
                    RunSpawn(player, (float)time.totalTime);
        }

        public void TileDiscovered(Player player, int x, int y, float time)
        {
            var position = new Vec2(x + 0.5f, y + 0.5f);
            if (time - player.lastSpawnTime < 1/* && player.lastSpawnPosition.DistanceTo(position) < 6*/ || Rand.Next(20) != 0) return;
            player.lastSpawnTime = time;
            player.lastSpawnPosition = position;

            SpawnMobTriggered(player, new Int2(x, y), time);
        }

        public Enemy GetEncounterQuest(Player player)
        {
            var position = player.position.Value;
            var level = player.GetLevel();

            if (mannah != null && !mannah.IsDead && level > 150) return mannah;

            int maxLevel = int.MaxValue;
            Enemy quest = null;

            if (!TryGetSpawnData(player, player.position.Value, out var data)) return null;

            //foreach (var data in spawnDatas.Values)
            data.GetEncounterQuest(position, level, ref maxLevel, ref quest);

            return quest;
        }

        /// <summary>
        /// Runs spawn logic on player's position
        /// </summary>
        /// <param name="player"></param>
        private void RunSpawn(Player player, float time)
        {
            SpawnMob(player, time);
        }

        private void SpawnMobTriggered(Player player, Int2 spawnPos, float time)
        {
            if (!TryGetSpawnData(player, spawnPos, out var data)) return;
            var key = GetSpawnKey(data.definition, spawnPos);
            if (!data.TryAllowSpawn(key)) return;
            data.SpawnMob(time, spawnPos, key);
        }

        private void SpawnMob(Player player, float time)
        {
            if (!TryGetSpawnData(player, out var data, out var spawnPos)) return;
            var key = GetSpawnKey(data.definition, spawnPos);
            if (!data.TryAllowSpawn(key)) return;
            data.SpawnMob(time, spawnPos, key);
        }

        private void SpawnEncounters(float time)
        {
            foreach (var data in spawnDatas.Values)
            {
                SpawnEncounter(data, time);
            }
        }

        private void SpawnEncounter(TileSpawnData data, float time)
        {
            if (!data.ReadyToSpawnEncounter(time)) return;
            if (!TryGetSpawnPosition(data, out var spawnPos)) return;
            data.SpawnEncounter(time, spawnPos);
        }

        private bool TryGetSpawnPosition(TileSpawnData data, out Int2 spawnPos)
        {
            int bc = 0;
            while (true)
            {
                spawnPos = data.tiles[Rand.Next(data.tiles.Count)];
                if (CanSpawn(spawnPos)) return true;
                if (++bc >= 10) return false;
            }
        }

        private bool TryGetSpawnData(Player player, out TileSpawnData data, out Int2 spawnPos)
        {
            var playerPos = player.position.Value;
            spawnPos = playerPos + Vec2.randomAngle * Sight.Player_Sight_Radius * 2;
            return TryGetSpawnData(player, spawnPos, out data);
        }

        private bool TryGetSpawnData(Player player, Int2 spawnPos, out TileSpawnData data)
        {
            if (OutOfBounds(spawnPos) || !CanSpawn(spawnPos, player))
            {
                data = null;
                return false;
            }

            var tile = player.world.tiles.GetTile(spawnPos.x, spawnPos.y);
            if (!TileSpawnDefinition.TryGet(tile.tileType, out var definition))
            {
                data = null;
                return false;
            }

            data = GetData(definition);
            return true;
        }

        private TileSpawnData GetData(SoulGroup soulGroup)
        {
            if (TileSpawnDefinition.TryGet(soulGroup, out var d))
                return GetData(d);
            return null;
        }

        /// <summary>
        /// Returns the spawn data object associated with the definition
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        private TileSpawnData GetData(TileSpawnDefinition definition)
        {
            if (!spawnDatas.TryGetValue(definition, out var data))
            {
                data = new TileSpawnData(this, definition, world);
                spawnDatas.Add(definition, data);
            }
            return data;
        }

        /// <summary>
        /// Returns the key for the given position and definition
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private long GetSpawnKey(TileSpawnDefinition definition, Int2 position)
        {
            int x = position.x / definition.landingSize;
            if (x % 2 == 0)
                position.y += definition.landingSize / 2;
            position /= definition.landingSize;
            return (((long)position.x) << 32) | (long)position.y;
        }

        private void Cleanup(ref WorldTime time)
        {
            foreach (var data in spawnDatas.Values)
                data.Cleanup(ref time);
        }

        public void SpawnMannah()
        {
            var data = GetData(Rand.Next(2) == 0 ? SoulGroup.Mountains : SoulGroup.Tundra);
            mannah = data.SpawnMannah();
        }
    }
}
