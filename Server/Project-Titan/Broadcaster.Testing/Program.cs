using ServerBroadcaster;
using System;
using Utils.NET.Modules;

namespace Broadcaster.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new ServerBroadcasterModule());
        }
    }
}
