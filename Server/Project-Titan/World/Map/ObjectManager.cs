using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Partitioning;
using Utils.NET.Utils;
using World.GameState;
using World.Map.Chunks;
using World.Map.Objects;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;

namespace World.Map
{
    public class ObjectManager
    {
        private const int Minion_Cleanup_Missed_Ticks = WorldManager.Ticks_Per_Second * 10;

        private const int Leader_Cleanup_Missed_Ticks = WorldManager.Ticks_Per_Second * 30;

        private class SpawnDelay
        {
            public float delay;

            public GameObject obj;

            public SpawnDelay(float delay, GameObject obj)
            {
                this.delay = delay;
                this.obj = obj;
            }
        }

        /// <summary>
        /// The distance in units that the player can see
        /// </summary>
        public const float Player_Sight_Distance = 9;

        /// <summary>
        /// All objects
        /// </summary>
        private Dictionary<uint, GameObject> objects = new Dictionary<uint, GameObject>();

        /// <summary>
        /// All players
        /// </summary>
        public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

        /// <summary>
        /// All enemies
        /// </summary>
        public Dictionary<uint, Enemy> enemies = new Dictionary<uint, Enemy>();

        public List<Enemy> titans = new List<Enemy>();

        /// <summary>
        /// The next id to assign to an object
        /// </summary>
        private uint nextObjectId = 0;

        /// <summary>
        /// Chunk manager for entity objects
        /// </summary>
        private ArrayPartitionMap<Player> playerChunks;

        /// <summary>
        /// Chunk manager for entity objects
        /// </summary>
        public ArrayPartitionMap<GameObject> objectChunks;

        /// <summary>
        /// Partition map for all wall lines
        /// </summary>
        public bool[,] collision;

        /// <summary>
        /// The world
        /// </summary>
        private World world;

        /// <summary>
        /// A list of all objects spawned during a logic tick
        /// </summary>
        private List<GameObject> spawnedObjects = new List<GameObject>();

        /// <summary>
        /// A list of all objects spawned during a logic tick once their delay reaches 0
        /// </summary>
        private List<SpawnDelay> delayedSpawnedObjects = new List<SpawnDelay>();

        /// <summary>
        /// List of all objects removed during a logic tick
        /// </summary>
        private List<GameObject> removedObjects = new List<GameObject>();

        private HashSet<NotPlayable> tickedNotPlayables = new HashSet<NotPlayable>();

        public ObjectManager(World world, int width, int height)
        {
            this.world = world;

            playerChunks = new ArrayPartitionMap<Player>(width, height, 10);
            objectChunks = new ArrayPartitionMap<GameObject>(width, height, 16);

            collision = new bool[width, height];
        }

        /// <summary>
        /// Creates an enemy from a given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Enemy CreateEnemy(ushort type)
        {
            if (!GameData.objects.TryGetValue(type, out var info))
                return null;

            return CreateEnemy(info);
        }

        /// <summary>
        /// Creates an enemy from a given info
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Enemy CreateEnemy(GameObjectInfo info)
        {
            if (!(info is EnemyInfo enemyInfo))
                return null;

            var enemy = new Enemy();
            enemy.Initialize(enemyInfo);
            return enemy;
        }

        /// <summary>
        /// Called to add an object to the world during a logic tick
        /// </summary>
        /// <param name="obj"></param>
        public void SpawnObject(GameObject obj)
        {
            spawnedObjects.Add(obj);
            obj.SetGameId(nextObjectId++);
        }

        /// <summary>
        /// Called to add an object after a delay to the world during a logic tick
        /// </summary>
        /// <param name="obj"></param>
        public void SpawnObject(GameObject obj, float delay)
        {
            delayedSpawnedObjects.Add(new SpawnDelay(delay, obj));
            obj.SetGameId(nextObjectId++);
        }

        public void AssignGameId(GameObject obj)
        {
            if (obj.gameId != 0) return;
            obj.SetGameId(nextObjectId++);
        }

        /// <summary>
        /// Adds an object to the manager
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(GameObject obj)
        {
            obj.world = world;
            AssignGameId(obj);
            obj.AssignSpawnTick(world.time.tickId);
            objects.Add(obj.gameId, obj);

            switch (obj.Type)
            {
                case GameObjectType.Player:
                    var player = (Player)obj;
                    player.position.Value = world.GetRandomRegion(Region.Spawn).ToVec2() + Vec2.randomAngle * 0.5f;
                    players.Add(player.client.account.id, player);
                    playerChunks.Add(player);
                    break;
                case GameObjectType.Enemy:
                    var enemy = (Enemy)obj;
                    enemies.Add(obj.gameId, enemy);
                    objectChunks.Add(obj);
                    var enemyInfo = (EnemyInfo)enemy.info;
                    if (enemyInfo.titan)
                        titans.Add(enemy);

                    break;
                default:
                    objectChunks.Add(obj);
                    break;
            }

            obj.OnAddToWorld();
        }

