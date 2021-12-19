using PackageDownloader;
using ServerNode;
using System;
using Utils.NET.Modules;

namespace Run.Prod.WorldNode
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new PackageDownloaderModule("trials-of-titan", "game-server/prod/world.zip", 2), new NodeModule());
        }
    }
}
