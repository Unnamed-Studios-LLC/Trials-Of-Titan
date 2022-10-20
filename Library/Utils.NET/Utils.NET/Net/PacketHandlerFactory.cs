using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Logging;
using Utils.NET.Net.Tcp;

namespace Utils.NET.Net
{
    public class PacketHandlerFactory<TCon, THandler, TPacket> 
        where TPacket : Packet
        where TCon : NetConnection<TPacket>
        where THandler : IPacketHandler<TCon, TPacket>
    {
        private IPacketHandler<TCon, TPacket>[] handlerTypes;

        public PacketHandlerFactory()
        {
            var handlerType = typeof(THandler).GetGenericTypeDefinition();
            var handlers = handlerType.Assembly.GetTypes().Where(_ => IsPacketHandler(_, handlerType)).Select(_ => (IPacketHandler<TCon, TPacket>)Activator.CreateInstance(_));
            handlerTypes = new IPacketHandler<TCon, TPacket>[256];
            foreach (var handler in handlers)
                handlerTypes[handler.Id] = handler;
        }

        private bool IsPacketHandler(Type sub, Type baseClass)
        {
            var baseType = sub.BaseType;
            if (!baseType.IsAbstract) return false;
            return baseType.IsGenericType && (baseType.GetGenericTypeDefinition() == baseClass);
        }

        public void Handle(TPacket packet, TCon connection)
        {
            var handler = handlerTypes[packet.Id];
            if (handler == null) return;
            handler.Handle(packet, connection);
        }
    }
}
