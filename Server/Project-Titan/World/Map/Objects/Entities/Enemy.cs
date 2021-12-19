using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net.Packets.Models;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Algorithms;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.GameState;
using World.Logic;
using World.Looting;
using World.Map.Objects.Map.Containers;
using World.Worlds.Gates;

namespace World.Map.Objects.Entities
{
    public class Enemy : NotPlayable
    {
        public override GameObjectType Type => GameObjectType.Enemy;

        /// <summary>
        /// The logic that this enemy runs
        /// </summary>
        private EnemyLogic logic;

        private ObjectStat<int> health = new ObjectStat<int>(ObjectStatType.Health, ObjectStatScope.Public, 100, 100);

        public ObjectStat<int> maxHealth = new ObjectStat<int>(ObjectStatType.MaxHealth, ObjectStatScope.Public, 100, 100);

        /// <summary>
        /// Dictionary containing all damage done keyed to the account id of the damager
        /// </summary>
        private Dictionary<ulong, int> damagers = new Dictionary<ulong, int>();

        public ulong spawnedBy;

        private Player closestPlayer;

        private float playerSearchRadius = 0;

        /// <summary>
        /// The leader of this enemy
        /// </summary>
        public Enemy leader;

        /// <summary>
        /// The defense of this enemy
        /// </summary>
        public int defense;

        /// <summary>
        /// The minions of this enemy
        /// </summary>
        private Dictionary<uint, Enemy> minions = new Dictionary<uint, Enemy>();

        /// <summary>
        /// Returns if this enemy is a minion
        /// </summary>
        public bool IsMinion => leader != null;

        public SoulGroup soulGroup;

        public int level;

        public bool clearable = true;

        public int baseMaxHealth;

        public override void Initialize(GameObjectInfo info)
        {
            base.Initialize(info);

            logic = new EnemyLogic(this);

            var enemyInfo = (EnemyInfo)info;
            soulGroup = enemyInfo.soulGroup;
            level = SoulGroupDefinitions.GetLevelValue(soulGroup);
            baseMaxHealth = (int)(SoulGroupDefinitions.GetMaxHealthValue(soulGroup) * enemyInfo.healthMod);
            maxHealth.Value = baseMaxHealth;
            defense = enemyInfo.defense;
            health.Value = maxHealth.Value;
        }

        public bool InState(string[] stateNames)
        {
            return logic.InState(stateNames);
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(health);
            list.Add(maxHealth);
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            if (!dead)
                logic.Tick(ref time);

            closestPlayer = null;
            playerSearchRadius = 0;

            foreach (var minion in minions.Values)
                if (minion.world != null)
                    minion.Tick(ref time);
        }

        public override Player GetClosestPlayer(float searchRadius)
        {
            if (closestPlayer != null)
            {
                var dis = closestPlayer.position.Value - position.Value;
                if (dis.SqrLength <= searchRadius * searchRadius)
                    return closestPlayer;
                else
                    return null;
            }
            else if (searchRadius <= playerSearchRadius) return closestPlayer;
            playerSearchRadius = searchRadius;
            closestPlayer = world.objects.GetClosestPlayer(position.Value, searchRadius);
            if (closestPlayer != null && closestPlayer.DistanceTo(this) > searchRadius)
                closestPlayer = null;
            return closestPlayer;
        }

        public int GetHealth()
        {
            return health.Value;
        }

        public void Shoot(ushort damage, byte index, float angle, Vec2 position)
        {
            var proj = new EnemyProjectile()
            {
                projectileId = world.GetProjectileId(1),
                ownerId = gameId,
                damage = damage,
                index = index,
                angle = angle,
                position = position
            };

            foreach (var player in playersSentTo)
                player.gameState.AddEnemyProjectile(this, proj);
        }

        public void ShootAoe(ushort damage, byte index, Vec2 target)
        {
            var proj = new EnemyAoeProjectile()
            {
                projectileId = world.GetProjectileId(1),
                ownerId = gameId,
                damage = damage,
                index = index,
                target = target
            };

            foreach (var player in playersSentTo)
                player.gameState.AddEnemyAoeProjectile(this, proj);
        }

        public override void Hurt(int damage, Entity damager)
        {
            health.Value -= damage;

            if (damager is Player player && player.client != null && player.client.account != null)
            {
                var key = player.client.account.id;
                if (!damagers.TryGetValue(key, out var damageDone))
                {
                    damageDone = 0;
                }
                damageDone += damage;
                damagers[key] = damageDone;
            }
        }

