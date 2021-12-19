using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnAscendStat : TnPacket
    {
        public override TnPacketType Type => TnPacketType.AscendStat;

        public uint tableGameId;

        public StatType statType;

        public TnAscendStat(uint tableGameId, StatType statType)
        {
            this.tableGameId = tableGameId;
            this.statType = statType;
        }

        public TnAscendStat()
        {

        }

        protected override void Read(BitReader r)
        {
            tableGameId = r.ReadUInt32();
            statType = (StatType)r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(tableGameId);
            w.Write((byte)statType);
        }
    }
}
