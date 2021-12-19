using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Net.Packets;
using Utils.NET.Logging;
using Utils.NET.Net;

namespace World.Net.Handling
{
    public class HandlerMissingException : Exception
    {
        public HandlerMissingException(string message) : base(message) { }

        public HandlerMissingException() { }
    }

    public abstract class ClientPacketHandler<TPacket> : ClientPacketHandler where TPacket : TnPacket
    {
        public override void Handle(TnPacket packet, Client connection)
        {
            Handle((TPacket)packet, connection);
        }

        public abstract void Handle(TPacket packet, Client connection);
    }

    public abstract class ClientPacketHandler
    {
        private static ClientPacketHandler[] handlers;

        static ClientPacketHandler()
        {
            handlers = CreateHandlers();
        }

        private static ClientPacketHandler[] CreateHandlers()
        {
            var baseType = typeof(ClientPacketHandler);
            var handlers = baseType.Assembly.GetTypes()
                .Where(_ => _.IsSubclassOf(baseType) && !_.IsAbstract)
                .Select(_ => (ClientPacketHandler)Activator.CreateInstance(_));

            var handlersOrdered = new ClientPacketHandler[256];
            foreach (var handler in handlers)
            {
                var packetType = handler.GetType().BaseType.GenericTypeArguments[0];
                var type = ((TnPacket)Activator.CreateInstance(packetType)).Id;
                handlersOrdered[type] = handler;
            }
            return handlersOrdered;
        }

        public static void HandlePacket(TnPacket packet, Client connection)
        {
            var handler = handlers[packet.Id];
            if (handler == null)
                throw new HandlerMissingException("Packet Handler missing for packet type: " + packet.Type);
            handler.Handle(packet, connection);
        }

        public abstract void Handle(TnPacket packet, Client connection);
    }
}
