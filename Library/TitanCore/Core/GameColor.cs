using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Utils;

namespace TitanCore.Core
{
    /// <summary>
    /// Used to represent a color in game
    /// </summary>
    public struct GameColor
    {
        public static GameColor flashClear = new GameColor(0, 0, 0);
        public static GameColor white = new GameColor(sbyte.MaxValue, sbyte.MaxValue, sbyte.MaxValue);

        public static GameColor red = new GameColor(sbyte.MaxValue, sbyte.MinValue, sbyte.MinValue);
        public static GameColor green = new GameColor(sbyte.MinValue, sbyte.MaxValue, sbyte.MinValue);
        public static GameColor yellow = new GameColor(sbyte.MaxValue, sbyte.MaxValue, 0);
        public static GameColor black = new GameColor(0, 0, 0);
        public static GameColor blue = new GameColor(sbyte.MinValue, sbyte.MinValue, sbyte.MaxValue);
        public static GameColor cyan = new GameColor(sbyte.MinValue, sbyte.MaxValue, sbyte.MaxValue);
        public static GameColor pink = new GameColor(sbyte.MaxValue, sbyte.MinValue, sbyte.MaxValue);
        public static GameColor orange = new GameColor(sbyte.MaxValue, 0, sbyte.MinValue);
        public static GameColor purple = new GameColor(0, sbyte.MinValue, sbyte.MaxValue);

        public static GameColor Parse(string stringValue)
        {
            if (stringValue.StartsWith("#"))
            {
                uint color = StringUtils.ParseHex(stringValue);
                uint r = (color >> 16) & 255;
                uint g = (color >> 8) & 255;
                uint b = (color) & 255;
                return new GameColor(ConvertColorValue(r), ConvertColorValue(g), ConvertColorValue(b));
            }

            switch (stringValue.Trim().ToLower())
            {
                case "red":
                    return red;
                case "green":
                    return green;
                case "yellow":
                    return yellow;
                case "black":
                    return black;
                case "blue":
                    return blue;
                case "cyan":
                    return cyan;
                case "pink":
                    return pink;
                case "orange":
                    return orange;
                case "purple":
                    return purple;
                default:
                    return white;
            }
        }

        private static sbyte ConvertColorValue(uint colorValue)
        {
            return (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, colorValue - 128));
        }

        public static GameColor ReadColor(BitReader r)
        {
            var color = new GameColor();
            color.Read(r);
            return color;
        }

        public sbyte r;

        public sbyte g;

        public sbyte b;

        public GameColor(sbyte r, sbyte g, sbyte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void Read(BitReader r)
        {
            this.r = r.ReadInt8();
            g = r.ReadInt8();
            b = r.ReadInt8();
        }

        public void Write(BitWriter w)
        {
            w.Write(r);
            w.Write(g);
            w.Write(b);
        }

        public string ToHex()
        {
            return (r + 128).ToString("X2") + (g + 128).ToString("X2") + (b + 128).ToString("X2");
        }

        public override bool Equals(object obj)
        {
            if (obj is GameColor color)
                return this == color;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (r << 16) | (g << 8) | (int)b;
        }

        public static bool operator ==(GameColor a, GameColor b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b;
        }

        public static bool operator !=(GameColor a, GameColor b)
        {
            return !(a == b);
        }
    }
}
