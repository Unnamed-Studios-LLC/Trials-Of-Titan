using System;
namespace Utils.NET.Random
{
    public interface IRandomProvider
    {
        byte Byte();

        sbyte SByte();

        byte[] Bytes(int length);

        ushort UInt16();

        short Int16();

        uint UInt32();

        int Int32();

        ulong UInt64();

        long Int64();

        string Alphanumeric(int length);

        string Numeric(int length);

        string Alpha(int length);

        string String(int length, char[] characterSet);

        string Base64(int byteLength);

        float Float01();

        float AngleRadians();

        float AngleDegrees();

        float Range(float min, float max);

        int Range(int min, int max);
    }
}
