using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnUseAbility : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.UseAbility;

        public uint clientTickId;

        public Vec2 position;

        public Vec2 target;

        public byte value;

        public TnUseAbility()
        {

        }

        public TnUseAbility(uint clientTickId, Vec2 position, Vec2 target, byte value)
        {
            this.clientTickId = clientTickId;
            this.position = position;
            this.target = target;
            this.value = value;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            position = r.ReadVec2();
            target = r.ReadVec2();
            value = r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(position);
            w.Write(target);
            w.Write(value);
        }
    }
}
