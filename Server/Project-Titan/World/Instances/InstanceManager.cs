using Amazon.EC2;
using Amazon.EC2.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Net;
using TitanCore.Net.Web;
using TitanDatabase;
using TitanDatabase.Broadcasting;
using TitanDatabase.Broadcasting.Packets;
using TitanDatabase.Instances;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;
using Utils.NET.Utils;
using World.Instances.Packets;
using World.Map.Objects.Map;
using World.Worlds;

namespace World.Instances
{
    public class InstanceManager : NetListener<ManagerToInstanceConnection, InPacket>
    {
        private class InstanceInfo
        {
            public ManagerToInstanceConnection connection;

            public string name;

            public string instanceId;

            public int playerCount;
        }

        public const int Max_Overworlds = 5;

        public const int Port = 6435;

        public WorldModule module;

        private List<ManagerToInstanceConnection> unverifiedConnections = new List<ManagerToInstanceConnection>();

        private ConcurrentDictionary<string, InstanceInfo> instances = new ConcurrentDictionary<string, InstanceInfo>();

        private bool spawningOverworld = false;

        private Timer webServerUpdateTimer;

        private string webServerHost;

        private int nextInstanceId;

        private List<string> portalNames = new List<string>()
        {
            "Gaea",
            "Uranus",
            "Oceanus",
            "Coeus",
            "Crius",
            "Hyperion",
            "Iapetus",
            "Cronus",
            "Thea",
            "Rhea",
            "Themis",
            "Mnemosyne",
            "Phoebe",
            "Tethys"
        };

        public InstanceManager() : base(Port)
        {
        }

        public override void Start()
        {
            base.Start();

            webServerHost = ModularProgram.manifest.Value("webServer", "https://local.trialsoftitan.com:8443/");

            if (ModularProgram.manifest.local)
            {
                var localInfo = new InstanceInfo
                {
                    connection = null,
                    name = GetServerName(),
                    instanceId = "local",
                    playerCount = 0
                };
                instances.TryAdd(localInfo.instanceId, localInfo);
                return;
            }

            var resetEvent = new ManualResetEvent(false);
            RemoveOldInstances(resetEvent);
            resetEvent.WaitOne();

            webServerUpdateTimer = new Timer(OnWebServerUpdate, null, 5000, 5000);
        }

        public override void Stop()
        {
            base.Stop();

            webServerUpdateTimer?.Dispose();
        }

        private void EvaluateOverworldNeeds()
        {
            if (spawningOverworld) return;
            spawningOverworld = true;

            int instanceCount = instances.Count;
            int capacity = NetConstants.Max_Overworld_Players * instanceCount;
            int nexusCount = module.worldManager.GetPlayerCount();
            int overworldPlayers = GetOverworldPlayerCount();

            int openSpaces = capacity - overworldPlayers;
            Log.Write(openSpaces);
            if (openSpaces < 20 && instanceCount < Max_Overworlds)
            {
                SpawnOverworld();
            }
            else
            {
                spawningOverworld = false;
            }
        }

        private async void SpawnOverworld()
        {
            var name = Ec2Instance.ServerName + '.' + GetServerName();
            var launchRequest = new RunInstancesRequest()
            {
                ImageId = Ec2Instance.OverworldAmi,
                InstanceType = Ec2Instance.OverworldInstanceType,
                MinCount = 1,
                MaxCount = 1,
                KeyName = Ec2Instance.KeyPairName,
                SecurityGroupIds = Ec2Instance.SecurityGroupIds,
                TagSpecifications = new List<TagSpecification>
                {
                    new TagSpecification
                    {
                        ResourceType = ResourceType.Instance,
                        Tags = new List<Tag>
                        {
                            new Tag("OwnerIp", Ec2Instance.PublicIp),
                            new Tag("ServerType", "1"),
                            new Tag("Name", name),
                            new Tag("ServerName", name),
                        }
                    }
                }
            };

            var response = await Ec2Instance.client.RunInstancesAsync(launchRequest);
            foreach (var instance in response.Reservation.Instances)
            {
                instances.TryAdd(instance.InstanceId, new InstanceInfo
                {
                    instanceId = instance.InstanceId,
                    playerCount = 0,
                    connection = null,
                    name = name
                });
            }

            spawningOverworld = false;
        }

        private string GetServerName()
        {
            lock (portalNames)
            {
                var index = Rand.Next(portalNames.Count);
                var name = portalNames[index];
                portalNames.RemoveAt(index);
                return name;
            }
        }

        private void ReturnServerName(string name)
        {
            lock (portalNames)
            {
                portalNames.Add(name);
            }
        }

