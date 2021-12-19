using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public class NewObjectStats
    {
        /// <summary>
        /// The game id of this object
        /// </summary>
        public uint gameId;

        /// <summary>
        /// The type id of this object
        /// </summary>
        public ushort type;

        /// <summary>
        /// Array of all object stats that are not default for this object
        /// </summary>
        public NetStat[] stats;

        public NewObjectStats(uint gameId, ushort type, NetStat[] stats)
        {
            this.gameId = gameId;
            this.type = type;
            this.stats = stats;
        }

        public NewObjectStats(BitReader r)
        {
            Read(r);
        }

        public void Read(BitReader r)
        {
            gameId = r.ReadUInt32();
            type = r.ReadUInt16();
            stats = new NetStat[r.ReadUInt8()];
            for (int i = 0; i < stats.Length; i++)
                stats[i] = NetStat.ReadNetStat(r);
        }

        public void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write(type);
            w.Write((byte)stats.Length);
            for (int i = 0; i < stats.Length; i++)
                stats[i].Write(w);
        }
    }
}
