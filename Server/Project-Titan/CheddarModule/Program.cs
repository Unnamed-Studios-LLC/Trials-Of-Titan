using PackageDownloader;
using ServerNode;
using System;
using Utils.NET.Modules;

namespace CheddarModule
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new PackageDownloaderModule("trials-of-titan", "cheddar.zip", 3), new NodeModule());
        }
    }
}
