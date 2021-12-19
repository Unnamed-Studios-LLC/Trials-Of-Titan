using ProgramNode;
using System;
using Utils.NET.Modules;
using WebServer;

namespace Run.Testing.WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new WebServerModule(), new ProgramNodeModule());
        }
    }
}
