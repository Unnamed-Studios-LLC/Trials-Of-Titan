using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnQuest : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Quest;

        public uint gameId;

        public ushort objectType;

        public TnQuest()
        {

        }

        public TnQuest(uint gameId, ushort objectType)
        {
            this.gameId = gameId;
            this.objectType = objectType;
        }

        protected override void Read(BitReader r)
        {
            gameId = r.ReadUInt32();
            objectType = r.ReadUInt16();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write(objectType);
        }
    }
}
