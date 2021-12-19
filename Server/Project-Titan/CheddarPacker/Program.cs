using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using TitanDatabase;
using Utils.NET.Logging;

namespace CheddarPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            string cheddarPath = "../Cheddar";

            var cheddarZipPath = "../cheddar.zip";

            Log.Write("Zipping packages");

            if (File.Exists(cheddarZipPath))
                File.Delete(cheddarZipPath);

            ZipFile.CreateFromDirectory(cheddarPath, cheddarZipPath);

            Log.Write("Transfering to AWS S3...");

            Transfer(cheddarZipPath);

            Log.Write("Finished packaging update");
        }

        private static void Transfer(string worldZip)
        {
            var resetEvent = new ManualResetEvent(false);
            DoTransfer(worldZip, resetEvent);
            resetEvent.WaitOne();
        }

        private static async void DoTransfer(string worldZip, ManualResetEvent resetEvent)
        {
            var s3Client = new AmazonS3Client(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USEast2);

            Log.Write("Transfering cheddar...");

            var fileTransfer = new TransferUtility(s3Client);
            await fileTransfer.UploadAsync(worldZip, "trials-of-titan", "cheddar.zip");

            resetEvent.Set();
        }
    }
}
