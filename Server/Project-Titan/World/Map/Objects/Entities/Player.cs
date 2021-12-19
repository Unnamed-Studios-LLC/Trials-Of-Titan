using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using TitanDatabase.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Commands;
using World.GameState;
using World.Map.Objects.Map;
using World.Net;

namespace World.Map.Objects.Entities
{
    public partial class Player : Entity
    {
        public override GameObjectType Type => GameObjectType.Player;

        public override bool Teleportable => world.AllowPlayerTeleport && startTick;

        /// <summary>
        /// The network tick group of this player
        /// </summary>
        public ulong tickGroup = 0;

        /// <summary>
        /// The game state of this player
        /// </summary>
        public PlayerGameState gameState;

        /// <summary>
        /// The client that associated with this player
        /// </summary>
        public Client client;

        /// <summary>
        /// The database character that this player represents
        /// </summary>
        public Character character;

        /// <summary>
        /// New chats to send to the player
        /// </summary>
        public List<ChatData> gameChats = new List<ChatData>();

        /// <summary>
        /// All stats that are not their default value for this object
        /// </summary>
        public NewObjectStats newObjectStatsPrivate;

        /// <summary>
        /// All stats that have been updated since last tick
        /// </summary>
        public UpdatedObjectStats updatedObjectStatsPrivate;

        /// <summary>
        /// The name of the player
        /// </summary>
        public ObjectStat<string> playerName = new ObjectStat<string>(ObjectStatType.Name, ObjectStatScope.Public, "", "");

        /// <summary>
        /// The premium currency count of this player
        /// </summary>
        public ObjectStat<long> premiumCurrency = new ObjectStat<long>(ObjectStatType.PremiumCurrency, ObjectStatScope.Private, (long)0, (long)0);

        /// <summary>
        /// The premium currency count of this player
        /// </summary>
        public ObjectStat<long> deathCurrency = new ObjectStat<long>(ObjectStatType.DeathCurrency, ObjectStatScope.Private, (long)0, (long)0);

        /// <summary>
        /// The premium currency count of this player
        /// </summary>
        public ObjectStat<int> hitDamage = new ObjectStat<int>(ObjectStatType.HitDamage, ObjectStatScope.Public, (int)0, (int)0);

        /// <summary>
        /// The amount of class quests completed
        /// </summary>
        public ObjectStat<byte> classQuests = new ObjectStat<byte>(ObjectStatType.ClassQuest, ObjectStatScope.Public, (byte)0, (byte)0);

        /// <summary>
        /// The rank of this player
        /// </summary>
        public ObjectStat<byte> rank = new ObjectStat<byte>(ObjectStatType.Rank, ObjectStatScope.Public, (byte)0, (byte)0);

        /// <summary>
        /// The rank of this player
        /// </summary>
        public ObjectStat<ushort> skin = new ObjectStat<ushort>(ObjectStatType.Skin, ObjectStatScope.Public, (ushort)0, (ushort)0);

        /// <summary>
        /// The rank of this player
        /// </summary>
        public ObjectStat<uint> target = new ObjectStat<uint>(ObjectStatType.Target, ObjectStatScope.Private, (uint)0, (uint)0);

        public Sight sight;

        public bool startTick = false;

        public uint startTickTime;

        private ClassQuest currentClassQuest;

        public override void Initialize(GameObjectInfo info)
        {
            base.Initialize(info);

            var charInfo = (CharacterInfo)info;
            sight = new Sight();

            hitDamage.SetOneSend();
        }

        public override void OnAddToWorld()
        {
            base.OnAddToWorld();

            gameState = new PlayerGameState(this, new Int2(world.width, world.height));
            gameState.projectileBlockTick = GetSpawnTick();

            lastMovePosition = position.Value;

            LoadPet();
        }

        public override void RemoveFromWorld()
        {
            base.RemoveFromWorld();

            currentTrade?.CancelTrade(this);
        }

        public void LoadAccount(Account account)
        {
            playerName.Value = account.playerName;
            rank.Value = (byte)account.rank;
            premiumCurrency.Value = account.premiumCurrency;
            deathCurrency.Value = account.deathCurrency;
            classQuests.Value = (byte)account.GetClassQuestCompletedCount();
            currentClassQuest = account.GetClassQuest((ClassType)info.id);
        }

        public void LoadCharacter(Character character)
        {
            this.character = character;

            skin.Value = character.skin;

            LoadInventory();

            LoadStats();
        }

        public void SaveCharacter()
        {
            SaveInventory();

            SaveStats();
        }

        public override void SetGameId(uint gameId)
        {
            base.SetGameId(gameId);

            newObjectStatsPrivate = new NewObjectStats(gameId, info.id, new NetStat[0]);
            updatedObjectStatsPrivate = new UpdatedObjectStats(gameId, new NetStat[0]);
        }

        public void SetSkin(ushort skin)
        {
            this.skin.Value = skin;
            character.skin = skin;
        }

        protected override void UpdateStats()
        {
            if (gameState.playerState != null)
                health.Value = (int)gameState.playerState.Health(0);

            base.UpdateStats();
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.AddRange(inventory.stats);

            list.Add(playerName);

            list.Add(health);
            list.Add(maxHealth);
            list.Add(speed);
            list.Add(attack);
            list.Add(defense);
            list.Add(vigor);

            list.Add(maxHealthBonus);
            list.Add(speedBonus);
            list.Add(attackBonus);
            list.Add(defenseBonus);
            list.Add(vigorBonus);

            list.Add(maxHealthLock);
            list.Add(speedLock);
            list.Add(attackLock);
            list.Add(defenseLock);
            list.Add(vigorLock);

            list.Add(fullSouls);
            list.Add(soulGoal);

            list.Add(premiumCurrency);
            list.Add(deathCurrency);

            list.Add(hitDamage);
            list.Add(classQuests);
            list.Add(rank);
            list.Add(skin);
            list.Add(target);
        }

