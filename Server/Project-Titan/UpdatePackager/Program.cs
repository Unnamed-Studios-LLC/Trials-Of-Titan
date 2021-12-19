using System;
using System.IO;
using System.IO.Compression;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace UpdatePackager
{
    class Program
    {
        static void Main(string[] args)
        {
            string path;
            do
            {
                Console.WriteLine("Enter path to update directory.");
                path = Console.ReadLine();
            }
            while (!Directory.Exists(path));

            var parentDir = Path.GetDirectoryName(path);
            var zipPath = Path.Combine(parentDir, "world.zip");
            var md5Path = Path.Combine(parentDir, "world.md5");

            ZipFile.CreateFromDirectory(path, zipPath);
            File.WriteAllBytes(md5Path, Checksum.MD5(zipPath));

            Console.WriteLine("Finished packaging update");
        }
    }
}
