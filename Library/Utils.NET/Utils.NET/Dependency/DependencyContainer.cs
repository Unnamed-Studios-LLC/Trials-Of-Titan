using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Utils.NET.Dependency.Attributes;
using Utils.NET.Dependency.Exceptions;
using Utils.NET.Dependency.Resolvers;

namespace Utils.NET.Dependency
{
    public class DependencyContainer : IDependencyContainer
    {
        private readonly ConcurrentDictionary<Type, IDependencyResolver> dependencyResolvers;

        internal DependencyContainer()
        {
            dependencyResolvers = new ConcurrentDictionary<Type, IDependencyResolver>();
        }

        public static DependencyContainer Create()
        {
            var container = new DependencyContainer();
            container.Register<IDependencyContainer>(container);
            return container;
        }

        public void Register<TType, TResolved>()
        {
            dependencyResolvers[typeof(TType)] = new TypeDependencyResolver(typeof(TResolved));
        }

        public void Register<T>(object obj)
        {
            dependencyResolvers[typeof(T)] = new ObjectDependencyResolver(obj);
        }

        public void Register(params Assembly[] assemblies)
        {
            var mappedSolutions = new Dictionary<Type, Type>();

            var types = assemblies.Aggregate(new List<Type>(), (list, assembly) =>
            {
                list.AddRange(assembly.GetTypes());
                return list;
            });

            var interfaces = types.Where(x => x.IsInterface);

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || type.GetConstructors().Length == 0 || type.ContainsGenericParameters)
                {
                    continue;
                }

                foreach (var interfaceType in interfaces)
                {
                    if (!interfaceType.IsAssignableFrom(type)) continue;
                    Register(interfaceType, type);
                }
            }
        }

        public void RegisterInstance<TType, TResolved>()
        {
            dependencyResolvers[typeof(TType)] = new ObjectDependencyResolver(Resolve(typeof(TResolved)));
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var stack = new Stack<Type>();
            return Resolve(type, stack);
        }

        private void Register(Type interfaceType, Type resolvedType)
        {
            dependencyResolvers[interfaceType] = new TypeDependencyResolver(resolvedType);
        }

        internal object Resolve(Type type, Stack<Type> dependancyStack)
        {
            if (dependancyStack.Contains(type))
            {
                dependancyStack.Push(type);
                throw new CircularDependancyException($"Circular dependancy encountered for {type}\nStack:\n{StackToMessage(dependancyStack)}");
            }

            dependancyStack.Push(type);

            if (!dependencyResolvers.TryGetValue(type, out var resolver))
            {
                if (type.IsClass && !type.IsAbstract)
                {
                    resolver = new TypeDependencyResolver(type);
                    dependencyResolvers[type] = resolver;
                }
                else
                {
                    throw new UnableToResolveTypeException(type, "Type is not registered");
                }
            }

            var obj = resolver.Resolve(this, dependancyStack);

            dependancyStack.Pop();

            return obj;
        }

        internal string StackToMessage(Stack<Type> stack)
        {
            var builder = new StringBuilder();
            foreach (var type in stack)
            {
                if (builder.Length != 0)
                {
                    builder.Append('\n');
                }
                builder.Append(type);
            }

            return builder.ToString();
        }
    }
}
