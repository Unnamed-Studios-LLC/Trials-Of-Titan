using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanDatabase;
using Utils.NET.Modules;

namespace TitanDatabase.Instances
{
    public class Ec2Instance
    {
        public static AmazonEC2Client client;

        public static int ServerType { get; private set; }

        public static string ServerName { get; private set; }

        public static string PingHost { get; private set; }

        public static string PublicIp { get; private set; }

        public static string InstanceId { get; private set; }

        public static string OwnerIp { get; private set; }

        public static string Region { get; private set; }

        public static string OverworldAmi { get; private set; }

        public static string OverworldInstanceType { get; private set; }

        public static string KeyPairName { get; private set; }

        public static List<string> SecurityGroupIds { get; private set; }

        public static bool Closed { get; private set; } = false;

        public static ManualResetEvent Initialize()
        {
            var resetEvent = new ManualResetEvent(false);
            if (InstanceId != null)
            {
                resetEvent.Set();
                return resetEvent;
            }

            InitEc2Client(resetEvent);
            return resetEvent;
        }

        private static async void InitEc2Client(ManualResetEvent resetEvent)
        {
            Region = await GetRegion();
            InstanceId = await GetInstanceId();
            client = new AmazonEC2Client(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.GetBySystemName(Region));

            var describeRequest = new DescribeInstancesRequest()
            {
                InstanceIds = new List<string>
                {
                    InstanceId
                }
            };
            var response = await client.DescribeInstancesAsync(describeRequest);
            var reservation = response.Reservations[0];
            var instance = reservation.Instances[0];

            SecurityGroupIds = instance.SecurityGroups.Select(_ => _.GroupId).ToList();
            PublicIp = instance.PublicIpAddress;
            KeyPairName = instance.KeyName;
            ServerType = 0;
            foreach (var tag in instance.Tags)
            {
                switch (tag.Key)
                {
                    case "ServerType":
                        ServerType = int.Parse(tag.Value);
                        break;
                    case "OwnerIp":
                        OwnerIp = tag.Value;
                        break;
                    case "ServerName":
                        ServerName = tag.Value;
                        break;
                    case "PingHost":
                        PingHost = tag.Value;
                        break;
                    case "OverworldAmi":
                        OverworldAmi = tag.Value;
                        break;
                    case "OverworldInstanceType":
                        OverworldInstanceType = tag.Value;
                        break;
                    case "Closed":
                        Closed = true;
                        break;
                }
            }

            resetEvent.Set();
        }

        private static async Task<string> GetRegion()
        {
            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/latest/meta-data/placement/region");
            var response = await client.SendAsync(message);

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> GetInstanceId()
        {
            var client = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/latest/meta-data/instance-id");
            var response = await client.SendAsync(message);

            return await response.Content.ReadAsStringAsync();
        }

        public static async void Terminate()
        {
            await TerminateAsync();
        }

        public static async Task TerminateAsync()
        {
            var toTerminate = new List<string>() { InstanceId };
            await client.TerminateInstancesAsync(new TerminateInstancesRequest(toTerminate));
        }

        public static async void FlagAsClosed()
        {
            await client.CreateTagsAsync(new CreateTagsRequest()
            {
                Resources = new List<string> { InstanceId },
                Tags = new List<Tag>
                {
                    new Tag("Closed", "true")
                }
            });
        }
    }
}
