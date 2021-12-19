using System;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public class UpdatedObjectStats
    {
        /// <summary>
        /// The game id of this object
        /// </summary>
        public uint gameId;

        /// <summary>
        /// Array of all object stats that are not default for this object
        /// </summary>
        public NetStat[] stats;

        public UpdatedObjectStats(uint gameId, NetStat[] stats)
        {
            this.gameId = gameId;
            this.stats = stats;
        }

        public UpdatedObjectStats(BitReader r)
        {
            Read(r);
        }

        public void Read(BitReader r)
        {
            gameId = r.ReadUInt32();
            stats = new NetStat[r.ReadUInt8()];
            for (int i = 0; i < stats.Length; i++)
                stats[i] = NetStat.ReadNetStat(r);
        }

        public void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write((byte)stats.Length);
            for (int i = 0; i < stats.Length; i++)
                stats[i].Write(w);
        }
    }
}