        /// <summary>
        /// Removes an object from the manager
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(GameObject obj)
        {
            obj.OnRemoveFromWorld();
            obj.world = null;
            objects.Remove(obj.gameId);
            switch (obj.Type)
            {
                case GameObjectType.Player:
                    var player = obj as Player;
                    players.Remove(player.client.account.id);
                    playerChunks.Remove(player);
                    break;
                case GameObjectType.Enemy:
                    enemies.Remove(obj.gameId);
                    //enemyChunks.Remove(obj as Enemy);
                    objectChunks.Remove(obj);

                    var enemyInfo = (EnemyInfo)obj.info;
                    if (enemyInfo.titan)
                        titans.Remove((Enemy)obj);
                    break;
                default:
                    objectChunks.Remove(obj);
                    break;
            }
        }

        public void RemoveObjectPostLogic(GameObject obj)
        {
            removedObjects.Add(obj);
        }

        /// <summary>
        /// Updates an enemy's assigned partition
        /// </summary>
        /// <param name="enemy"></param>
        public void UpdateObject(GameObject enemy)
        {
            //enemyChunks.Update(enemy);
            objectChunks.Update(enemy);
        }

        /// <summary>
        /// Updates a player's assigned partition
        /// </summary>
        /// <param name="player"></param>
        public void UpdatePlayer(Player player)
        {
            playerChunks.Update(player);
        }

        /// <summary>
        /// Trys to get a player from a given account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool TryGetPlayer(ulong accountId, out Player player)
        {
            return players.TryGetValue(accountId, out player);
        }

        /// <summary>
        /// Trys to get a player from a given name (ignores case)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="outPlayer"></param>
        /// <returns></returns>
        public bool TryGetPlayer(string name, out Player outPlayer)
        {
            foreach (var player in players.Values)
                if (player.playerName.Value.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    outPlayer = player;
                    return true;
                }
            outPlayer = null;
            return false;
        }

        /// <summary>
        /// Returns all enemies within a distance from a point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public IEnumerable<Enemy> GetEnemiesWithin(float x, float y, float distance, Func<Enemy, bool> where = null)
        {
            var p = new Vec2(x, y);
            foreach (var obj in objectChunks.GetNearObjects(new Rect(x - distance / 2, y - distance / 2, distance * 2, distance * 2)))
            {
                if (!(obj is Enemy enemy)) continue;
                if (!enemy.position.Value.RadiusContains(p, distance)) continue;
                if (where != null && !where(enemy)) continue;
                yield return enemy;
            }
            //eturn playerChunks.GetWithin(x, y, distance, where);
        }

        /// <summary>
        /// Returns all players within a distance from a point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public IEnumerable<Player> GetPlayersWithin(float x, float y, float distance, Func<Player, bool> where = null)
        {
            var p = new Vec2(x, y);
            foreach (var player in playerChunks.GetNearObjects(new Rect(x - distance / 2, y - distance / 2, distance * 2, distance * 2)))
            {
                if (!player.position.Value.RadiusContains(p, distance)) continue;
                if (where != null && !where(player)) continue;
                yield return player;
            }
            //eturn playerChunks.GetWithin(x, y, distance, where);
        }

        /// <summary>
        /// Gets the closest player to the given position that is within the search radius
        /// </summary>
        /// <param name="position"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public Player GetClosestPlayer(Vec2 position, float searchRadius)
        {
            var objs = playerChunks.GetNearObjects(new Rect(position.x - searchRadius / 2, position.y - searchRadius / 2, searchRadius * 2, searchRadius * 2));
            return objs.Closest(_ => _.position.Value.SqrDistanceTo(position));
        }

        /// <summary>
        /// Gets the closest player to the given position that is within the search radius
        /// </summary>
        /// <param name="position"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        public Player GetRandomPlayer(Vec2 position, float searchRadius)
        {
            var objs = playerChunks.GetNearObjects(new Rect(position.x - searchRadius / 2, position.y - searchRadius / 2, searchRadius * 2, searchRadius * 2)).ToArray();
            if (objs.Length == 0) return null;
            return objs[Rand.Next(objs.Length)];
        }

        private Rect SightRect(Vec2 position)
        {
            return new Rect(position.x - Sight.Player_Sight_Radius, position.y - Sight.Player_Sight_Radius, Sight.Player_Sight_Radius * 2, Sight.Player_Sight_Radius * 2);
        }

        /// <summary>
        /// Ticks each object
        /// </summary>
        /// <param name="time"></param>
        public void Tick(ref WorldTime time)
        {
            foreach (var player in players.Values)
                player.client.ProcessTickPackets();

            CleanupMinions();
            TickLogic(ref time);
            TickProcessing(ref time);
        }

