using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnUseItem : TnPacket, ITimePacket
    {
        public uint GetTime() => clientTickId * NetConstants.Client_Delta;

        public override TnPacketType Type => TnPacketType.UseItem;

        public uint gameId;

        public int slot;

        public uint clientTickId;

        public Vec2 position;

        public TnUseItem()
        {

        }

        public TnUseItem(uint gameId, int slot, uint clientTickId, Vec2 position)
        {
            this.gameId = gameId;
            this.slot = slot;
            this.clientTickId = clientTickId;
            this.position = position;
        }

        protected override void Read(BitReader r)
        {
            gameId = r.ReadUInt32();
            slot = r.ReadInt32();
            clientTickId = r.ReadUInt32();
            position = r.ReadVec2();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(gameId);
            w.Write(slot);
            w.Write(clientTickId);
            w.Write(position);
        }
    }
}
