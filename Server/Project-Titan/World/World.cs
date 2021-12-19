using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Files;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TitanDatabase.Models;
using Utils.NET.Collections;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Map.Objects.Map.Containers;
using World.Net;

namespace World
{
    public class World
    {
        private class PlayerLogout
        {
            public Player player;

            public Action<bool> callback;

            public PlayerLogout(Player player, Action<bool> callback)
            {
                this.player = player;
                this.callback = callback;
            }
        }

        public virtual bool LimitSight => true;

        public virtual bool KeyedAccess => true;

        public virtual bool AllowPlayerTeleport => false;

        public virtual bool AllowGlobalObjects => false;

        public virtual ushort PreferredPortal => 0xa22;

        protected virtual string MapFile => "";

        public virtual string WorldName => "World";

        protected virtual string DefaultMusic => "Sanctuary";

        private string currentMusic;

        public virtual int MaxPlayerCount => 70;

        public virtual bool AutoCleanupEnemies => false;

        public ConcurrentQueue<Action> tickActions = new ConcurrentQueue<Action>();

        private ConcurrentExpirationQueue<ulong> joinKeyExpirationQueue = new ConcurrentExpirationQueue<ulong>(5);

        private ConcurrentDictionary<ulong, ulong> joinKeys = new ConcurrentDictionary<ulong, ulong>();

        private ConcurrentDictionary<ulong, ulong> accToKey = new ConcurrentDictionary<ulong, ulong>();

        /// <summary>
        /// The time that this world started
        /// </summary>
        public double startTime;

        /// <summary>
        /// The time of the world
        /// </summary>
        public WorldTime time;

        /// <summary>
        /// The id of the world
        /// </summary>
        public uint worldId;

        /// <summary>
        /// The width of the world
        /// </summary>
        public int width;

        /// <summary>
        /// The height of the world
        /// </summary>
        public int height;

        /// <summary>
        /// Manager of all objects within the world
        /// </summary>
        public ObjectManager objects;

        /// <summary>
        /// Manager of this worlds tiles
        /// </summary>
        public TileManager tiles;

        /// <summary>
        /// The world manager
        /// </summary>
        public WorldManager manager;

        /// <summary>
        /// Queue used to put player in game
        /// </summary>
        private ConcurrentQueue<Player> playerQueue = new ConcurrentQueue<Player>();

        /// <summary>
        /// Queue used to remove players from the game
        /// </summary>
        private ConcurrentQueue<PlayerLogout> logoutQueue = new ConcurrentQueue<PlayerLogout>();

        /// <summary>
        /// The seed for the random
        /// </summary>
        private int randomSeed;

        /// <summary>
        /// The random array for damage calc
        /// </summary>
        public double[] randArray;

        /// <summary>
        /// The next projectile id to send
        /// </summary>
        private uint nextProjectileId = 1;

        /// <summary>
        /// List of all regions within the world
        /// </summary>
        private Dictionary<Region, List<Int2>> regions = new Dictionary<Region, List<Int2>>();

        /// <summary>
        /// Per-world defined methods that can be called by a logic action
        /// </summary>
        private Dictionary<string, Action<Entity>> logicMethods = new Dictionary<string, Action<Entity>>();

        private GameObjectInfo vaultInfo = null;

        private Vec2 vaultPosition;

        public int playerCount;

        private bool initiated = false;

        public void InitWorld()
        {
            if (initiated) return;
            initiated = true;

            DoInitWorld();
        }

        /// <summary>
        /// Initializes the world
        /// </summary>
        protected virtual void DoInitWorld()
        {
            var map = LoadMap();
            width = map.width + 2;
            height = map.height + 2;

            randomSeed = Rand.IntValue();
            MakeRandArray();

            objects = new ObjectManager(this, width, height);
            CreateEntities(map, new Int2(1, 1));

            tiles = new TileManager(this, map.width, map.height, map.tiles);

            foreach (var region in map.regions)
                AddRegion(region.regionType, new Int2((int)region.x + 1, (int)region.y + 1));

            currentMusic = DefaultMusic;
        }

        protected void AddLogicMethod(string key, Action<Entity> action)
        {
            logicMethods.Add(key, action);
        }

