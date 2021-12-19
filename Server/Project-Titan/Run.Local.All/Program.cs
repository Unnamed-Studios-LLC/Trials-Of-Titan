using System;
using System.Timers;
using Utils.NET.Modules;
using WebServer;
using World;

namespace Run.Local.All
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new WebServerModule(), new WorldModule());
        }
    }
}
