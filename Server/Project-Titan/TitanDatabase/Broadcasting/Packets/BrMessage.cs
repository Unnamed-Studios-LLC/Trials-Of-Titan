using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Utils;

namespace TitanDatabase.Broadcasting.Packets
{
    public class BrMessage : BrPacket
    {
        private static TypeFactory<BrPacketType, BrPacket> factory = new TypeFactory<BrPacketType, BrPacket>(_ => _.Type);
        
        public override BrPacketType Type => BrPacketType.Message;

        public string server;

        public BrPacket packet;

        public BrMessage()
        {

        }

        public BrMessage(string server, BrPacket packet)
        {
            this.server = server;
            this.packet = packet;
        }

        protected override void Read(BitReader r)
        {
            server = r.ReadUTF(120);

            packet = factory.Create((BrPacketType)r.ReadUInt8());
            packet.ReadPacket(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(server);

            w.Write(packet.Id);
            packet.WritePacket(w);
        }
    }
}
