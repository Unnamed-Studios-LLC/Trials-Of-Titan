using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using TitanDatabase;
using TitanDatabase.Email;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Utils;

namespace PackageDownloader
{
    public class PackageDownloaderModule : Module
    {
        public override string Name => "PackgeDownloader";

        private const string Zip_Path = "temp.zip";

        private const string Programs_Path = "Programs";

        private string programPath = "Programs/Program-";

        private string bucketName;

        private string packageName;

        private AmazonS3Client s3Client;

        public PackageDownloaderModule(string bucketName, string packageName, int programType)
        {
            this.bucketName = bucketName;
            this.packageName = packageName;
            programPath += programType;
        }

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            BugReporter.Setup(Emailer.SendBugReport);

            s3Client = new AmazonS3Client(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USEast2);

            DownloadProgram().GetAwaiter().GetResult();
            ApplyWorld();
        }

        public override void Stop()
        {

        }

        private async Task DownloadProgram()
        {
            Log.Write("Downloading Program...");

            var transferUtility = new TransferUtility(s3Client);
            await transferUtility.DownloadAsync(Zip_Path, bucketName, packageName);
        }

        private void ApplyWorld()
        {
            if (!Directory.Exists(Programs_Path))
                Directory.CreateDirectory(Programs_Path);

            if (Directory.Exists(programPath))
            {
                Directory.Delete(programPath, true);
            }

            var programDirectoryInfo = Directory.CreateDirectory(programPath);
            programDirectoryInfo.Attributes |= FileAttributes.Hidden;

            ZipFile.ExtractToDirectory(Zip_Path, programPath);

            File.Delete(Zip_Path);

            programDirectoryInfo.Attributes &= ~FileAttributes.Hidden;
        }
    }
}