        public override int GetDefense()
        {
            return defense;
        }

        protected override void OnDeath(Player killer)
        {
            base.OnDeath(killer);

            var enemyInfo = (EnemyInfo)info;

            int index = 0;
            var damagersOrdered = new List<Damager>();
            var soulAmount = (int)(SoulGroupDefinitions.GetSoulValue(enemyInfo.soulGroup) * enemyInfo.soulMod);
            foreach (var damager in damagers.OrderByDescending(_ => _.Value))
            {
                if (!world.objects.TryGetPlayer(damager.Key, out var player))
                    continue;
                index++;
                damagersOrdered.Add(new Damager(player, damager.Value));
                var playerLevel = Math.Min(130, player.GetLevel());
                var levelScalar = 1f;
                if (playerLevel > level)
                    levelScalar = Math.Max(0.2f, (level / (float)playerLevel));
                player.GiveSouls((ulong)(soulAmount * Math.Min(1, damager.Value * 4 / (float)maxHealth.Value) * levelScalar));
            }

            logic.OnDeath(killer, damagersOrdered);

            var containers = DropTables.GetLootContainers(enemyInfo);
            var itemBags = new Dictionary<ulong, List<Item>>();
            foreach (var container in containers)
                container.OnDeath(this, killer, damagersOrdered, itemBags);

            foreach (var bag in itemBags)
                if (bag.Value.Count != 0)
                    DoCreateItems(world, position.Value, bag.Key, bag.Value);
        }

        private async void DoCreateItems(World world, Vec2 position, ulong ownerId, List<Item> items)
        {
            var serverItems = new List<ServerItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var createResponse = await Database.CreateItem(item, 0);
                if (createResponse.result != CreateItemResult.Success) continue;
                serverItems.Add(createResponse.item);
            }

            world.PushTickAction(() =>
            {
                var bag = GetLootBag(world, position, ownerId != 0, serverItems, 0);
                bag.SetOwnerId(ownerId);
                for (int i = 0; i < serverItems.Count; i++)
                {
                    var item = serverItems[i];
                    if (!bag.TryGiveItem(item)) // bag full
                    {
                        bag = GetLootBag(world, position, ownerId != 0, serverItems, i);
                        i--;
                    }
                }
            });
        }
        private ushort GetBagType(Item item, bool soulbound, out int livingTime)
        {
            var info = item.GetInfo();
            var equip = info as EquipmentInfo;
            var weapon = info as WeaponInfo;

            livingTime = 60;
            if (info is ScrollInfo scroll)
            {
                livingTime = 120;
                return 0xf05;
            }

            if (equip != null)
            {
                if (equip.tier == ItemTier.Untiered)
                {
                    livingTime = 300;
                    return 0xf07;
                }

                if (equip.slotType == SlotType.Accessory)
                {
                    switch (equip.tier)
                    {
                        case ItemTier.Tier1:
                            return 0xf02;
                        case ItemTier.Tier2:
                            return 0xf03;
                        case ItemTier.Tier3:
                            livingTime = 120;
                            return 0xf04;
                        case ItemTier.Tier4:
                            livingTime = 160;
                            return 0xf05;
                        case ItemTier.Tier5:
                            livingTime = 200;
                            return 0xf06;
                    }
                }
                else
                {
                    switch (equip.tier)
                    {
                        case ItemTier.Tier1:
                        case ItemTier.Tier2:
                            return 0xf02;
                        case ItemTier.Tier3:
                        case ItemTier.Tier4:
                        case ItemTier.Tier5:
                            return 0xf03;
                        case ItemTier.Tier6:
                        case ItemTier.Tier7:
                            livingTime = 120;
                            return 0xf04;
                        case ItemTier.Tier8:
                            livingTime = 180;
                            return 0xf05;
                        case ItemTier.Tier9:
                            livingTime = 240;
                            return 0xf06;
                        case ItemTier.Tier10:
                            livingTime = 240;
                            return 0xf09;
                    }
                }
            }

            return 0xf02;
        }

