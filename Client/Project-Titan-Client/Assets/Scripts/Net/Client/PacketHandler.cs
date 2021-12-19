using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Packets;

public class PacketHandler<T> : PacketHandler where T : TnPacket
{
    private Action<T> handlerFunc;

    public PacketHandler(Action<T> handlerFunc) : base()
    {
        this.handlerFunc = handlerFunc;
    }

    public override void Handle(TnPacket packet)
    {
        handlerFunc((T)packet);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is PacketHandler<T> handler))
            return false;
        return handlerFunc == handler.handlerFunc;
    }

    public override int GetHashCode()
    {
        return handlerFunc.GetHashCode();
    }

    public static bool operator ==(PacketHandler<T> a, PacketHandler<T> b) => a.Equals(b);
    public static bool operator !=(PacketHandler<T> a, PacketHandler<T> b) => !a.Equals(b);
}

public abstract class PacketHandler
{
    public abstract void Handle(TnPacket packet);
}
