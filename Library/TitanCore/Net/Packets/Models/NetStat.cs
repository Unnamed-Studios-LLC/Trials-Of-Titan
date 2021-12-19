using System;
using TitanCore.Core;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public struct NetStat
    {
        public static NetStat ReadNetStat(BitReader r)
        {
            var stat = new NetStat();
            stat.Read(r);
            return stat;
        }

        public ObjectStatType type;

        public object value;

        public NetStat(ObjectStatType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public void Read(BitReader r)
        {
            type = (ObjectStatType)r.ReadUInt8();
            ReadValue(r);
        }

        private void ReadValue(BitReader r)
        {
            switch (type)
            {
                case ObjectStatType.Rage:
                    value = r.ReadUInt8();
                    break;
                case ObjectStatType.Position:
                    value = new Vec2(r.ReadFloat(), r.ReadFloat());
                    break;
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
                    value = Item.ReadItem(r);
                    break;
                case ObjectStatType.MaxHealth:
                case ObjectStatType.Health:
                case ObjectStatType.Speed:
                case ObjectStatType.Attack:
                case ObjectStatType.Defense:
                case ObjectStatType.Vigor:
                case ObjectStatType.Souls:
                case ObjectStatType.Texture:
                case ObjectStatType.MaxHealthBonus:
                case ObjectStatType.SpeedBonus:
                case ObjectStatType.AttackBonus:
                case ObjectStatType.DefenseBonus:
                case ObjectStatType.VigorBonus:
                case ObjectStatType.MaxHealthLock:
                case ObjectStatType.SpeedLock:
                case ObjectStatType.AttackLock:
                case ObjectStatType.DefenseLock:
                case ObjectStatType.VigorLock:
                case ObjectStatType.Heal:
                case ObjectStatType.ServerDamage:
                case ObjectStatType.HitDamage:
                case ObjectStatType.SoulGoal:
                    value = r.ReadInt32();
                    break;
                case ObjectStatType.Name:
                    value = r.ReadUTF(40);
                    break;
                case ObjectStatType.Stopped:
                    value = r.ReadBool();
                    break;
                case ObjectStatType.Hover:
                case ObjectStatType.Size:
                    value = r.ReadFloat();
                    break;
                case ObjectStatType.StatusEffects:
                case ObjectStatType.OwnerId:
                case ObjectStatType.Target:
                    value = r.ReadUInt32();
                    break;
                case ObjectStatType.PremiumCurrency:
                case ObjectStatType.DeathCurrency:
                    value = r.ReadInt64();
                    break;
                case ObjectStatType.VaultData:
                    var vaultItems = new Item[r.ReadInt32()];
                    for (int i = 0; i < vaultItems.Length; i++)
                        vaultItems[i] = Item.ReadItem(r);
                    value = vaultItems;
                    break;
                case ObjectStatType.FlashColor:
                    value = GameColor.ReadColor(r);
                    break;
                case ObjectStatType.Emote:
                case ObjectStatType.ClassQuest:
                case ObjectStatType.Rank:
                    value = (EmoteType)r.ReadUInt8();
                    break;
                case ObjectStatType.Skin:
                case ObjectStatType.GroundObject:
                    value = r.ReadUInt16();
                    break;
            }
        }

        public void Write(BitWriter w)
        {
            w.Write((byte)type);
            WriteValue(w);
        }

        private void WriteValue(BitWriter w)
        {
            switch (type)
            {
                case ObjectStatType.Rage:
                    w.Write((byte)value);
                    break;
                case ObjectStatType.Position:
                    var vector = (Vec2)value;
                    w.Write(vector.x);
                    w.Write(vector.y);
                    break;
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
                    ((Item)value).Write(w);
                    break;
                case ObjectStatType.MaxHealth:
                case ObjectStatType.Health:
                case ObjectStatType.Speed:
                case ObjectStatType.Attack:
                case ObjectStatType.Defense:
                case ObjectStatType.Vigor:
                case ObjectStatType.Souls:
                case ObjectStatType.Texture:
                case ObjectStatType.MaxHealthBonus:
                case ObjectStatType.SpeedBonus:
                case ObjectStatType.AttackBonus:
                case ObjectStatType.DefenseBonus:
                case ObjectStatType.VigorBonus:
                case ObjectStatType.MaxHealthLock:
                case ObjectStatType.SpeedLock:
                case ObjectStatType.AttackLock:
                case ObjectStatType.DefenseLock:
                case ObjectStatType.VigorLock:
                case ObjectStatType.Heal:
                case ObjectStatType.ServerDamage:
                case ObjectStatType.HitDamage:
                case ObjectStatType.SoulGoal:
                    w.Write((int)value);
                    break;
                case ObjectStatType.Name:
                    w.Write((string)value);
                    break;
                case ObjectStatType.Stopped:
                    w.Write((bool)value);
                    break;
                case ObjectStatType.Hover:
                case ObjectStatType.Size:
                    w.Write((float)value);
                    break;
                case ObjectStatType.StatusEffects:
                case ObjectStatType.OwnerId:
                case ObjectStatType.Target:
                    w.Write((uint)value);
                    break;
                case ObjectStatType.PremiumCurrency:
                case ObjectStatType.DeathCurrency:
                    w.Write((long)value);
                    break;
                case ObjectStatType.VaultData:
                    var vaultItems = (Item[])value;
                    w.Write(vaultItems.Length);
                    for (int i = 0; i < vaultItems.Length; i++)
                        vaultItems[i].Write(w);
                    break;
                case ObjectStatType.FlashColor:
                    ((GameColor)value).Write(w);
                    break;
                case ObjectStatType.Emote:
                case ObjectStatType.ClassQuest:
                case ObjectStatType.Rank:
                    w.Write((byte)value);
                    break;
                case ObjectStatType.Skin:
                case ObjectStatType.GroundObject:
                    w.Write((ushort)value);
                    break;
            }
        }
    }
}
