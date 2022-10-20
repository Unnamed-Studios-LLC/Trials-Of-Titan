using System;
using System.Collections.Concurrent;
using Utils.NET.Dependency.Attributes;

namespace Utils.NET.IO.Serialization
{
    public class BitSerializer : IBitSerializer
    {
        private readonly int headerLength;

        private readonly ConcurrentDictionary<Type, IBitObjectFactory> objectFactories;

        public BitSerializer(int headerLength)
        {
            this.headerLength = headerLength;
            objectFactories = new ConcurrentDictionary<Type, IBitObjectFactory>();
        }

        public bool TryDeserialize<T>(byte[] bytes, out T obj)
        {
            var result = TryDeserialize(typeof(T), bytes, out var o);
            if (result)
                obj = (T)o;
            else
                obj = default;
            return result;
        }

        public bool TryDeserialize(Type type, byte[] bytes, out object obj)
        {
            var factory = GetFactory(type);

            var r = new BitReader(bytes, bytes.Length);
            for (int i = 0; i < headerLength; i++)
                r.ReadUInt8();

            obj = factory.Read(r);
            return true;

            /*
            try
            {
                obj = factory.Read(r);
                return true;
            }
            catch (Exception e)
            {
                obj = null;
                return false;
            }
            */
        }

        public byte[] Serialize<T>(T value)
        {
            var factory = GetFactory(typeof(T));

            // write header
            var w = new BitWriter();
            for (int i = 0; i < headerLength; i++)
                w.Write((byte)0);
            factory.Write(w, value);

            return w.GetData().data;
        }

        private IBitObjectFactory GetFactory(Type type)
        {
            if (objectFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }

            factory = new BitObjectFactory(type);
            if (!objectFactories.TryAdd(type, factory))
            {
                return GetFactory(type);
            }

            return factory;
        }
    }
}
