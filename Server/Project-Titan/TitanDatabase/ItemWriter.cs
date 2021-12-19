using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TitanDatabase
{
    public class ItemWriter
    {
        private Dictionary<string, AttributeValue> values = new Dictionary<string, AttributeValue>();

        public Dictionary<string, AttributeValue> GetValues() => values;

        public void Write(string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            values[name] = new AttributeValue()
            {
                S = value
            };
        }

        public void Write(string name, bool value)
        {
            values[name] = new AttributeValue()
            {
                BOOL = value
            };
        }

        public void Write(string name, MemoryStream value)
        {
            values[name] = new AttributeValue()
            {
                B = value
            };
        }

        public void Write(string name, DateTime value)
        {
            values[name] = new AttributeValue()
            {
                N = value.Ticks.ToString()
            };
        }

        public void Write(string name, List<string> list)
        {
            if (list.Count == 0) return;

            var slist = new List<AttributeValue>();
            for (int i = 0; i < list.Count; i++)
                slist.Add(new AttributeValue { S = list[i] });
            values[name] = new AttributeValue()
            {
                L = slist
            };
        }

        public void Write(string name, List<uint> list)
        {
            if (list.Count == 0) return;

            var slist = new List<AttributeValue>();
            for (int i = 0; i < list.Count; i++)
                slist.Add(new AttributeValue { N = list[i].ToString() });
            values[name] = new AttributeValue()
            {
                L = slist
            };
        }

        public void Write(string name, List<ulong> list)
        {
            if (list.Count == 0) return;

            var slist = new List<AttributeValue>();
            for (int i = 0; i < list.Count; i++)
                slist.Add(new AttributeValue { N = list[i].ToString() });
            values[name] = new AttributeValue()
            {
                L = slist
            };
        }

        public void Write(string name, byte value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, ushort value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, uint value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, ulong value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, sbyte value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, short value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, int value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }

        public void Write(string name, long value)
        {
            values[name] = new AttributeValue()
            {
                N = value.ToString()
            };
        }
    }
}
