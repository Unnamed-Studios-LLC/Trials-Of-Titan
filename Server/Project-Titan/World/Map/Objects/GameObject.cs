using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Partitioning;
using World.GameState;
using World.Map.Chunks;
using World.Map.Objects.Entities;

namespace World.Map.Objects
{
    public abstract class GameObject : IPartitionable
    {
        public Rect BoundingRect => new Rect(position.Value.x, position.Value.y, 0, 0);

        public virtual bool Teleportable => false;

        public IntRect LastBoundingRect { get; set; }

        /// <summary>
        /// The type of this object
        /// </summary>
        public abstract GameObjectType Type { get; }

        /// <summary>
        /// Determines if tick should be called on this object
        /// </summary>
        public abstract bool Ticks { get; }

        /// <summary>
        /// The unique id to this object within the world
        /// </summary>
        public uint gameId;

        /// <summary>
        /// The info that describes this game object
        /// </summary>
        public GameObjectInfo info;

        /// <summary>
        /// The world that this object belongs to
        /// </summary>
        public World world;

        /// <summary>
        /// The position of this object in the world
        /// </summary>
        public ObjectStat<Vec2> position = new ObjectStat<Vec2>(ObjectStatType.Position, ObjectStatScope.Public, new Vec2(0, 0), new Vec2(0, 0));

        /// <summary>
        /// The position of this object in the world
        /// </summary>
        public ObjectStat<bool> spawned = new ObjectStat<bool>(ObjectStatType.Spawned, ObjectStatScope.Public, false, false);

        /// <summary>
        /// The scale/size of this object
        /// </summary>
        private ObjectStat<float> size = new ObjectStat<float>(ObjectStatType.Size, ObjectStatScope.Public, 1f, 1f);

        /// <summary>
        /// The color that this obejct is flashing
        /// </summary>
        public ObjectStat<GameColor> flashColor = new ObjectStat<GameColor>(ObjectStatType.FlashColor, ObjectStatScope.Public, GameColor.flashClear, GameColor.flashClear);

        /// <summary>
        /// The last tick id that this object ticked
        /// </summary>
        protected ulong lastTickId = 0;

        /// <summary>
        /// The tick that this object spawned in on
        /// </summary>
        protected ulong spawnTickId = 0;

        /// <summary>
        /// The time when this object was last ticked
        /// </summary>
        private double lastTime = 0;

        /// <summary>
        /// Collection of all players that are receiving updates of this object
        /// </summary>
        public HashSet<Player> playersSentTo = new HashSet<Player>();

        /// <summary>
        /// All stats that are not their default value for this object
        /// </summary>
        public NewObjectStats newObjectStats;

        /// <summary>
        /// All stats that have been updated since last tick
        /// </summary>
        public UpdatedObjectStats updatedObjectStats;

        /// <summary>
        /// True if this object has updated stats since last tick
        /// </summary>
        public bool hasUpdatedStats = false;

        /// <summary>
        /// All stat variables in the object
        /// </summary>
        private List<ObjectStat> allStats;

        private ConcurrentQueue<Action<GameObject>> tickActions = new ConcurrentQueue<Action<GameObject>>();

        public GameObject()
        {
        }

        /// <summary>
        /// Initializes the GameObject after construction
        /// </summary>
        public virtual void Initialize(GameObjectInfo info)
        {
            this.info = info;

            allStats = new List<ObjectStat>();
            GetStats(allStats);

            size.SetDefault(info.size.min);
            size.Value = info.size.min;

            spawned.SetOneSend();
        }

        protected void AddStats(IEnumerable<ObjectStat> addStats)
        {
            allStats.AddRange(addStats);
        }

        protected void RemoveStats(IEnumerable<ObjectStat> removedStats)
        {
            foreach (var remove in removedStats)
                allStats.Remove(remove);
        }

        /// <summary>
        /// Sets the game id of this object
        /// </summary>
        /// <param name="gameId"></param>
        public virtual void SetGameId(uint gameId)
        {
            this.gameId = gameId;

            newObjectStats = new NewObjectStats(gameId, info.id, new NetStat[0]);
        }

        /// <summary>
        /// Ticks the object's state forward in time
        /// </summary>
        /// <param name="time"></param>
        public void Tick(ref WorldTime time)
        {
            if (lastTickId == time.tickId) return;
            UpdateSpawned(time.tickId);
            lastTickId = time.tickId;
            lastTime = time.totalTime;
            DoTick(ref time);

            if ((lastTickId % 4) == 0)
                UpdateStats();
        }

        /// <summary>
        /// Returns the tick ID of the last time this object was ticked
        /// </summary>
        /// <returns></returns>
        public ulong GetLastTick()
        {
            return lastTickId;
        }

