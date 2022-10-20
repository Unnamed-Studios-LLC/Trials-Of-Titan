using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Utils.NET.Dependency.Attributes;

namespace Utils.NET.Dependency.Resolvers
{
    internal class TypeDependencyResolver : IDependencyResolver
    {
        delegate object ObjectActivator(params object[] args);

        private readonly Type[] parameterTypes;

        private readonly ObjectActivator objectActivator;

        private readonly bool isSharedInstance;

        private object resolvedObject;

        public TypeDependencyResolver(Type type)
        {
            var contructors = type.GetConstructors();
            var contructor = contructors[0];

            var parameters = contructor.GetParameters();
            parameterTypes = parameters.Select(x => x.ParameterType)
                .ToArray();

            objectActivator = BuildObjectActivator(contructor);

            if (type.GetCustomAttribute<SharedInstanceAttribute>() != null)
            {
                isSharedInstance = true;
            }
        }

        /// <summary>
        /// Pulled from https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        private ObjectActivator BuildObjectActivator(ConstructorInfo constructor)
        {
            //create a single param of type object[]
            var param = Expression.Parameter(typeof(object[]), "args");

            var argsExp = new Expression[parameterTypes.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = parameterTypes[i];

                var paramAccessorExp = Expression.ArrayIndex(param, index);

                var paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            var newExp = Expression.New(constructor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            var lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);

            //compile it
            var compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }

        public object Resolve(DependencyContainer container, Stack<Type> stack)
        {
            if (resolvedObject != null)
            {
                return resolvedObject;
            }

            var parameters = ResolveParameters(container, stack);

            var obj = objectActivator.Invoke(parameters);

            if (isSharedInstance)
            {
                resolvedObject = obj;
            }
            return obj;
        }

        private object[] ResolveParameters(DependencyContainer container, Stack<Type> stack)
        {
            var parameters = new object[parameterTypes.Length];

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                parameters[i] = container.Resolve(parameterTypes[i], stack);
            }

            return parameters;
        }
    }
}
