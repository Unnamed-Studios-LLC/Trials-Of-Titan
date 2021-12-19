using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnReconnect : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Reconnect;

        public string name;

        public string host;

        public ulong key;

        public uint worldId;

        public TnReconnect()
        {

        }

        public TnReconnect(string name, string host, ulong key, uint worldId)
        {
            this.name = name;
            this.host = host;
            this.key = key;
            this.worldId = worldId;
        }

        protected override void Read(BitReader r)
        {
            name = r.ReadUTF(240);
            host = r.ReadUTF(240);
            key = r.ReadUInt64();
            worldId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(name);
            w.Write(host);
            w.Write(key);
            w.Write(worldId);
        }
    }
}
