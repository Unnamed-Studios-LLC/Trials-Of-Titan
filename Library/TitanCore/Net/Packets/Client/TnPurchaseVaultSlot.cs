using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnPurchaseVaultSlot : TnPacket
    {
        public override TnPacketType Type => TnPacketType.PurchaseVaultSlot;

        /// <summary>
        /// The game Id of the vault object
        /// </summary>
        public uint vaultGameId;

        public TnPurchaseVaultSlot()
        {

        }

        public TnPurchaseVaultSlot(uint vaultGameId)
        {
            this.vaultGameId = vaultGameId;
        }

        protected override void Read(BitReader r)
        {
            vaultGameId = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(vaultGameId);
        }
    }
}
