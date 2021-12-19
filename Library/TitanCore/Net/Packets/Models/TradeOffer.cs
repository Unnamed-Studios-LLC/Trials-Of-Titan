using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public struct TradeOffer
    {
        public static TradeOffer ReadOffer(BitReader r)
        {
            var offer = new TradeOffer();
            offer.Read(r);
            return offer;
        }

        private byte offer;

        public bool this[int index]
        {
            get => ((offer >> index) & 1) == 1;
            set
            {
                if (value)
                    offer |= (byte)(1 << index);
                else
                    offer &= (byte)~(1 << index);
            }
        }

        public void Read(BitReader r)
        {
            offer = r.ReadUInt8();
        }

        public void Write(BitWriter w)
        {
            w.Write(offer);
        }

        public static bool operator ==(TradeOffer a, TradeOffer b) => a.offer == b.offer;
        public static bool operator !=(TradeOffer a, TradeOffer b) => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is TradeOffer offer)
                return this == offer;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return offer.GetHashCode();
        }
    }
}
