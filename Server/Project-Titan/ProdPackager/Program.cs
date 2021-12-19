using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using TitanDatabase;
using Utils.NET.Logging;

namespace ProdPackager
{
    class Program
    {
        static void Main(string[] args)
        {
            string worldPath = "../Prod.World";
            string webServerPath = "../Prod.WebServer";

            var worldZipPath = "../world.zip";
            var webZipPath = "../webserver.zip";

            Log.Write("Zipping packages");

            if (File.Exists(worldZipPath))
                File.Delete(worldZipPath);
            if (File.Exists(webZipPath))
                File.Delete(webZipPath);

            ZipFile.CreateFromDirectory(worldPath, worldZipPath);
            ZipFile.CreateFromDirectory(webServerPath, webZipPath);

            Log.Write("Transfering to AWS S3...");

            Transfer(worldZipPath, webZipPath);

            Log.Write("Finished packaging update");
        }

        private static void Transfer(string worldZip, string webZip)
        {
            var resetEvent = new ManualResetEvent(false);
            DoTransfer(worldZip, webZip, resetEvent);
            resetEvent.WaitOne();
        }

        private static async void DoTransfer(string worldZip, string webZip, ManualResetEvent resetEvent)
        {
            var s3Client = new AmazonS3Client(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USEast2);

            Log.Write("Transfering world...");

            var fileTransfer = new TransferUtility(s3Client);
            await fileTransfer.UploadAsync(worldZip, "trials-of-titan", "game-server/prod/world.zip");

            Log.Write("Transfering webserver...");

            await fileTransfer.UploadAsync(webZip, "trials-of-titan", "game-server/prod/webserver.zip");

            resetEvent.Set();
        }
    }
}
