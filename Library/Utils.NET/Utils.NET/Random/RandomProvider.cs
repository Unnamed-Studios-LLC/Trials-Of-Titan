using System;
using System.Text;
using System.Threading;
using Utils.NET.Geometry;

namespace Utils.NET.Random
{
    public class RandomProvider : IRandomProvider
    {
        private static readonly char[] alphanumericCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static readonly char[] alphaCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private static readonly char[] numericCharacters = "0123456789".ToCharArray();

        private readonly ThreadLocal<System.Random> randoms = new ThreadLocal<System.Random>(() => new System.Random((int)DateTime.Now.Ticks));

        private System.Random random => randoms.Value;

        public string Alpha(int length)
        {
            return String(length, alphaCharacters);
        }

        public string Alphanumeric(int length)
        {
            return String(length, alphanumericCharacters);
        }

        public float AngleDegrees()
        {
            return (float)Math.PI * 2 * AngleUtils.Rad2Deg * Float01();
        }

        public float AngleRadians()
        {
            return (float)Math.PI * 2 * Float01();
        }

        public string Base64(int byteLength)
        {
            return Convert.ToBase64String(Bytes(byteLength));
        }

        public byte Byte()
        {
            return Bytes(1)[0];
        }

        public byte[] Bytes(int length)
        {
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        public float Float01()
        {
            return (float)random.NextDouble();
        }

        public short Int16()
        {
            return BitConverter.ToInt16(Bytes(2), 0);
        }

        public int Int32()
        {
            return BitConverter.ToInt32(Bytes(4), 0);
        }

        public long Int64()
        {
            return BitConverter.ToInt64(Bytes(8), 0);
        }

        public string Numeric(int length)
        {
            return String(length, numericCharacters);
        }

        public float Range(float min, float max)
        {
            return min + (max - min) * Float01();
        }

        public int Range(int min, int max)
        {
            return random.Next(min, max);
        }

        public sbyte SByte()
        {
            return (sbyte)Byte();
        }

        public string String(int length, char[] characterSet)
        {
            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                builder.Append(characterSet[Range(0, characterSet.Length)]);
            return builder.ToString();
        }

        public ushort UInt16()
        {
            return BitConverter.ToUInt16(Bytes(2), 0);
        }

        public uint UInt32()
        {
            return BitConverter.ToUInt32(Bytes(4), 0);
        }

        public ulong UInt64()
        {
            return BitConverter.ToUInt64(Bytes(8), 0);
        }
    }
}
