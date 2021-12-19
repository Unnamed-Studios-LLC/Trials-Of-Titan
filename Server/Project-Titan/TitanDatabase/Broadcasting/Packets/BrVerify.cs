using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanDatabase.Broadcasting.Packets
{
    public class BrVerify : BrPacket
    {
        public override BrPacketType Type => BrPacketType.Verify;

        private const ulong Valid_Token = 324567329090243589;

        public ulong token;

        public string serverName;

        public BrVerify()
        {

        }

        public BrVerify(string serverName)
        {
            this.serverName = serverName;
        }

        protected override void Read(BitReader r)
        {
            serverName = r.ReadUTF(60);
            token = r.ReadUInt64();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(serverName);
            w.Write(Valid_Token);
        }

        public bool IsValid() => token == Valid_Token;
    }
}
