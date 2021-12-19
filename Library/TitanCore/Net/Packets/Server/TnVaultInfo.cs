using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnVaultInfo : TnPacket
    {
        public override TnPacketType Type => TnPacketType.VaultInfo;

        public Item[] vaultSlots;

        public TnVaultInfo()
        {

        }

        public TnVaultInfo(Item[] vaultSlots)
        {
            this.vaultSlots = vaultSlots;
        }

        protected override void Read(BitReader r)
        {
            vaultSlots = new Item[r.ReadInt32()];
            for (int i = 0; i < vaultSlots.Length; i++)
                vaultSlots[i] = Item.ReadItem(r);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(vaultSlots.Length);
            for (int i = 0; i < vaultSlots.Length; i++)
                vaultSlots[i].Write(w);
        }
    }
}
