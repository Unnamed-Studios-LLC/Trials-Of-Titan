using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Net.Tcp;

namespace Utils.NET.Net
{
    public interface IPacketHandler<TCon, TPacket> 
        where TPacket : Packet
        where TCon : NetConnection<TPacket>
    {
        byte Id { get; }

        void Handle(TPacket packet, TCon connection);
    }
}
