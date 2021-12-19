using ProgramNode;
using System;
using Utils.NET.Modules;
using World;

namespace Run.Testing.World
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new WorldModule(), new ProgramNodeModule());
        }
    }
}
