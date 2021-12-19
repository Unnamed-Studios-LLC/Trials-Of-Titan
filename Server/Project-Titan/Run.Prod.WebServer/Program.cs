using ProgramNode;
using System;
using Utils.NET.Modules;
using WebServer;

namespace Run.Prod.WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new WebServerModule(), new ProgramNodeModule());
        }
    }
}
