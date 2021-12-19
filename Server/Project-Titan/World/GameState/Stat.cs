using System;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace World.GameState
{
    public enum ObjectStatScope
    {
        Private,
        Public
    }

    public class ObjectStat<T> : ObjectStat
    {
        public T Value
        {
            get => (T)value;
            set => this.value = value;
        }

        public ObjectStat(ObjectStatType type, ObjectStatScope scope, object value, object defaultValue) : base(type, scope, value, defaultValue)
        {
        }
    }

    public class ObjectStat
    {
        /// <summary>
        /// The type of stat that is represented
        /// </summary>
        public ObjectStatType type;

        /// <summary>
        /// The scope of this stat
        /// </summary>
        public ObjectStatScope scope;

        /// <summary>
        /// The value of this stat
        /// </summary>
        protected object value;

        /// <summary>
        /// The value of this stat in the previous tick
        /// </summary>
        protected object lastValue;

        /// <summary>
        /// The default value of this stat
        /// </summary>
        protected object defaultValue;

        private bool oneSend = false;

        public NetStat NetStat => new NetStat(type, value);

        public ObjectStat(ObjectStatType type, ObjectStatScope scope, object value, object defaultValue)
        {
            this.type = type;
            this.scope = scope;
            this.value = value;
            lastValue = value;
            this.defaultValue = defaultValue;
        }

        public void SetDefault(object value)
        {
            defaultValue = value;
        }

        public void SetOneSend()
        {
            oneSend = true;
        }

        /// <summary>
        /// Determines if the stat value is equal to the default value
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return ValuesEqual(value, defaultValue);
        }

        /// <summary>
        /// Determines if the value has been updated since the last tick
        /// </summary>
        /// <returns></returns>
        public bool IsUpdated()
        {
            bool isChanged = !ValuesEqual(value, lastValue);
            lastValue = value;
            return isChanged;
        }

        public void Post()
        {
            if (!oneSend) return;
            value = defaultValue;
            lastValue = value;
        }

        private bool ValuesEqual(object a, object b)
        {
            switch (type)
            {
                case ObjectStatType.Rage:
                    return (byte)a == (byte)b;
                case ObjectStatType.Position:
                    return (Vec2)a == (Vec2)b;
                case ObjectStatType.Inventory0:
                case ObjectStatType.Inventory1:
                case ObjectStatType.Inventory2:
                case ObjectStatType.Inventory3:
                case ObjectStatType.Inventory4:
                case ObjectStatType.Inventory5:
                case ObjectStatType.Inventory6:
                case ObjectStatType.Inventory7:
                case ObjectStatType.Inventory8:
                case ObjectStatType.Inventory9:
                case ObjectStatType.Inventory10:
                case ObjectStatType.Inventory11:
                case ObjectStatType.Backpack0:
                case ObjectStatType.Backpack1:
                case ObjectStatType.Backpack2:
                case ObjectStatType.Backpack3:
                case ObjectStatType.Backpack4:
                case ObjectStatType.Backpack5:
                case ObjectStatType.Backpack6:
                case ObjectStatType.Backpack7:
                    return (Item)a == (Item)b;
                case ObjectStatType.Name:
                    return ((string)a).Equals((string)b, StringComparison.Ordinal);
                case ObjectStatType.Stopped:
                    return (bool)a == (bool)b;
                case ObjectStatType.Hover:
                case ObjectStatType.Size:
                    return (float)a == (float)b;
                case ObjectStatType.StatusEffects:
                case ObjectStatType.OwnerId:
                    return (uint)a == (uint)b;
                case ObjectStatType.PremiumCurrency:
                case ObjectStatType.DeathCurrency:
                    return (long)a == (long)b;
                case ObjectStatType.FlashColor:
                    return (GameColor)a == (GameColor)b;
                case ObjectStatType.Emote:
                    return (EmoteType)a == (EmoteType)b;
            }
            return a.Equals(b);
        }
    }
}
