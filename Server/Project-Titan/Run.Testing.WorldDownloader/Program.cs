using System;
using Utils.NET.Modules;
using WorldDownloader;

namespace Run.Testing.WorldDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new WorldDownloaderModule());
        }
    }
}
