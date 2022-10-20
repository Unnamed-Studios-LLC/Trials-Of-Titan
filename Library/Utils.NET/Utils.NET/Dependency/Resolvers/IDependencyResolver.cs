using System;
using System.Collections.Generic;

namespace Utils.NET.Dependency.Resolvers
{
    internal interface IDependencyResolver
    {
        object Resolve(DependencyContainer container, Stack<Type> stack);
    }
}