        private int GetOverworldPlayerCount()
        {
            int count = 0;
            foreach (var instancePair in instances.ToArray())
                count += instancePair.Value.playerCount;
            return count;
        }

        private ServerStatus GetStatus()
        {
            var count = module.worldManager.GetPlayerCount();

            if (count >= 190)
                return ServerStatus.Full;
            else if (count >= 120)
                return ServerStatus.Crowded;
            else
                return ServerStatus.Normal;
        }

        private async void OnWebServerUpdate(object state)
        {
            EvaluateOverworldNeeds();

            try
            {
                var http = new HttpClient();
                var query = new Dictionary<string, string>()
                {
                    { "auth", Tokens.Server_Auth_Token },
                    { "name", Ec2Instance.ServerName },
                    { "host", Ec2Instance.PublicIp },
                    { "pingHost", Ec2Instance.PingHost },
                    { "status", GetStatus().ToString() }
                };

                var content = new FormUrlEncodedContent(query);
                var response = await http.PostAsync(webServerHost + "v1/server/update", content);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private async void RemoveOldInstances(ManualResetEvent resetEvent)
        {
            var describeRequest = new DescribeInstancesRequest()
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                         Name = "tag:OwnerIp",
                         Values = new List<string>
                         {
                             Ec2Instance.PublicIp
                         }
                    }
                }
            };
            var response = await Ec2Instance.client.DescribeInstancesAsync(describeRequest);

            var toTerminate = new List<string>();
            foreach (var reservation in response.Reservations)
                foreach (var instance in reservation.Instances)
                    toTerminate.Add(instance.InstanceId);

            if (toTerminate.Count > 0)
                await Ec2Instance.client.TerminateInstancesAsync(new TerminateInstancesRequest(toTerminate));

            resetEvent.Set();
        }

        protected override void HandleConnection(ManagerToInstanceConnection connection)
        {
            connection.manager = this;
            lock (unverifiedConnections)
            { 
                unverifiedConnections.Add(connection);
            }
            connection.ReadAsync();
        }

        public void PlayerCountUpdate(string instanceId, int count)
        {
            if (!instances.TryGetValue(instanceId, out var info)) return;
            info.playerCount = count;
        }

        public void Verify(ManagerToInstanceConnection connection)
        {
            RemoveUnverifiedConnection(connection);

            if (!instances.TryGetValue(connection.instanceId, out var info))
            {
                connection.Disconnect();
                return;
            }

            Log.Write("Received Instance Connection");

            if (!module.worldManager.TryGetWorld(1, out var nexusWorld))
            {
                connection.Disconnect();
                return;
            }

            info.connection = connection;
            connection.portalName = info.name.Split('.').Last();
            var nexus = (Nexus)nexusWorld;
            nexus.PushTickAction(() =>
            {
                if (info.connection == null) return;
                if (info.connection.overworldPortal != null) return;
                if (ModularProgram.manifest.local)
                    info.connection.overworldPortal = nexus.AddOverworldPortal(info.connection.portalName, null, 2);
                else
                    info.connection.overworldPortal = nexus.AddOverworldPortal(info.connection.portalName, info.connection.instanceId, 1);
            });
        }

        public void RemoveUnverifiedConnection(ManagerToInstanceConnection connection)
        {
            lock (unverifiedConnections)
            {
                unverifiedConnections.Remove(connection);
            }
        }

        public void InstanceDisconnected(ManagerToInstanceConnection connection)
        {
            var id = connection.instanceId;
            if (id == null) return;

            if (!instances.TryGetValue(id, out var info))
            {
                return;
            }

            RemovePortal(connection);

            connection.manager = null;
            info.connection = null;
        }

        public void InstanceOverworldClosed(ManagerToInstanceConnection connection)
        {
            ReturnServerName(connection.portalName);
            RemovePortal(connection);

            if (!instances.TryRemove(connection.instanceId, out var info))
            {
                return;
            }
        }

        private void RemovePortal(ManagerToInstanceConnection connection)
        {
            if (connection.overworldPortal != null)
            {
                var portal = connection.overworldPortal;
                connection.overworldPortal = null;

                portal.world.PushTickAction(() =>
                {
                    var nexus = (Nexus)portal.world;
                    portal.RemoveFromWorld();
                    nexus.ReturnPortalPosition(portal.position.Value);
                });
            }
        }

        public void GetTransferKey(string instanceId, ulong accountId, uint worldId, Action<string, ulong> callback)
        {
            if (!instances.TryGetValue(instanceId, out var instance) || instance.connection == null || instance.connection.Disconnected)
            {
                callback?.Invoke(null, 0);
                return;
            }

            instance.connection.RequestWorldKey(accountId, worldId, callback);
        }
    }
}