        protected override void SetUpdatedStats(Dictionary<ObjectStatScope, List<NetStat>> changed, Dictionary<ObjectStatScope, List<NetStat>> updated)
        {
            base.SetUpdatedStats(changed, updated);

            changed.TryGetValue(ObjectStatScope.Public, out var pubChanged);
            updated.TryGetValue(ObjectStatScope.Public, out var pubUpdated);

            newObjectStatsPrivate = new NewObjectStats(gameId, info.id, new NetStat[0]);
            if (changed.TryGetValue(ObjectStatScope.Private, out var changedStats))
            {
                if (pubChanged != null)
                    changedStats.AddRange(pubChanged);
                newObjectStatsPrivate.stats = changedStats.ToArray();
            }
            else if (pubChanged != null)
                newObjectStatsPrivate.stats = pubChanged.ToArray();

            updatedObjectStatsPrivate = new UpdatedObjectStats(gameId, new NetStat[0]);
            if (updated.TryGetValue(ObjectStatScope.Private, out var updatedStats))
            {
                if (pubUpdated != null)
                    updatedStats.AddRange(pubUpdated);
                updatedObjectStatsPrivate.stats = updatedStats.ToArray();
            }
            else if (pubUpdated != null)
                updatedObjectStatsPrivate.stats = pubUpdated.ToArray();
        }

        public override NewObjectStats GetNewStats(Player requester)
        {
            if (requester == this)
                return newObjectStatsPrivate;
            else
                return newObjectStats;
        }

        public override UpdatedObjectStats GetUpdatedStats(Player requester)
        {
            if (requester == this)
                return updatedObjectStatsPrivate;
            else
                return updatedObjectStats;
        }

        public void DoSight(ref WorldTime time)
        {
            var tiles = new List<MapTile>();
            Int2 intPos = position.Value;
            foreach (var refPoint in sight.GetSightPoints(16, position.Value, world))
            {
                var point = intPos + refPoint;
                if (point.x < 0 || point.y < 0 || point.x >= world.width || point.y >= world.height) continue;
                world.tiles.UpdateTile(point.x, point.y, (refPoint.x == 0 && refPoint.y == 0), ref time);
                if (gameState.HasDiscoveredTile(point.x, point.y)) continue;
                var tile = world.tiles.GetTile(point.x, point.y);
                gameState.SetDiscoveredTile(point.x, point.y, true);
                world.PlayerDiscoveredTile(this, point.x, point.y);
                tiles.Add(tile);
            }

            if (tiles.Count == 0) return;

            client.SendAsync(new TnTiles(tiles.ToArray()));
        }

        public void ResetTile(int x, int y)
        {
            gameState.SetDiscoveredTile(x, y, false);
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            premiumCurrency.Value = client.account.premiumCurrency;
            deathCurrency.Value = client.account.deathCurrency;

            TickInventory(ref time);
            TickTrading(ref time);
            TickVault(ref time);
            TickQuest(ref time);
            TickPet(ref time);
            TickMovement(ref time);
        }

        /// <summary>
        /// Processes a command received from the player
        /// </summary>
        public void ProcessCommand(CommandArgs args)
        {
            var chat = CommandHandlerFactory.Handle(this, args);
            if (chat != null)
                AddChat(chat);
        }

        /// <summary>
        /// Processes the stats of the given GameObject in relation to this player
        /// </summary>
        /// <param name="gameObject"></param>
        public void ProcessObject(GameObject gameObject, ref WorldTime time)
        {
            gameState.ProcessObject(gameObject, ref time);
        }

        /// <summary>
        /// Flushes network tick data to the player
        /// </summary>
        public void SendNetworkTick()
        {
            ProcessVault();
            client.SendAsync(gameState.GetTick());
            SendChats();
        }

        /// <summary>
        /// Sends any packet that relies on tick
        /// </summary>
        public void SendTickPackets()
        {
            SendProjectiles();
        }

        /// <summary>
        /// Sends any new projectiles to the player
        /// </summary>
        private void SendProjectiles()
        {
            var projectiles = gameState.GetProjectiles();
            if (projectiles == null) return;
            client.SendAsync(projectiles);
        }

        private void SendChats()
        {
            if (gameChats.Count == 0) return;
            client.SendAsync(new TnChats(gameChats.ToArray()));
            gameChats.Clear();
        }

        /// <summary>
        /// Adds a chat to be sent to this player
        /// </summary>
        /// <param name="chatData"></param>
        public void AddChat(ChatData chatData)
        {
            gameChats.Add(chatData);
        }

        /// <summary>
        /// Handles a received command from the player
        /// </summary>
        /// <param name="chatText"></param>
        public CommandArgs CreateCommand(string chatText)
        {
            var split = chatText.Split(' ');
            var command = split[0].Substring(1);

            if (string.IsNullOrWhiteSpace(command)) return null;

            var arguments = new string[split.Length - 1];
            if (arguments.Length > 0)
                Array.Copy(split, 1, arguments, 0, arguments.Length);

            return new CommandArgs(command, arguments);
        }

        public int GetLevel()
        {
            return GetStatBase(StatType.Speed) +
                GetStatBase(StatType.Attack) +
                GetStatBase(StatType.Defense) +
                GetStatBase(StatType.Vigor) +
                GetStatBase(StatType.MaxHealth) / 10;
        }

        public override void OnRemoveFromWorld()
        {
            base.OnRemoveFromWorld();

            gameState.OnRemovedFromWorld();
            RemovePet();
        }
    }
}
