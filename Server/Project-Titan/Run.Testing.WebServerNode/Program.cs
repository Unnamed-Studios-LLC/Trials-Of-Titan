using PackageDownloader;
using ServerNode;
using Utils.NET.Modules;
using System;

namespace Run.Testing.WebServerNode
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new PackageDownloaderModule("trials-of-titan", "game-server/testing/webserver.zip", 1), new NodeModule());
        }
    }
}
