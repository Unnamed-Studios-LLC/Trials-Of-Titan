using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Net;

namespace World.Instances.Packets
{
    public class InWorldKeyRequest : InPacket, ITokenPacket
    {
        public override InPacketType Type => InPacketType.WorldKeyRequest;

        public int Token { get; set; }

        public bool TokenResponse { get; set; }

        public ulong accountId;

        public uint worldId;

        public InWorldKeyRequest(ulong accountId, uint worldId)
        {
            this.accountId = accountId;
            this.worldId = worldId;
        }

        public InWorldKeyRequest()
        {

        }

        protected override void Read(BitReader r)
        {
            accountId = r.ReadUInt64();
            worldId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(accountId);
            w.Write(worldId);
        }
    }
}
