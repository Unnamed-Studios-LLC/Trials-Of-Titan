using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using World.GameState;

namespace World.Map.Objects.Entities
{
    public abstract class Entity : GameObject
    {
        public override bool Ticks => true;

        private Vec2 lastPosition;

        private ObjectStat<bool> stopped = new ObjectStat<bool>(ObjectStatType.Stopped, ObjectStatScope.Public, false, false);

        private ObjectStat<float> hover = new ObjectStat<float>(ObjectStatType.Hover, ObjectStatScope.Public, 0.0f, 0.0f);

        private ObjectStat<int> heal = new ObjectStat<int>(ObjectStatType.Heal, ObjectStatScope.Public, 0, 0);

        private ObjectStat<int> serverDamage = new ObjectStat<int>(ObjectStatType.ServerDamage, ObjectStatScope.Public, 0, 0);

        public ObjectStat<uint> statusEffects = new ObjectStat<uint>(ObjectStatType.StatusEffects, ObjectStatScope.Public, (uint)0, (uint)0);

        private ObjectStat<int> texture = new ObjectStat<int>(ObjectStatType.Texture, ObjectStatScope.Public, 0, 0);

        private ObjectStat<ushort> groundObject = new ObjectStat<ushort>(ObjectStatType.GroundObject, ObjectStatScope.Public, (ushort)0, (ushort)0);

        /// <summary>
        /// Emote
        /// </summary>
        public ObjectStat<EmoteType> emote = new ObjectStat<EmoteType>(ObjectStatType.Emote, ObjectStatScope.Public, EmoteType.None, EmoteType.None);

        /// <summary>
        /// The durations of the status effects
        /// </summary>
        private Dictionary<StatusEffect, float> statusEffectDurations = new Dictionary<StatusEffect, float>();

        protected bool dead = false;

        public Vec2 spawn;

        private bool setSpawn = false;

        protected override void UpdateStats()
        {
            var pos = position.Value;
            stopped.Value = position.Value == lastPosition;
            lastPosition = pos;

            if (!setSpawn)
            {
                setSpawn = true;
                spawn = pos;
            }

            base.UpdateStats();
        }

        public override void Initialize(GameObjectInfo info)
        {
            base.Initialize(info);

            var entInfo = (EntityInfo)info;
            hover.SetDefault(entInfo.hover);
            hover.Value = entInfo.hover;

            texture.SetDefault(info.startTexture);
            texture.Value = info.startTexture;

            groundObject.SetDefault(info.groundObject);
            groundObject.Value = info.groundObject;

            heal.SetOneSend();
            serverDamage.SetOneSend();
            emote.SetOneSend();
        }

        protected override void GetStats(List<ObjectStat> list)
        {
            base.GetStats(list);

            list.Add(stopped);
            list.Add(hover);
            list.Add(statusEffects);
            list.Add(heal);
            list.Add(texture);
            list.Add(emote);
            list.Add(serverDamage);
            list.Add(groundObject);
        }

        public void SetTexture(int texture)
        {
            this.texture.Value = texture;
        }

        public void SetGroundObject(ushort groundObject)
        {
            this.groundObject.Value = groundObject;
        }

        public int GetDamageTaken(int damage)
        {
            return StatFunctions.DamageTaken(GetDefense(), damage, HasServerEffect(StatusEffect.Fortified));
        }

        public virtual int GetDefense()
        {
            return 0;
        }

        public bool IsDead => dead;

        public bool InWorld => world != null;

        public void Die(Player killer)
        {
            if (dead) return;
            dead = true;
            OnDeath(killer);
            RemoveFromWorld();
        }

        public virtual void Heal(int amount)
        {
            if (amount <= 0) return;
            heal.Value += amount;
        }

        public virtual void ServerDamage(int amount, GameObjectInfo damager)
        {
            serverDamage.Value += amount;
        }

        protected virtual void OnDeath(Player killer)
        {

        }

        public virtual Player GetClosestPlayer(float searchRadius)
        {
            return world.objects.GetClosestPlayer(position.Value, searchRadius);
        }

        protected override void DoTick(ref WorldTime time)
        {
            base.DoTick(ref time);

            TickEffects((float)time.totalTime);
        }

        public abstract void Hurt(int damage, Entity damager);

        public void SetHover(float hover)
        {
            this.hover.Value = hover;
        }

        public bool HasServerEffect(StatusEffect effect)
        {
            if (!statusEffectDurations.TryGetValue(effect, out var time))
                return false;
            return time > world.time.totalTime;
        }

        public void AddEffect(StatusEffect effect, float duration)
        {
            statusEffectDurations[effect] = (float)world.time.totalTime + duration;
        }

        public void RemoveEffect(StatusEffect effect)
        {
            statusEffectDurations.Remove(effect);
        }

        private void TickEffects(float time)
        {
            if (statusEffectDurations.Count == 0)
            {
                statusEffects.Value = 0;
                return;
            }

            uint effects = 0;
            foreach (var effectPair in statusEffectDurations)
            {
                if (effectPair.Value > time)
                    effects |= (uint)1 << (int)effectPair.Key;
            }
            statusEffects.Value = effects;
        }
    }
}
