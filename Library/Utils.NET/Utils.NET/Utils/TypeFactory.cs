using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.NET.Utils
{
    public class TypeFactory<TKey, TValue>
    {
        private Dictionary<TKey, Type> types;

        public TypeFactory(Func<TValue, TKey> keyGrabber)
        {
            var baseType = typeof(TValue);
            types = baseType.Assembly.GetTypes()
                .Where(_ => baseType.IsAssignableFrom(_) && !_.IsAbstract)
                .ToDictionary(_ => keyGrabber((TValue)Activator.CreateInstance(_)));
        }

        public TValue Create(TKey key)
        {
            if (!types.TryGetValue(key, out var type))
                return default;
            return (TValue)Activator.CreateInstance(type);
        }
    }
}