        public void RunLogicMethod(string key, Entity entity)
        {
            if (!logicMethods.TryGetValue(key, out var action)) return;
            action.Invoke(entity);
        }

        private void CreateEntities(MapElementFile map, Vec2 offset)
        {
            for (int y = 0; y < map.height; y++)
                for (int x = 0; x < map.width; x++)
                {
                    var objs = map.tiles[x, y];
                    if (objs.objectType == 0) continue;
                    if (!GameData.objects.TryGetValue(objs.objectType, out var info)) continue;
                    switch (info.Type)
                    {
                        case GameObjectType.VaultChest:
                            vaultInfo = info;
                            vaultPosition = new Vec2(x + 0.5f, y + 0.5f) + offset;
                            objs.objectType = 0;
                            map.tiles[x, y] = objs;
                            break;
                        case GameObjectType.Wardrobe:

                            var wardrobe = new Wardrobe();
                            wardrobe.position.Value = new Vec2(x + 0.5f, y + 0.5f) + offset;
                            wardrobe.Initialize(info);
                            objects.AddObject(wardrobe);

                            objs.objectType = 0;
                            map.tiles[x, y] = objs;
                            break;
                        case GameObjectType.Npc:

                            var npc = new Npc();
                            npc.position.Value = new Vec2(x + 0.5f, y + 0.5f) + offset;
                            npc.Initialize(info);
                            objects.AddObject(npc);

                            objs.objectType = 0;
                            map.tiles[x, y] = objs;
                            break;
                        case GameObjectType.Enemy:

                            var enemy = objects.CreateEnemy(info);
                            enemy.position.Value = new Vec2(x + 0.5f, y + 0.5f) + offset;
                            enemy.Initialize(info);
                            objects.AddObject(enemy);

                            objs.objectType = 0;
                            map.tiles[x, y] = objs;
                            break;
                    }
                }
        }

        /// <summary>
        /// Makes an array of random values used to player calc damage
        /// </summary>
        private void MakeRandArray()
        {
            var random = new Random(randomSeed);
            randArray = new double[500];
            for (int i = 0; i < randArray.Length; i++)
            {
                randArray[i] = random.NextDouble();
            }
        }

        /// <summary>
        /// Returns the random value at a given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double RandValue(uint index)
        {
            return randArray[index % randArray.Length];
        }

        /// <summary>
        /// Loads the map resource
        /// </summary>
        protected virtual MapElementFile LoadMap()
        {
            var map = MapElementFile.ReadFrom("Map/Files/" + MapFile);
            return map;
        }

        public void ChangeMusic(string music)
        {
            currentMusic = music;
            var pkt = new TnChangeMusic(music);
            foreach (var player in objects.players.Values)
                player.client.SendAsync(pkt);
        }

        /// <summary>
        /// Ticks the world's objects and behaviours
        /// </summary>
        public virtual void Tick()
        {
            AddPlayers();
            RemovePlayers();

            while (tickActions.TryDequeue(out var action))
                action();

            objects.Tick(ref time);
            playerCount = objects.players.Count;

            foreach (var expired in joinKeyExpirationQueue.GetExpired())
            {
                joinKeys.TryRemove(expired, out var accId);
                accToKey.TryRemove(accId, out var d);
            }
        }

        public void PushTickAction(Action action)
        {
            tickActions.Enqueue(action);
        }

