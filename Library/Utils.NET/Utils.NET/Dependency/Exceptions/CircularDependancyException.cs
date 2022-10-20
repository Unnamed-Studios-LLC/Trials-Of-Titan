using System;
using System.Collections.Generic;

namespace Utils.NET.Dependency.Exceptions
{
    public class CircularDependancyException : Exception
    {
        public CircularDependancyException() : base() { }

        public CircularDependancyException(string message) : base(message) { }
    }
}
