using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Utils.NET.Geometry;
using Utils.NET.IO.Serialization.Accessors;

namespace Utils.NET.IO.Serialization
{
    internal class BitObjectFactory : IBitObjectFactory
    {
        private readonly Type baseType;

        private readonly Dictionary<Type, Func<FieldInfo, IAccessor>> accessorFactories;

        private readonly IAccessor[] accessors;

        private readonly CreateDelegate createFactory;

        public BitObjectFactory(Type type)
        {
            baseType = type;
            createFactory = BuildCreate(type);

            accessorFactories = new Dictionary<Type, Func<FieldInfo, IAccessor>>();
            CreateAccessorFactories();

            var fields = type.GetFields();
            accessors = new IAccessor[fields.Length];
            LoadFieldAccessors(fields);
        }

        public object Read(BitReader r)
        {
            var obj = createFactory();
            foreach (var accessor in accessors)
                accessor.Read(r, obj);
            return obj;
        }

        public void Write(BitWriter w, object obj)
        {
            foreach (var accessor in accessors)
                accessor.Write(w, obj);
        }

        private void AddFactory<TField>(Func<FieldInfo, IAccessor> factory)
        {
            accessorFactories[typeof(TField)] = factory;
        }

        private CreateDelegate BuildCreate(Type type)
        {
            var newExp = Expression.New(type);

            var conversion = Expression.Convert(newExp, typeof(object));

            var lambda = Expression.Lambda(typeof(CreateDelegate), conversion);

            var compiled = (CreateDelegate)lambda.Compile();
            return compiled;
        }

        private IAccessor CreateAccessor(FieldInfo field)
        {
            var fieldType = field.FieldType;

            if (fieldType.IsEnum)
            {
                fieldType = Enum.GetUnderlyingType(fieldType);
            }
            else if (IsObjectType(fieldType) && (!fieldType.IsArray || IsObjectType(fieldType.GetElementType())))
            {
                if (fieldType.IsArray)
                {
                    return new ArrayAccessorBase<object>(baseType, field, new ObjectProcessor(field));
                }
                return new AccessorBase<object>(baseType, field, new ObjectProcessor(field));
            }

            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }

            if (!accessorFactories.TryGetValue(fieldType, out var factory))
            {
                throw new InvalidOperationException($"Field '{field.Name}' of {field.DeclaringType.Name} is not serializable");
            }

            return factory(field);
        }

        private void CreateAccessorFactories()
        {
            AddFactory<bool>(x => CreateAccessorFactory(x, new BoolProcessor()));

            AddFactory<byte>(x => CreateAccessorFactory(x, new UInt8Processor()));
            AddFactory<ushort>(x => CreateAccessorFactory(x, new UInt16Processor()));
            AddFactory<uint>(x => CreateAccessorFactory(x, new UInt32Processor()));
            AddFactory<ulong>(x => CreateAccessorFactory(x, new UInt64Processor()));

            AddFactory<sbyte>(x => CreateAccessorFactory(x, new Int8Processor()));
            AddFactory<short>(x => CreateAccessorFactory(x, new Int16Processor()));
            AddFactory<int>(x => CreateAccessorFactory(x, new Int32Processor()));
            AddFactory<long>(x => CreateAccessorFactory(x, new Int64Processor()));

            AddFactory<float>(x => CreateAccessorFactory(x, new FloatProcessor()));
            AddFactory<double>(x => CreateAccessorFactory(x, new DoubleProcessor()));

            AddFactory<string>(x => CreateAccessorFactory(x, new StringProcessor()));
            AddFactory<DateTime>(x => CreateAccessorFactory(x, new DateTimeProcessor()));
            AddFactory<Vec2>(x => CreateAccessorFactory(x, new Vec2Processor()));
        }

        private IAccessor CreateAccessorFactory<T>(FieldInfo field, Processor<T> processor)
        {
            if (field.FieldType.IsArray)
            {
                return new ArrayAccessorBase<T>(baseType, field, processor);
            }
            return new AccessorBase<T>(baseType, field, processor);
        }

        private void LoadFieldAccessors(FieldInfo[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                accessors[i] = CreateAccessor(fields[i]);
            }
        }

        private bool IsObjectType(Type type)
        {
            return type.IsClass && type != typeof(string);
        }
    }
}
