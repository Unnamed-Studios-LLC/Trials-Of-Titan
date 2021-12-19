using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using Test.Net;
using TitanCore.Data;
using Utils.NET.IO;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Udp.Packets;
using Utils.NET.Utils;

namespace Test
{
    public class TestModule : Utils.NET.Modules.Module
    {
        private enum CollisionTypes : uint
        {
            Entity = 1,
            Projectile = 2
        }

        public override string Name => "test";

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            GameData.LoadDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        public override void Stop()
        {

        }
    }
}
