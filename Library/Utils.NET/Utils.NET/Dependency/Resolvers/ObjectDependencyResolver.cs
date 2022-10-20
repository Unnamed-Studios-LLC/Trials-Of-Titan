using System;
using System.Collections.Generic;

namespace Utils.NET.Dependency.Resolvers
{
    public class ObjectDependencyResolver : IDependencyResolver
    {
        private readonly object obj;

        public ObjectDependencyResolver(object obj)
        {
            this.obj = obj;
        }

        public object Resolve(DependencyContainer container, Stack<Type> stack)
        {
            return obj;
        }
    }
}
