using PackageDownloader;
using ServerNode;
using System;
using Utils.NET.Modules;

namespace Run.Testing.WorldNode
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new PackageDownloaderModule("trials-of-titan", "game-server/testing/world.zip", 2), new NodeModule());
        }
    }
}
