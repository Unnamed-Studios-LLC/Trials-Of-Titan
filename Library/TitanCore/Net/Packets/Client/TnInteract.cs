using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnInteract : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.Interact;

        public uint clientTickId;

        public uint objectGameId;

        public Vec2 position;

        public int value;

        public TnInteract()
        {

        }

        public TnInteract(uint clientTickId, uint objectGameId, Vec2 position, int value)
        {
            this.clientTickId = clientTickId;
            this.objectGameId = objectGameId;
            this.position = position;
            this.value = value;
        }

        protected override void Read(BitReader r)
        {
            clientTickId = r.ReadUInt32();
            objectGameId = r.ReadUInt32();
            position = r.ReadVec2();
            value = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(clientTickId);
            w.Write(objectGameId);
            w.Write(position);
            w.Write(value);
        }
    }
}