        public void NotPlayableTicked(NotPlayable notPlayable)
        {
            tickedNotPlayables.Add(notPlayable);
        }

        private void TickLogic(ref WorldTime time)
        {
            foreach (var player in players.Values)
            {
                player.Tick(ref time);
                var rect = SightRect(player.GetTickPosition());
                foreach (var obj in objectChunks.GetNearObjects(rect))
                {
                    if (obj.Ticks)
                        obj.Tick(ref time);
                }
            }

            if (world.AllowGlobalObjects)
                foreach (var titan in titans)
                    titan.Tick(ref time);

            foreach (var np in tickedNotPlayables)
                UpdateObject(np);
            tickedNotPlayables.Clear();

            foreach (var obj in spawnedObjects)
                AddObject(obj);
            spawnedObjects.Clear();

            for (int i = 0; i < delayedSpawnedObjects.Count; i++)
            {
                var spawnDelay = delayedSpawnedObjects[i];
                spawnDelay.delay -= (float)time.deltaTime;
                if (spawnDelay.delay <= 0)
                {
                    AddObject(spawnDelay.obj);
                    delayedSpawnedObjects.RemoveAt(i);
                    i--;
                }
            }

            foreach (var obj in removedObjects)
                RemoveObject(obj);
            removedObjects.Clear();
        }

        private void TickProcessing(ref WorldTime time)
        {
            foreach (var player in players.Values)
            {
                if (!player.startTick) return;
                bool networkTick = (time.tickId % 4) - player.tickGroup == 0;
                if (networkTick)
                    NetworkTickPlayer(ref time, player);
                player.SendTickPackets();
            }
        }

        /// <summary>
        /// Ticks the player's network states
        /// </summary>
        /// <param name="time"></param>
        /// <param name="player"></param>
        private void NetworkTickPlayer(ref WorldTime time, Player player)
        {
            player.DoSight(ref time);
            foreach (var obj in PlayerSight(time, player))
            {
                player.ProcessObject(obj, ref time);
            }

            if (world.AllowGlobalObjects)
                foreach (var titan in titans)
                    player.ProcessObject(titan, ref time);

            player.ProcessQuest(ref time);
            player.ProcessPet(ref time);
            player.SendNetworkTick();
        }

        /// <summary>
        /// Collects and ticks all object within the player's sight
        /// </summary>
        /// <param name="time"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private IEnumerable<GameObject> PlayerSight(WorldTime time, Player player)
        {
            var rect = SightRect(player.GetTickPosition());
            foreach (var otherPlayer in players.Values)
            {
                if (!otherPlayer.startTick) continue;
                yield return otherPlayer;
            }

            foreach (var obj in objectChunks.GetNearObjects(rect))
            {
                if (obj.DistanceTo(player) > Sight.Player_Sight_Radius) continue;
                yield return obj;
            }
        }

        /// <summary>
        /// Trys to retrieve an enemy given its gameId
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public bool TryGetEnemy(uint gameId, out Enemy enemy)
        {
            return enemies.TryGetValue(gameId, out enemy);
        }

        /// <summary>
        /// Trys to get an object given its gameId
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool TryGetObject(uint gameId, out GameObject gameObject)
        {
            return objects.TryGetValue(gameId, out gameObject);
        }

        /// <summary>
        /// Cleans up leftover minions
        /// </summary>
        private void CleanupMinions()
        {
            if (!world.AutoCleanupEnemies) return;
            if ((world.time.tickId % (WorldManager.Ticks_Per_Second * 5)) != 0) return;

            int minionsRemoved = 0;
            foreach (var enemy in enemies.Values)
            {
                if (!enemy.IsMinion) continue;
                if (!enemy.clearable) continue;
                if (!enemy.leader.IsDead && enemy.leader.InWorld) continue;
                var lastTick = enemy.GetLastTick();
                if (world.time.tickId - enemy.GetLastTick() < Minion_Cleanup_Missed_Ticks) continue;
                RemoveObjectPostLogic(enemy);
                minionsRemoved++;
            }

            int leadersRemoved = 0;
            foreach (var enemy in enemies.Values)
            {
                if (enemy.IsMinion) continue;
                if (!enemy.clearable) continue;
                var lastTick = enemy.GetLastTick();
                if (world.time.tickId - enemy.GetLastTick() < Leader_Cleanup_Missed_Ticks) continue;
                RemoveObjectPostLogic(enemy);
                leadersRemoved++;
            }

            if (leadersRemoved != 0 || minionsRemoved != 0)
                Log.Write($"Removed {leadersRemoved} Leaders and {minionsRemoved} Minions");
        }
    }
}
