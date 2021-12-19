using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TitanDatabase;
using TitanDatabase.Email;
using TitanDatabase.Instances;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Utils;

namespace WorldDownloader
{
    public class WorldDownloaderModule : Module
    {
        public override string Name => "WorldDownloader";

        private const string Zip_Path = "temp.zip";

        private const string Checksum_Path = "temp.md5";

        private const string Programs_Path = "Programs";

        private const string Program_Path = "Programs/Program-2";

        private AmazonS3Client s3Client;

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            BugReporter.Setup(Emailer.SendBugReport);

            s3Client = new AmazonS3Client(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USEast2);

            DownloadWorld().GetAwaiter().GetResult();
            ApplyWorld();
        }

        public override void Stop()
        {

        }

        private async Task DownloadWorld()
        {
            Log.Write("Downloading World Program...");

            var transferUtility = new TransferUtility(s3Client);
            await transferUtility.DownloadAsync(Checksum_Path, "trials-of-titan", "game-server/world.md5");
            await transferUtility.DownloadAsync(Zip_Path, "trials-of-titan", "game-server/world.zip");
        }

        private void ApplyWorld()
        {
            if (!Directory.Exists(Programs_Path))
                Directory.CreateDirectory(Programs_Path);

            if (Directory.Exists(Program_Path))
            {
                Directory.Delete(Program_Path, true);
            }

            var programDirectoryInfo = Directory.CreateDirectory(Program_Path);
            programDirectoryInfo.Attributes |= FileAttributes.Hidden;

            ZipFile.ExtractToDirectory(Zip_Path, Program_Path);

            File.Delete(Zip_Path);

            programDirectoryInfo.Attributes &= ~FileAttributes.Hidden;
        }
    }
}
