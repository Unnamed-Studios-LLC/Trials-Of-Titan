using System;

namespace Utils.NET.Dependency.Exceptions
{
    public class UnableToResolveTypeException : Exception
    {
        public UnableToResolveTypeException() : base() { }

        public UnableToResolveTypeException(Type type, string reason) : base($"Unable to resolve {type.FullName}: {reason}") { }
    }
}
