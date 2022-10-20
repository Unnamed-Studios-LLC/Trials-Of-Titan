using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils.NET.IO.Serialization.Accessors
{
    internal class ArrayAccessorBase<TField> : AccessorBase<Array>
    {
        private readonly FieldInfo field;

        private readonly Processor<TField> elementProcessor;

        public ArrayAccessorBase(Type baseType, FieldInfo field, Processor<TField> elementProcessor) : base(baseType, field, null)
        {
            this.field = field;
            this.elementProcessor = elementProcessor;
        }

        protected override Array ReadElement(BitReader r)
        {
            var count = r.ReadUInt16();
            var array = Array.CreateInstance(field.FieldType.GetElementType(), count);
            for (int i = 0; i < count; i++)
            {
                array.SetValue(elementProcessor.ReadElement(r), i);
            }
            return array;
        }

        protected override void WriteElement(BitWriter w, Array element)
        {
            var count = (ushort)element.Length;
            w.Write(count);
            for (int i = 0; i < count; i++)
            {
                elementProcessor.WriteElement(w, (TField)element.GetValue(i));
            }
        }
    }
}
