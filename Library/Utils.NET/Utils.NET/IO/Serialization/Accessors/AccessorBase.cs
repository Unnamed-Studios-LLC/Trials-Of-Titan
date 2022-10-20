using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Utils.NET.IO.Serialization.Accessors
{
    internal class AccessorBase<TField> : IAccessor
    {
        private readonly Processor<TField> processor;

        private readonly GetDelegate<TField> getter;

        private readonly SetDelegate<TField> setter;

        public AccessorBase(Type baseType, FieldInfo field, Processor<TField> processor)
        {
            this.processor = processor;

            var targetExp = Expression.Parameter(typeof(object), "target");

            var valueType = typeof(TField);
            var fieldType = field.FieldType;
            //if (fieldType.IsArray)
            //    fieldType = fieldType.GetElementType();

            var valueExp = Expression.Parameter(valueType, "value");
            var valueConvExp = (Expression)valueExp;

            if (fieldType != valueType)
            {
                valueConvExp = Expression.Convert(valueExp, fieldType);
            }

            var conversion = Expression.Convert(targetExp, baseType);
            var fieldExp = Expression.Field(conversion, field);
            var assignExp = Expression.Assign(fieldExp, valueConvExp);

            var fieldConvExp = (Expression)fieldExp;

            if (fieldType != valueType)
            {
                fieldConvExp = Expression.Convert(fieldExp, valueType);
            }


            getter = Expression.Lambda<GetDelegate<TField>>(fieldConvExp, targetExp).Compile();
            setter = Expression.Lambda<SetDelegate<TField>>(assignExp, targetExp, valueExp).Compile();
        }

        public void Read(BitReader r, object obj)
        {
            Set(obj, ReadElement(r));
        }

        protected virtual TField ReadElement(BitReader r)
        {
            return processor.ReadElement(r);
        }

        public void Write(BitWriter w, object obj)
        {
            WriteElement(w, Get(obj));
        }

        protected virtual void WriteElement(BitWriter w, TField element)
        {
            processor.WriteElement(w, element);
        }

        protected TField Get(object obj)
        {
            return getter(obj);
        }

        protected void Set(object obj, TField value)
        {
            setter(obj, value);
        }
    }
}