        private LootBag GetLootBag(World world, Vec2 position, bool soulbound, List<ServerItem> items, int index)
        {
            ushort type = 0xf02;
            int livingTime = 60;
            for (int i = index; i < index + 8 && i < items.Count; i++)
            {
                var item = items[i];
                var newType = GetBagType(item.itemData, soulbound, out var lt);
                if (newType <= type) continue;
                type = newType;
                livingTime = lt;
            }

            var info = GameData.objects[type];
            var bag = new LootBag();
            bag.livingTime = livingTime;
            bag.Initialize(info);
            bag.position.Value = position + Vec2.FromAngle(Rand.FloatValue() * AngleUtils.PI_2) * 0.4f;
            if (!world.tiles.CanWalk(bag.position.Value.x, bag.position.Value.y))
                bag.position.Value = position;
            world.objects.SpawnObject(bag);

            return bag;
        }

        public void ScaleHealth(int targetMaxHealth)
        {
            float increase = targetMaxHealth / (float)maxHealth.Value;
            maxHealth.Value = targetMaxHealth;
            health.Value = (int)(health.Value * increase);

            foreach (var damager in damagers.ToArray())
            {
                damagers[damager.Key] = (int)(damager.Value * increase);
            }
        }

        public override void OnRemoveFromWorld()
        {
            base.OnRemoveFromWorld();

            if (leader != null)
                leader.MinionRemoved(this);
        }

        public int GetDamageBy(ulong player)
        {
            if (damagers.TryGetValue(player, out var damage))
                return damage;
            return 0;
        }

        public override void Heal(int amount)
        {
            base.Heal(amount);

            health.Value += amount;
        }

        public override void ProcessedBy(Player player)
        {
            base.ProcessedBy(player);

            if (!dead && world is Gate gate)
                gate.ScaleEnemyHp(this);
        }

        public override void RemovedBy(Player player)
        {
            base.RemovedBy(player);

            if (!dead && world is Gate gate)
                gate.ScaleEnemyHp(this);
        }

        /// <summary>
        /// Sets this enemy's leader
        /// </summary>
        /// <param name="leader"></param>
        private void SetLeader(Enemy leader)
        {
            this.leader = leader;
        }

        /// <summary>
        /// Returns the first minion of this enemy
        /// </summary>
        /// <returns></returns>
        public Enemy GetFirstMinion()
        {
            if (minions.Count == 0) return null;
            return minions.Values.First();
        }

        /// <summary>
        /// Adds a minion to this enemy
        /// </summary>
        /// <param name="minion"></param>
        public void AddMinion(Enemy minion)
        {
            minions.Add(minion.gameId, minion);
            minion.SetLeader(this);
        }

        /// <summary>
        /// Called when a minions of this enemy dies
        /// </summary>
        /// <param name="minion"></param>
        private void MinionRemoved(Enemy minion)
        {
            minion.SetLeader(null);
            minions.Remove(minion.gameId);
        }

        /// <summary>
        /// Returns to amount of minions that are of the given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int CountMinionsOfType(ushort type)
        {
            int count = 0;
            foreach (var enemy in minions.Values)
            {
                if (enemy.info.id == type)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Returns to amount of minions
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int CountMinions()
        {
            return minions.Count;
        }

        public Enemy GetTopmostLeader( out int count)
        {
            Enemy leader = this;
            count = 0;
            while (true)
            {
                if (leader.leader == null)
                    return leader;
                count++;
                leader = leader.leader;
            }
        }

        public Enemy GetBottomMinion(out int count)
        {
            Enemy minion = this;
            count = 0;
            while (true)
            {
                var newMinion = minion.GetFirstMinion();
                if (newMinion == null)
                    return minion;
                count++;
                minion = newMinion;
            }
        }

        public IEnumerable<Enemy> GetLeaderHierarchy()
        {
            Enemy leader = this.leader;
            while (true)
            {
                if (leader == null)
                    yield break;
                yield return leader;
                leader = leader.leader;
            }
        }

        public IEnumerable<Enemy> GetMinions()
        {
            return minions.Values;
        }

        /// <summary>
        /// Returns to amount of minions
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int LeaderCount()
        {
            int count = 0;
            Enemy parent = leader;
            while (parent != null)
            {
                count++;
                parent = parent.leader;
            }
            return count;
        }

        /// <summary>
        /// Gives all minions to the leader, if exists
        /// </summary>
        public void GiveMinionsToLeader()
        {
            if (leader == null) return;
            foreach (var minion in minions.Values.ToArray())
            {
                minions.Remove(minion.gameId);
                leader.AddMinion(minion);
            }
        }
    }
}
