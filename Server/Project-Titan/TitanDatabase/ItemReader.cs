using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utils.NET.Logging;

namespace TitanDatabase
{
    public class ItemReader
    {
        public Dictionary<string, AttributeValue> values;

        public Dictionary<string, AttributeValue> GetValues() => values;

        public ItemReader(Dictionary<string, AttributeValue> values)
        {
            this.values = values;
        }

        public string String(string name, string def = "")
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return attr.S;
        }

        public bool Bool(string name, bool def = false)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return attr.BOOL;
        }

        public MemoryStream Binary(string name, MemoryStream def = null)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return attr.B;
        }

        public DateTime Date(string name, DateTime def)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return new DateTime(long.Parse(attr.N));
        }

        public List<string> StringList(string name)
        {
            if (!values.TryGetValue(name, out var list)) return new List<string>();

            var l = list.L;
            var r = new List<string>();
            for (int i = 0; i < l.Count; i++)
                r.Add(l[i].S);
            return r;
        }

        public List<uint> UInt32List(string name)
        {
            if (!values.TryGetValue(name, out var list)) return new List<uint>();

            var l = list.L;
            var r = new List<uint>();
            for (int i = 0; i < l.Count; i++)
                r.Add(uint.Parse(l[i].N));
            return r;
        }

        public List<ulong> UInt64List(string name)
        {
            if (!values.TryGetValue(name, out var list)) return new List<ulong>();

            var l = list.L;
            var r = new List<ulong>();
            for (int i = 0; i < l.Count; i++)
                r.Add(ulong.Parse(l[i].N));
            return r;
        }

        public byte UInt8(string name, byte def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return byte.Parse(attr.N);
        }

        public ushort UInt16(string name, ushort def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return ushort.Parse(attr.N);
        }

        public uint UInt32(string name, uint def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return uint.Parse(attr.N);
        }

        public ulong UInt64(string name, ulong def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return ulong.Parse(attr.N);
        }

        public sbyte Int8(string name, sbyte def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return sbyte.Parse(attr.N);
        }

        public short Int16(string name, short def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return short.Parse(attr.N);
        }

        public int Int32(string name, int def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return int.Parse(attr.N);
        }

        public long Int64(string name, long def = 0)
        {
            if (!values.TryGetValue(name, out var attr)) return def;
            return long.Parse(attr.N);
        }
    }
}