        /// <summary>
        /// Does the actual ticking work for the object
        /// </summary>
        /// <param name="time"></param>
        protected virtual void DoTick(ref WorldTime time)
        {
            while (tickActions.TryDequeue(out var action))
                action(this);
        }

        /// <summary>
        /// Method called when this object is added to a world
        /// </summary>
        /// <param name="world"></param>
        public virtual void OnAddToWorld()
        {

        }

        /// <summary>
        /// Method called when this object is added to a world
        /// </summary>
        /// <param name="world"></param>
        public virtual void OnRemoveFromWorld()
        {

        }

        public void PushTickAction(Action<GameObject> action)
        {
            tickActions.Enqueue(action);
        }

        /// <summary>
        /// Called when this object is processed by the player
        /// </summary>
        /// <param name="player"></param>
        public virtual void ProcessedBy(Player player)
        {
            playersSentTo.Add(player);
        }

        /// <summary>
        /// Called when this object is processed by the player
        /// </summary>
        /// <param name="player"></param>
        public virtual void RemovedBy(Player player)
        {
            playersSentTo.Remove(player);
        }

        public virtual bool CanShowTo(Player player)
        {
            return true;
        }

        /// <summary>
        /// Gets all stat variables 
        /// </summary>
        protected virtual void GetStats(List<ObjectStat> list)
        {
            list.Add(position);
            list.Add(size);
            list.Add(flashColor);
            list.Add(spawned);
        }

        public void UpdateStats(ulong tick)
        {
            if ((lastTickId % 4) != 0 || lastTickId == tick) return;
            UpdateSpawned(tick);
            lastTickId = tick;
            UpdateStats();
        }

        public void AssignSpawnTick(ulong tickId)
        {
            spawnTickId = tickId;
            /*
            var extra = tickId % 4;
            if (extra == 0)
            else
                spawnTickId = tickId + (4 - extra);
            */
        }

        public ulong GetSpawnTick() => spawnTickId;

        private void UpdateSpawned(ulong tick)
        {
            spawned.Value = (tick - spawnTickId) < 4;
        }

        /// <summary>
        /// Creates the stat arrays for this object
        /// </summary>
        protected virtual void UpdateStats()
        {
            var changedStats = new Dictionary<ObjectStatScope, List<NetStat>>();
            var updatedStats = new Dictionary<ObjectStatScope, List<NetStat>>();

            foreach (var stat in allStats)
            {
                if (stat.IsUpdated())
                {
                    if (!updatedStats.TryGetValue(stat.scope, out var updated))
                    {
                        updated = new List<NetStat>();
                        updatedStats.Add(stat.scope, updated);
                    }
                    updated.Add(stat.NetStat);
                }
                if (!stat.IsDefault())
                {
                    if (!changedStats.TryGetValue(stat.scope, out var changed))
                    {
                        changed = new List<NetStat>();
                        changedStats.Add(stat.scope, changed);
                    }
                    changed.Add(stat.NetStat);
                }
                stat.Post();
            }

            SetUpdatedStats(changedStats, updatedStats);
        }

        protected virtual void SetUpdatedStats(Dictionary<ObjectStatScope, List<NetStat>> changed, Dictionary<ObjectStatScope, List<NetStat>> updated)
        {
            newObjectStats = new NewObjectStats(gameId, info.id, new NetStat[0]);
            if (changed.TryGetValue(ObjectStatScope.Public, out var changedStats))
                newObjectStats.stats = changedStats.ToArray();

            updatedObjectStats = new UpdatedObjectStats(gameId, new NetStat[0]);
            if (updated.TryGetValue(ObjectStatScope.Public, out var updatedStats))
                updatedObjectStats.stats = updatedStats.ToArray();

            hasUpdatedStats = updatedObjectStats.stats.Length > 0;
        }

        public virtual NewObjectStats GetNewStats(Player requester)
        {
            return newObjectStats;
        }

        public virtual UpdatedObjectStats GetUpdatedStats(Player requester)
        {
            return updatedObjectStats;
        }

        public virtual void RemoveFromWorld()
        {
            world?.objects.RemoveObject(this);
        }

        public float DistanceTo(GameObject other)
        {
            return DistanceTo(other.position.Value);
        }

        public float DistanceTo(Vec2 position)
        {
            return this.position.Value.DistanceTo(position);
        }

        public float AngleTo(GameObject other)
        {
            return AngleTo(other.position.Value);
        }

        public float AngleTo(Vec2 position)
        {
            return this.position.Value.AngleTo(position);
        }

        public void SetSize(float size)
        {
            this.size.Value = size;
        }

        public float GetSize()
        {
            return size.Value;
        }

        public void PlayEffect(WorldEffect effect)
        {
            var packet = new TnPlayEffect(effect);
            foreach (var player in playersSentTo)
            {
                player.client.SendAsync(packet);
            }
        }
    }
}
