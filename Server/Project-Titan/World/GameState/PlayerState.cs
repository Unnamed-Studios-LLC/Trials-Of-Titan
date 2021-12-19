using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.Abilities;
using World.Map.Objects.Abilities;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Net;

namespace World.GameState
{
    public struct PlayerSnapshot
    {
        public static PlayerSnapshot GetDefault()
        {
            return new PlayerSnapshot()
            {
                maxHealth = 100,
                attack = 0,
                defense = 0,
                vigor = 0,
                radius = 0.3f,
                fullSouls = 0,
                equips = new Item[4],
                extraStats = new Dictionary<StatType, int>(),
                extraAlternateStats = new Dictionary<AlternateStatType, int>()
            };
        }

        private int maxHealth;

        private int speed;

        private int attack;

        private int defense;

        private int vigor;

        public int maxHealthBonus;

        public int speedBonus;

        public int attackBonus;

        public int defenseBonus;

        public int vigorBonus;

        public float radius;

        public int fullSouls;

        public uint target;

        public int heal;

        public int health;

        public int serverDamage;

        public uint serverEffects;

        public Item[] equips;

        public Dictionary<StatType, int> extraStats;

        public Dictionary<AlternateStatType, int> extraAlternateStats;

        public uint time;

        public PlayerSnapshot(NetStat[] stats, PlayerSnapshot previous, uint time)
        {
            this.time = time;
            maxHealth = previous.maxHealth;
            speed = previous.speed;
            attack = previous.attack;
            defense = previous.defense;
            vigor = previous.vigor;
            maxHealthBonus = previous.maxHealthBonus;
            speedBonus = previous.speedBonus;
            attackBonus = previous.attackBonus;
            defenseBonus = previous.defenseBonus;
            vigorBonus = previous.vigorBonus;
            radius = previous.radius;
            fullSouls = previous.fullSouls;
            serverEffects = previous.serverEffects;
            equips = new Item[previous.equips.Length];
            for (int i = 0; i < equips.Length; i++)
                equips[i] = previous.equips[i];

            extraStats = new Dictionary<StatType, int>(previous.extraStats);
            extraAlternateStats = new Dictionary<AlternateStatType, int>(previous.extraAlternateStats);
            target = previous.target;
            health = previous.health;
            heal = 0;
            serverDamage = 0;

            foreach (var stat in stats)
            {
                switch (stat.type)
                {
                    case ObjectStatType.Health:
                        health = (int)stat.value;
                        break;
                    case ObjectStatType.MaxHealth:
                        maxHealth = (int)stat.value;
                        break;
                    case ObjectStatType.Speed:
                        speed = (int)stat.value;
                        break;
                    case ObjectStatType.Attack:
                        attack = (int)stat.value;
                        break;
                    case ObjectStatType.Defense:
                        defense = (int)stat.value;
                        break;
                    case ObjectStatType.Vigor:
                        vigor = (int)stat.value;
                        break;
                    case ObjectStatType.MaxHealthBonus:
                        maxHealthBonus = (int)stat.value;
                        break;
                    case ObjectStatType.SpeedBonus:
                        speedBonus = (int)stat.value;
                        break;
                    case ObjectStatType.AttackBonus:
                        attackBonus = (int)stat.value;
                        break;
                    case ObjectStatType.DefenseBonus:
                        defenseBonus = (int)stat.value;
                        break;
                    case ObjectStatType.VigorBonus:
                        vigorBonus = (int)stat.value;
                        break;
                    case ObjectStatType.Souls:
                        fullSouls = (int)stat.value;
                        break;
                    case ObjectStatType.StatusEffects:
                        serverEffects = (uint)stat.value;
                        break;
                    case ObjectStatType.Target:
                        target = (uint)stat.value;
                        break;
                    case ObjectStatType.Heal:
                        heal = (int)stat.value;
                        break;
                    case ObjectStatType.ServerDamage:
                        serverDamage = (int)stat.value;
                        break;
                    case ObjectStatType.Inventory0:
                    case ObjectStatType.Inventory1:
                    case ObjectStatType.Inventory2:
                    case ObjectStatType.Inventory3:
                        var item = (Item)stat.value;
                        int index = (int)stat.type - (int)ObjectStatType.Inventory0;
                        var lastItem = equips[index];
                        if (!lastItem.IsBlank) // remove the old item's stat boosts
                        {
                            var lastItemInfo = lastItem.GetInfo();
                            if (lastItemInfo is EquipmentInfo equipInfo)
                            {
                                if (equipInfo.statIncreases.Count != 0)
                                {
                                    foreach (var increase in equipInfo.statIncreases)
                                    {
                                        var increaseAmount = extraStats[increase.Key];
                                        increaseAmount -= increase.Value;
                                        if (increaseAmount == 0)
                                            extraStats.Remove(increase.Key);
                                        else
                                            extraStats[increase.Key] = increaseAmount;
                                    }
                                }

                                if (equipInfo.alternateStatIncreases.Count != 0)
                                {
                                    foreach (var increase in equipInfo.alternateStatIncreases)
                                    {
                                        var increaseAmount = extraAlternateStats[increase.Key];
                                        increaseAmount -= increase.Value;
                                        if (increaseAmount == 0)
                                            extraAlternateStats.Remove(increase.Key);
                                        else
                                            extraAlternateStats[increase.Key] = increaseAmount;
                                    }
                                }
                            }
                        }
                        equips[index] = item;

                        if (!item.IsBlank) // add the new item's stat boosts
                        {
                            var newItemInfo = item.GetInfo();
                            if (newItemInfo is EquipmentInfo equipInfo)
                            {
                                if (equipInfo.statIncreases.Count != 0)
                                {
                                    foreach (var increase in equipInfo.statIncreases)
                                    {
                                        if (!extraStats.TryGetValue(increase.Key, out var increaseAmount))
                                            increaseAmount = 0;
                                        increaseAmount += increase.Value;
                                        extraStats[increase.Key] = increaseAmount;
                                    }
                                }

                                if (equipInfo.alternateStatIncreases.Count != 0)
                                {
                                    foreach (var increase in equipInfo.alternateStatIncreases)
                                    {
                                        if (!extraAlternateStats.TryGetValue(increase.Key, out var increaseAmount))
                                            increaseAmount = 0;
                                        increaseAmount += increase.Value;
                                        extraAlternateStats[increase.Key] = increaseAmount;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        public int GetBaseStat(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHealth:
                    return maxHealth;
                case StatType.Speed:
                    return speed;
                case StatType.Attack:
                    return attack;
                case StatType.Defense:
                    return defense;
                case StatType.Vigor:
                    return vigor;
            }
            return 0;
        }

        public int GetFunctionalStat(StatType type)
        {
            if (!extraStats.TryGetValue(type, out var amount))
                amount = 0;
            switch (type)
            {
                case StatType.MaxHealth:
                    return maxHealth + maxHealthBonus + amount;
                case StatType.Speed:
                    return speed + speedBonus + amount;
                case StatType.Attack:
                    return attack + attackBonus + amount;
                case StatType.Defense:
                    return defense + defenseBonus + amount;
                case StatType.Vigor:
                    return vigor + vigorBonus + amount;
            }
            return amount;
        }

        public int GetAlternateStat(AlternateStatType type)
        {
            if (!extraAlternateStats.TryGetValue(type, out var amount))
                amount = 0;
            return amount;
        }

        public bool HasServerEffect(StatusEffect effect)
        {
            return ((serverEffects >> (int)effect) & 1) == 1;
        }
    }

    public class PlayerState
    {
        /// <summary>
        /// The currrent snapshot
        /// </summary>
        public PlayerSnapshot currentSnapshot;

        /// <summary>
        /// The health of the player
        /// </summary>
        private float health;

        /// <summary>
        /// The last time the health was advanced
        /// </summary>
        private uint lastHealthTime;

        /// <summary>
        /// The amount of rage the player has
        /// </summary>
        public byte rage;

        /// <summary>
        /// The next time an ability is available to use
        /// </summary>
        public uint nextAbility;

        /// <summary>
        /// Status effects that were applied by the client
        /// </summary>
        private Dictionary<StatusEffect, StatusEffectTime> clientEffects = new Dictionary<StatusEffect, StatusEffectTime>();

        private Player player;

        private CharacterInfo classInfo;

        public uint positionalEffectStartTime;

        public Vec2 positionalEffectPosition;

        public Vec2 positionalEffectVector;

        public bool positionalEffectCollided = false;

        private uint currentMoveCheckTime = 0;

        private Vec2 currentMoveCheckPosition;

        private float currentMoveTps = 0;

        private bool wasSlowed = false;

        private bool wasWasSlowed = false;

        private bool wasSpeedy = false;

        private bool wasWasSpeedy = false;

        private float lastTps;

        public ClassAbility ability;

        public PlayerState(uint time, Player player, NewObjectStats newObj)
        {
            this.player = player;
            currentSnapshot = new PlayerSnapshot(newObj.stats, PlayerSnapshot.GetDefault(), time);
            health = currentSnapshot.health;
            lastHealthTime = time;
            classInfo = (CharacterInfo)GameData.objects[newObj.type];
            currentMoveCheckTime = time;
            currentMoveCheckPosition = player.position.Value;
            currentMoveTps = GetTargetTps(time);
            lastTps = currentMoveTps;

            ability = ClassAbility.GetAbility((ClassType)player.info.id);
            ability.SetPlayer(player);
        }

        public void StartHealth(uint time)
        {
            lastHealthTime = time;
        }

        public void PushUpd(uint time, UpdatedObjectStats updStats)
        {
            AdvanceHealth(time - Client.Client_Fixed_Delta);
            currentSnapshot = new PlayerSnapshot(updStats.stats, currentSnapshot, time);
            wasSlowed = HasClientEffect(StatusEffect.Slowed, time);
            wasSpeedy = HasClientEffect(StatusEffect.Speedy, time);
            health = Math.Min(health + currentSnapshot.heal - currentSnapshot.serverDamage, currentSnapshot.GetFunctionalStat(StatType.MaxHealth));
            if (health <= 0)
            {
                Die(player.lastServerDamager);
            }

            currentSnapshot.heal = 0;
            currentSnapshot.serverDamage = 0;
            AdvanceHealth(time);
        }

        private float GetTargetTps(uint time)
        {
            var slowed = HasEffect(StatusEffect.Slowed, time);
            var speedy = HasEffect(StatusEffect.Speedy, time);

            var speed = currentSnapshot.GetFunctionalStat(StatType.Speed);
            var tps = StatFunctions.TilesPerSecond(speed,
                wasWasSlowed && wasSlowed && slowed,
                wasWasSpeedy || wasSpeedy || speedy);

            wasWasSlowed = slowed;
            wasWasSpeedy = speedy;

            return tps;
        }

        public void DidGoto(Vec2 position, uint time)
        {
            currentMoveCheckTime = time;
            currentMoveCheckPosition = position;

            var tps = GetTargetTps(time);
            currentMoveTps = tps;
            lastTps = tps;
        }

        public bool AdvancePosition(Vec2 position, uint time)
        {
            if (time == currentMoveCheckTime)
            {
                return position.DistanceTo(currentMoveCheckPosition) < 1;
            }

            if (!HasEffect(StatusEffect.Charmed, currentMoveCheckTime) && !HasEffect(StatusEffect.Charmed, time))
            {
                if (TryGetStatusEffectPosition(currentMoveCheckTime, time, out var effectPosition, out currentMoveCheckTime))
                {
                    currentMoveCheckPosition = effectPosition;
                    if (currentMoveCheckTime == time)
                    {
                        return position.DistanceTo(effectPosition) < 1;
                    }
                }

                var timeDif = time - currentMoveCheckTime;
                var tps = GetTargetTps(time);
                if (tps != lastTps)
                {
                    currentMoveTps = tps;
                    lastTps = tps;
                }
                var realizedTps = currentMoveCheckPosition.DistanceTo(position) / (timeDif / 1000f);

                currentMoveTps += (realizedTps - currentMoveTps) * 0.08f;

                if (currentMoveTps > tps * 1.1f + 0.1f)
                {
                    player.client.SendAsync(new TnError("Movement check failed! Moving too fast."));
                    //player.client.Disconnect();
                    return false;
                }
                else
                {

                }
            }

            currentMoveCheckPosition = position;
            currentMoveCheckTime = time;
            return true;
        }

        public float Health(uint time)
        {
            AdvanceHealth(time);
            return health;
        }

        public void Damage(uint time, int damage, GameObjectInfo damagerInfo, uint damagerId)
        {
            AdvanceHealth(time);
            var damageTaken = GetDamageTaken(damage, time);
            var previousHp = health;
            health -= damageTaken;
            player.hitDamage.Value += damageTaken;

            if (health <= 0 && previousHp > 0)
            {
                Die(damagerInfo);
                if (damagerId != 0)
                {
                    if (player.world.objects.TryGetEnemy(damagerId, out var damager))
                        damager.emote.Value = EmoteType.F;
                }
            }
        }

        public void Die(GameObjectInfo damagerInfo)
        {
            player.Die(damagerInfo);
        }

        public int GetDamageTaken(int damage, uint time)
        {
            var defense = currentSnapshot.GetFunctionalStat(StatType.Defense);
            return StatFunctions.DamageTaken(defense, damage, HasEffect(StatusEffect.Fortified, time));
        }

        private void AdvanceHealth(uint time)
        {
            while (lastHealthTime < time)
            {
                lastHealthTime += NetConstants.Client_Delta;
                var regen = StatFunctions.HealthRegen(currentSnapshot.GetFunctionalStat(StatType.Vigor), NetConstants.Client_Delta, HasEffect(StatusEffect.Healing, lastHealthTime), HasEffect(StatusEffect.Sick, lastHealthTime));
                health += regen;

                if (health > currentSnapshot.GetFunctionalStat(StatType.MaxHealth))
                    health = currentSnapshot.GetFunctionalStat(StatType.MaxHealth);

                //Log.Write(lastHealthTime / NetConstants.Client_Delta);
                //Log.Write(health, ConsoleColor.Green);
            }
        }

        public void UseAbility(uint time, Vec2 position, Vec2 target, byte value)
        {
            if (time < nextAbility)
            {
                //Log.Write("Ability is not able to be used! Ability is still on cooldown");
                player.client.SendAsync(new TnError("Ability is not able to be used! Ability is still on cooldown"));
                return;
            }

            if (HasPositionalEffect(time))
            {
                //Log.Write("Ability used during a positional effect!");
                player.client.SendAsync(new TnError("Ability used during a positional effect!"));
                return;
            }

            if (rage == 0 || (player.info.id == (ushort)ClassType.Lancer && rage < AbilityFunctions.Lancer.Rage_Cost))
            {
                //Log.Write("Ability is not able to be used! Not enough rage is available");
                player.client.SendAsync(new TnError("Ability is not able to be used! Not enough rage is available"));
                return;
            }

            int attack = currentSnapshot.GetFunctionalStat(StatType.Attack);

            var worldEffectPacket = ability.UseAbility(time, position, target, value, attack, ref rage, out var rageCost, out var sendToSelf, out var failed);

            if (failed)
            {
                player.client.SendAsync(new TnError("Failed to use ability"));
                return;
            }

            //Log.Write($"Rage: {rage}");

            var effects = AbilityFunctions.GetAbilityEffects(rageCost, attack, value, (ClassType)classInfo.id);

            for (int i = 0; i < effects.Count; i++) // add to client
            {
                var effect = effects[i];
                //Log.Write($"Added Effect: {effect.type}, Duration: {effect.duration}");
                AddClientStatusEffect(effect.type, time, effect.duration);
            }

            foreach (var otherPlayer in player.playersSentTo)
            {
                if (!sendToSelf && otherPlayer == player) continue;
                for (int i = 0; i < effects.Count; i++) // add to client
                {
                    var effect = effects[i];
                    if (otherPlayer.DistanceTo(position) < effect.area)
                        otherPlayer.AddEffect(effect.type, effect.duration / 1000f);
                }

                if (worldEffectPacket != null)
                    otherPlayer.client.SendAsync(worldEffectPacket);
            }

            nextAbility = time + (uint)AbilityFunctions.GetAbilityCooldownMs(rageCost, classInfo.id);
        }

        public bool IsInvincible(uint time)
        {
            return HasClientEffect(StatusEffect.Dashing, time) || HasEffect(StatusEffect.Invincible, time) || HasClientEffect(StatusEffect.KnockedBack, time) || HasClientEffect(StatusEffect.Grounded, time);
        }

        private bool HasPositionalEffect(uint time)
        {
            return HasClientEffect(StatusEffect.Charmed, time) || HasClientEffect(StatusEffect.Dashing, time) || HasClientEffect(StatusEffect.KnockedBack, time) || HasClientEffect(StatusEffect.Grounded, time);
        }

        public bool TryGetStatusEffectPosition(uint fromTime, uint toTime, out Vec2 position, out uint afterTime)
        {
            for (uint time = fromTime; time < toTime; time += NetConstants.Client_Delta)
            {
                if (HasClientEffect(StatusEffect.Charmed, time))
                {
                    position = GetPositionalEffectPosition(fromTime, toTime, StatusEffect.Charmed, out afterTime);
                    return true;
                }
                if (HasClientEffect(StatusEffect.Dashing, time))
                {
                    position = GetPositionalEffectPosition(fromTime, toTime, StatusEffect.Dashing, out afterTime);
                    return true;
                }
                if (HasClientEffect(StatusEffect.KnockedBack, time))
                {
                    position = GetPositionalEffectPosition(fromTime, toTime, StatusEffect.KnockedBack, out afterTime);
                    return true;
                }
                if (HasClientEffect(StatusEffect.Grounded, time))
                {
                    position = GetPositionalEffectPosition(fromTime, toTime, StatusEffect.Grounded, out afterTime);
                    return true;
                }
            }

            afterTime = fromTime;
            position = Vec2.zero;
            return false;
        }

        private Vec2 GetPositionalEffectPosition(uint fromTime, uint toTime, StatusEffect effect, out uint time)
        {
            for (time = Math.Max(fromTime, positionalEffectStartTime); time < toTime; time += NetConstants.Client_Delta)
            {
                if (!HasClientEffect(effect, time)) return positionalEffectPosition;

                if (!positionalEffectCollided)
                {
                    var newPos = positionalEffectPosition + positionalEffectVector;
                    if (!PlayerCanWalk(newPos))
                        positionalEffectCollided = true;
                    else
                        positionalEffectPosition = newPos;
                }
            }
            return positionalEffectPosition;
        }

        private bool PlayerCanWalk(Vec2 position)
        {
            return player.world.tiles.PlayerCanWalk(position.x, position.y);
        }

        public void AddCharmed(Vec2 position, Vec2 charmerPosition, uint time, uint duration)
        {
            if (HasPositionalEffect(time)) return;
            positionalEffectCollided = false;
            positionalEffectPosition = position;
            positionalEffectStartTime = time + NetConstants.Client_Delta;
            positionalEffectVector = StatusEffectFunctions.GetCharmedPositionVector(position, charmerPosition) * (NetConstants.Client_Delta / 1000f);
            AddClientStatusEffect(StatusEffect.Charmed, time, duration);
        }

        public void AddDashing(Vec2 position, Vec2 target, uint time, int rage)
        {
            if (HasPositionalEffect(time)) return;
            positionalEffectCollided = false;
            positionalEffectPosition = position;
            positionalEffectStartTime = time + NetConstants.Client_Delta;
            positionalEffectVector = AbilityFunctions.BladeWeaver.GetDashPositionVector(position.AngleTo(target), rage) * (NetConstants.Client_Delta / 1000f);
            AddClientStatusEffect(StatusEffect.Dashing, time, AbilityFunctions.BladeWeaver.Dash_Duration);
        }

        public void AddKnockedBack(Vec2 position, Vec2 knockerPosition, uint time, uint duration)
        {
            if (HasPositionalEffect(time)) return;
            positionalEffectCollided = false;
            positionalEffectPosition = position;
            positionalEffectStartTime = time + NetConstants.Client_Delta;
            positionalEffectVector = StatusEffectFunctions.GetKnockedBackPositionVector(position, knockerPosition) * (NetConstants.Client_Delta / 1000f);
            AddClientStatusEffect(StatusEffect.KnockedBack, time, duration);
        }

        public void AddGrounded(Vec2 position, uint time, uint duration)
        {
            if (HasPositionalEffect(time)) return;
            positionalEffectCollided = false;
            positionalEffectPosition = position;
            positionalEffectStartTime = time + NetConstants.Client_Delta;
            positionalEffectVector = Vec2.zero;
            AddClientStatusEffect(StatusEffect.Grounded, time, duration);
        }

        private uint GetClientEffectEndTime(StatusEffect effect)
        {
            if (clientEffects.TryGetValue(effect, out var value))
                return value.endTime;
            return 0;
        }

        public void AddClientStatusEffect(StatusEffect effect, uint time, uint duration)
        {
            AdvanceHealth(time);
            uint newEndTime = time + duration;
            if (clientEffects.TryGetValue(effect, out var effectTime))
            {
                if (newEndTime < effectTime.endTime) return;
                effectTime.endTime = newEndTime;
                clientEffects[effect] = effectTime;
            }
            else
                clientEffects.Add(effect, new StatusEffectTime(time, newEndTime));
        }

        public bool HasClientEffect(StatusEffect effect, uint time)
        {
            if (clientEffects.TryGetValue(effect, out var effectTime))
            {
                return effectTime.HasEffect(time);
            }

            return false;
        }

        public bool HasEffect(StatusEffect effect, uint time)
        {
            return HasClientEffect(effect, time) || currentSnapshot.HasServerEffect(effect);
        }

        public void AddRage(uint time, int amount = 1)
        {
            if (HasEffect(StatusEffect.Mundane, time)) return;
            rage = (byte)Math.Min(rage + amount, 100);
        }
    }
}
