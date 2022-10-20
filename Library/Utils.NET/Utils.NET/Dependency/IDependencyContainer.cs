using System;
using System.Reflection;

namespace Utils.NET.Dependency
{
    public interface IDependencyContainer
    {
        void Register<TType, TResolved>();

        void Register<T>(object obj);

        void Register(params Assembly[] assemblies);

        void RegisterInstance<TType, TResolved>();

        T Resolve<T>();

        object Resolve(Type type);
    }
}