        protected virtual bool IsAccessKeyValid(ulong accessKey, ulong accountId)
        {
            if (!KeyedAccess) return true;
            if (joinKeys.TryGetValue(accessKey, out var accId))
            {
                if (accId == accountId)
                {
                    accToKey.TryRemove(accountId, out var k);
                    joinKeys.TryRemove(accessKey, out accId);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool TryGenerateKey(ulong accountId, out ulong key)
        {
            if (!KeyedAccess)
            {
                key = 0;
                return true;
            }

            if (accToKey.TryGetValue(accountId, out key))
                return true;

            if (playerCount + joinKeys.Count >= MaxPlayerCount)
            {
                key = 0;
                return false;
            }

            var a = (ulong)Rand.IntValue();
            var b = (ulong)Rand.IntValue();
            key = a | (b << 32);
            if (!joinKeys.TryAdd(key, accountId))
            {
                return false;
            }
            accToKey[accountId] = key;
            joinKeyExpirationQueue.Enqueue(key);
            return true;
        }

        public bool TryEnter(ulong accessKey, Character character, Client client, out Player player)
        {
            if (!IsAccessKeyValid(accessKey, client.account.id) ||
                !GameData.objects.TryGetValue(character.type, out var objectInfo) ||
                !(objectInfo is CharacterInfo characterInfo))
            {
                player = null;
                return false;
            }

            player = new Player();
            player.client = client;
            player.Initialize(characterInfo);
            player.LoadAccount(client.account);
            player.LoadCharacter(character);
            playerQueue.Enqueue(player);

            return true;
        }

        public void LogoutPlayer(Player player, Action<bool> callback)
        {
            logoutQueue.Enqueue(new PlayerLogout(player, callback));
        }

        private void AddPlayers()
        {
            while (playerQueue.TryDequeue(out var player))
                AddPlayer(player);
        }

        private void AddPlayer(Player player)
        {
            objects.AddObject(player);
            AssignVault(player);
            player.client.SendAsync(new TnMapInfo(player.gameId, currentMusic, WorldName, width, height, randomSeed, AllowPlayerTeleport, MaxPlayerCount));
        }
        
        private void AssignVault(Player player)
        {
            if (vaultInfo == null) return;
            var vault = new Vault(player.client.account);
            vault.Initialize(vaultInfo);
            vault.position.Value = vaultPosition;
            objects.AssignGameId(vault);
            player.SetVault(vault);
        }

        private void RemovePlayers()
        {
            while (logoutQueue.TryDequeue(out var player))
                RemovePlayer(player);
        }

        private void RemovePlayer(PlayerLogout logout)
        {
            if (logout.player.world != this)
            {
                logout.callback?.Invoke(false);
                return;
            }

            logout.player.client.Logout(logout.callback);
        }

        public uint GetProjectileId(uint amount)
        {
            var id = nextProjectileId;
            nextProjectileId += amount;
            return id;
        }

        public void ApplySetPiece(SetPiece setPiece, Int2 position, bool centered = false)
        {
            if (centered)
                position -= new Int2(setPiece.file.width / 2, setPiece.file.height / 2);

            CreateEntities(setPiece.file, position);
            for (int y = 0; y < setPiece.file.height; y++)
                for (int x = 0; x < setPiece.file.width; x++)
                {
                    var worldPos = position + new Int2(x, y);
                    var tile = setPiece.file.tiles[x, y];
                    var worldTile = tiles.GetTile(worldPos.x, worldPos.y);
                    if (tile.tileType > 0)
                    {
                        worldTile.tileType = tile.tileType;
                        worldTile.objectType = tile.objectType;
                    }
                    else if (tile.objectType > 0)
                        worldTile.objectType = tile.objectType;
                    tiles.SetTile(worldTile);
                }

            foreach (var region in setPiece.file.regions)
                AddRegion(region.regionType, position + new Int2((int)region.x, (int)region.y));
        }

        protected void AddRegion(Region region, Int2 position)
        {
            if (!regions.TryGetValue(region, out var list))
            {
                list = new List<Int2>();
                regions.Add(region, list);
            }
            list.Add(position);
        }

        public List<Int2> GetRegions(Region region)
        {
            if (regions.TryGetValue(region, out var list))
                return list;
            return null;
        }

        public Vec2 GetClosestRegion(Region regionType, Vec2 position)
        {
            var point = new Vec2(0, 0);
            float distance = 99999999999;
            var regions = GetRegions(regionType);
            if (regions != null)
                foreach (var region in regions)
                {
                    var d = position.SqrDistanceTo(region.ToVec2() + 0.5f);
                    if (d < distance)
                    {
                        point = region.ToVec2() + 0.5f;
                        distance = d;
                    }
                }
            return point;
        }

        public Int2 GetRandomRegion(Region region)
        {
            var regions = GetRegions(region);
            if (regions == null) return new Int2();
            return regions[Rand.Next(regions.Count)];
        }

        public virtual void PlayerDiscoveredTile(Player player, int x, int y)
        {

        }

        public virtual void AssignQuest(Player player)
        {

        }
    }
}
