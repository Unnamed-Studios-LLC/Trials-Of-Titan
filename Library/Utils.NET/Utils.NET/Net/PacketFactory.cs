using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO;

namespace Utils.NET.Net
{
    public class PacketFactory<TPacket> where TPacket : Packet
    {
        protected Dictionary<byte, Type> packetTypes;

        public int TypeCount => packetTypes.Count;

        public PacketFactory()
        {
            packetTypes = GetPacketTypes();
        }

        protected virtual Dictionary<byte, Type> GetPacketTypes()
        {
            var t = typeof(TPacket);
            return t.Assembly.GetTypes().Where(_ => _.IsSubclassOf(t) && !_.IsAbstract).ToDictionary(_ => ((TPacket)Activator.CreateInstance(_)).Id);
        }

        public virtual TPacket CreatePacket(byte id)
        {
            if (!packetTypes.TryGetValue(id, out var type))
                return null;
            return (TPacket)Activator.CreateInstance(type);
        }
    }
}
