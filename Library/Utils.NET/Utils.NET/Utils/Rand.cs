using System;
using System.Text;
using System.Threading;
using Utils.NET.Collections;
using Utils.NET.Geometry;

namespace Utils.NET.Utils
{
    public static class Rand
    {
        private static ThreadLocal<System.Random> randoms = new ThreadLocal<System.Random>();

        private static System.Random random
        {
            get
            {
                if (randoms.IsValueCreated) return randoms.Value;
                System.Random rnd = new System.Random((int)DateTime.Now.Ticks);
                randoms.Value = rnd;
                return rnd;
            }
        }

        #region Integer

        /// <summary>
        /// Returns a random number within a range
        /// </summary>
        /// <returns>The next.</returns>
        /// <param name="min">The lowest number (inclusive)</param>
        /// <param name="max">The maximum output (exclusive)</param>
        public static int Range(int min, int max)
        {
            return random.Next(min, max);
        }

        /// <summary>
        /// Returns a random number with a maximum given value
        /// </summary>
        /// <returns>The next.</returns>
        /// <param name="max">The maximum output (exclusive)</param>
        public static int Next(int max)
        {
            return Range(0, max);
        }

        /// <summary>
        /// Returns a random int32 value
        /// </summary>
        /// <returns>The value.</returns>
        public static int IntValue()
        {
            return Range(int.MinValue, int.MaxValue);
        }

        #endregion

        #region Float

        /// <summary>
        /// Returns a random float value at or between 0 and 2 PI
        /// </summary>
        /// <returns>The value.</returns>
        public static float AngleValue()
        {
            return (float)random.NextDouble() * AngleUtils.PI_2;
        }

        /// <summary>
        /// Returns a random float value at or between 0 and 1
        /// </summary>
        /// <returns>The value.</returns>
        public static float FloatValue()
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Returns a random float value between two given numbers
        /// </summary>
        /// <returns>The range.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static float Range(float min, float max)
        {
            return min + (max - min) * FloatValue();
        }

        #endregion

        #region Misc

        public static ulong UInt64()
        {
            return BitConverter.ToUInt64(Bytes(8), 0);
        }

        public static float Range(Range range)
        {
            return range.min + (range.max - range.min) * FloatValue();
        }

        public static byte[] Bytes(int length)
        {
            var bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        public static string Base64(int byteLength)
        {
            var bytes = Bytes(byteLength);
            return Convert.ToBase64String(bytes);
        }

        private static char[] alphanumericCharacters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public static string Alphanumeric(int length)
        {
            var builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                builder.Append(alphanumericCharacters[Next(alphanumericCharacters.Length)]);
            return builder.ToString();
        }

        public static string String(int length, char[] characters)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < length; i++)
                builder.Append(characters[Next(characters.Length)]);
            return builder.ToString();
        }

        #endregion
    }
}
