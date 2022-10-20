using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Utils.NET.Geometry;

namespace Utils.NET.IO.Serialization.Accessors
{
    internal class BoolProcessor : Processor<bool>
    {
        public override bool ReadElement(BitReader r)
        {
            return r.ReadBool();
        }

        public override void WriteElement(BitWriter w, bool element)
        {
            w.Write(element);
        }
    }

    internal class Int8Processor : Processor<sbyte>
    {
        public override sbyte ReadElement(BitReader r)
        {
            return r.ReadInt8();
        }

        public override void WriteElement(BitWriter w, sbyte element)
        {
            w.Write(element);
        }
    }

    internal class Int16Processor : Processor<short>
    {
        public override short ReadElement(BitReader r)
        {
            return r.ReadInt16();
        }

        public override void WriteElement(BitWriter w, short element)
        {
            w.Write(element);
        }
    }

    internal class Int32Processor : Processor<int>
    {
        public override int ReadElement(BitReader r)
        {
            return r.ReadInt32();
        }

        public override void WriteElement(BitWriter w, int element)
        {
            w.Write(element);
        }
    }

    internal class Int64Processor : Processor<long>
    {
        public override long ReadElement(BitReader r)
        {
            return r.ReadInt64();
        }

        public override void WriteElement(BitWriter w, long element)
        {
            w.Write(element);
        }
    }
    internal class UInt8Processor : Processor<byte>
    {
        public override byte ReadElement(BitReader r)
        {
            return r.ReadUInt8();
        }

        public override void WriteElement(BitWriter w, byte element)
        {
            w.Write(element);
        }
    }

    internal class UInt16Processor : Processor<ushort>
    {
        public override ushort ReadElement(BitReader r)
        {
            return r.ReadUInt16();
        }

        public override void WriteElement(BitWriter w, ushort element)
        {
            w.Write(element);
        }
    }

    internal class UInt32Processor : Processor<uint>
    {
        public override uint ReadElement(BitReader r)
        {
            return r.ReadUInt32();
        }

        public override void WriteElement(BitWriter w, uint element)
        {
            w.Write(element);
        }
    }

    internal class UInt64Processor : Processor<ulong>
    {
        public override ulong ReadElement(BitReader r)
        {
            return r.ReadUInt64();
        }

        public override void WriteElement(BitWriter w, ulong element)
        {
            w.Write(element);
        }
    }

    internal class FloatProcessor : Processor<float>
    {
        public override float ReadElement(BitReader r)
        {
            return r.ReadFloat();
        }

        public override void WriteElement(BitWriter w, float element)
        {
            w.Write(element);
        }
    }

    internal class DoubleProcessor : Processor<double>
    {
        public override double ReadElement(BitReader r)
        {
            return r.ReadDouble();
        }

        public override void WriteElement(BitWriter w, double element)
        {
            w.Write(element);
        }
    }

    internal class StringProcessor : Processor<string>
    {
        public override string ReadElement(BitReader r)
        {
            return r.ReadUTF(400);
        }

        public override void WriteElement(BitWriter w, string element)
        {
            w.Write(element);
        }
    }

    internal class Vec2Processor : Processor<Vec2>
    {
        public override Vec2 ReadElement(BitReader r)
        {
            return new Vec2(r.ReadFloat(), r.ReadFloat());
        }

        public override void WriteElement(BitWriter w, Vec2 element)
        {
            w.Write(element.x);
            w.Write(element.y);
        }
    }

    internal class DateTimeProcessor : Processor<DateTime>
    {
        public override DateTime ReadElement(BitReader r)
        {
            return r.ReadDateTime();
        }

        public override void WriteElement(BitWriter w, DateTime element)
        {
            w.Write(element);
        }
    }

    internal class ObjectProcessor : Processor<object>
    {
        private readonly BitObjectFactory factory;

        public ObjectProcessor(FieldInfo field)
        {
            var fieldType = field.FieldType;
            if (fieldType.IsArray)
                fieldType = field.FieldType.GetElementType();
            factory = new BitObjectFactory(fieldType);
        }

        public override object ReadElement(BitReader r)
        {
            return factory.Read(r);
        }

        public override void WriteElement(BitWriter w, object element)
        {
            factory.Write(w, element);
        }
    }
}
