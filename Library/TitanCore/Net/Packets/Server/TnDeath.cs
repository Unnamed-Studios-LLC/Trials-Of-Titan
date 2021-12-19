using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnDeath : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Death;

        /// <summary>
        /// The type of killer
        /// </summary>
        public ushort killer;

        /// <summary>
        /// The time of death in UTC
        /// </summary>
        public DateTime deathTime;

        /// <summary>
        /// The death currency reward from death
        /// </summary>
        public long baseReward;

        /// <summary>
        /// Character stats used to calculate rewards
        /// </summary>
        public CharacterStatistic[] stats;

        public TnDeath()
        {

        }

        public TnDeath(ushort killer, DateTime deathTime, long baseReward, CharacterStatistic[] stats)
        {
            this.killer = killer;
            this.deathTime = deathTime;
            this.baseReward = baseReward;
            this.stats = stats;
        }

        protected override void Read(BitReader r)
        {
            killer = r.ReadUInt16();
            deathTime = DateTime.FromBinary(r.ReadInt64());
            baseReward = r.ReadInt64();
            stats = new CharacterStatistic[r.ReadInt32()];
            for (int i = 0; i < stats.Length; i++)
                stats[i] = new CharacterStatistic(r.ReadUInt64());
        }

        protected override void Write(BitWriter w)
        {
            w.Write(killer);
            w.Write(deathTime.ToBinary());
            w.Write(baseReward);
            w.Write(stats.Length);
            for (int i = 0; i < stats.Length; i++)
                w.Write(stats[i].ToBinary());
        }
    }
}
