using PackageDownloader;
using ServerNode;
using System;
using Utils.NET.Modules;

namespace Run.Prod.WebServerNode
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new PackageDownloaderModule("trials-of-titan", "game-server/prod/webserver.zip", 1), new NodeModule());
        }
    }
}
