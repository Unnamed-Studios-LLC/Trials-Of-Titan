using System;

namespace Utils.NET.IO.Serialization
{
    public interface IBitSerializer
    {
        bool TryDeserialize<T>(byte[] bytes, out T obj);

        bool TryDeserialize(Type type, byte[] bytes, out dynamic obj);

        byte[] Serialize<T>(T value);
    }
}
