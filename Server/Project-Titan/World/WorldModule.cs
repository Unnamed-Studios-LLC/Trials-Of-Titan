using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Net;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Client;
using Utils.NET.Crypto;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Udp;
using Utils.NET.Net.Udp.Reliability;
using TitanDatabase;
using TitanDatabase.Models;
using World.Logic.Reader;
using World.Logic.States;
using World.Net;
using WorldGen;
using World.Logic;
using TitanDatabase.Leaderboards;
using Utils.NET.Utils;
using TitanDatabase.Email;
using World.Instances;
using TitanDatabase.Instances;
using World.Instances.Packets;
using World.Worlds;
using World.Worlds.Gates;
using TitanCore.Net.Packets.Server;
using TitanDatabase.Broadcasting.Packets;
using TitanDatabase.Broadcasting;
using Utils.NET.Net.Tcp;
using TitanCore.Net.Packets.Models;
using System.Threading;
using System.Linq;
using World.Looting;
using TitanCore.Data.Items;
using TitanCore.Core;
using Utils.NET.Collections;
using Utils.NET.IO;
using Utils.NET.Geometry;

namespace World
{
    public class WorldModule : Module
    {
        public override string Name => "World";

        /// <summary>
        /// Manages worlds and updating game state
        /// </summary>
        public WorldManager worldManager;

        /// <summary>
        /// Manages connection and disconnection of clients
        /// </summary>
        public ClientManager clientManager;

        public InstanceManager instanceManager;

        public InstanceToManagerConnection instanceConnection;

        private BroadcastConnection broadcastConnection;

        private Timer shutdownTimer;

        public bool closed = false;

        public static string ServerName => Ec2Instance.ServerName ?? "Local";

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            BugReporter.Setup(Emailer.SendBugReport);

            Log.Write("World Version: " + NetConstants.Build_Version, ConsoleColor.Magenta);

            try
            {
                Database.Initialize().WaitOne();
            }
            catch (FailedToCreateClientException)
            {
                Log.Write("Failed to create client!");
            }

            var startWorlds = new List<World>();
            bool local = ModularProgram.manifest.Value<bool>("local", true);
            if (!local)
            {
                Log.Write("Initializing Ec2 Client");
                Ec2Instance.Initialize().WaitOne();

                if (Ec2Instance.Closed)
                {
                    Log.Write("This server has been closed. Terminating..");
                    Ec2Instance.Terminate();
                    new ManualResetEvent(false).WaitOne();
                }

                if (Ec2Instance.ServerType == 0)
                {
                    Log.Write("Listening for overworld servers...");

                    instanceManager = new InstanceManager();
                    instanceManager.module = this;
                    instanceManager.Start();

                    startWorlds.Add(new Nexus());
                }
                else
                {
                    Log.Write("Connecting to main server...");

                    instanceConnection = new InstanceToManagerConnection();
                    instanceConnection.module = this;
                    instanceConnection.Connect(Ec2Instance.OwnerIp, InstanceManager.Port);
                    instanceConnection.ReadAsync();
                    instanceConnection.Send(new InVerify
                    {
                        instanceId = Ec2Instance.InstanceId
                    });

                    startWorlds.Add(new Overworld());
                }
            }
            else
            {
                startWorlds.Add(new Nexus());
                startWorlds.Add(new Overworld());
                startWorlds.Add(new BhogninsGate());
            }

            GameData.LoadDirectory("Data/Xmls/", false);
            EntityState.LoadLogic("Logic/Scripts/");

            LeaderboardManager.Initialize().WaitOne();

            DropTables.InitTables();

            worldManager = new WorldManager(); // init and start the world manager
            worldManager.module = this;
            worldManager.Start(startWorlds);

            clientManager = new ClientManager(this, 100); // init and start the client manager
            clientManager.Start();

            StartBroadcastConnection();

            if (local)
            {
                instanceManager = new InstanceManager();
                instanceManager.module = this;
                instanceManager.Start();

                instanceConnection = new InstanceToManagerConnection();
                instanceConnection.module = this;
                instanceConnection.Connect("127.0.0.1", InstanceManager.Port);
                instanceConnection.ReadAsync();
                instanceConnection.Send(new InVerify
                {
                    instanceId = "local"
                });

                //LogDps();

                var nexus = (Nexus)startWorlds[0];
                nexus.AddPortal(startWorlds[2]);
            }
        }

        private void LogDps()
        {
            var str = new StringBuilder("Weapon Dps:\n");

            char sep = '=';
            foreach (var info in GameData.objects.Values.Where(_ => _ is WeaponInfo))
            {
                int count = 0;
                var damage = new Range(0, 0);
                var weapon = (WeaponInfo)info;

                foreach (var proj in weapon.projectiles)
                {
                    count++;
                    WeaponFunctions.GetProjectileDamage(weapon.slotType, proj, out var min, out var max);
                    damage.min += min * proj.amount;
                    damage.max += max * proj.amount;
                }

                var damagePerShot = new Range(damage.min / count, damage.max / count);
                var damagePerSecond = new Range(damagePerShot.min * weapon.rateOfFire, damagePerShot.max * weapon.rateOfFire);

                str.AppendLine($"{weapon.name.PadRight(34, sep)} Per Shot: {damagePerShot.Average().ToString().PadRight(10, sep)} Per Second: {damagePerSecond.Average().ToString().PadRight(10, sep)}");
                if (sep == '=')
                    sep = ' ';
                else
                    sep = '=';
            }

            Log.Write(str.ToString(), ConsoleColor.Green);
        }

        public override void Stop()
        {
            clientManager.Stop(); // stops the client manager

            worldManager.Stop().GetAwaiter().GetResult(); // stops the world manager

            if (broadcastConnection != null)
            {
                broadcastConnection.SetDisconnectCallback(null);
                broadcastConnection.Disconnect();
            }

            instanceManager?.Stop();
        }

        public void SendToNexus(Client client)
        {
            client.SendAsync(new TnReconnect("Nexus", Ec2Instance.OwnerIp, 0, 1));
        }

        private void StartBroadcastConnection()
        {
            Log.Write("Connecting to broadcaster...");
            broadcastConnection = new BroadcastConnection();
            broadcastConnection.ConnectAsync(ModularProgram.manifest.Value("webServerIp", "127.0.0.1"), BroadcastListener.Port, BroadcasterConnected);
        }

        private async void BroadcasterConnected(bool success, NetConnection<BrPacket> net)
        {
            var connection = (BroadcastConnection)net;
            if (!success)
            {
                Log.Error("Broadcaster connection failed. Retrying in 5 seconds");
                connection.Disconnect();
                await Task.Delay(5000);
                StartBroadcastConnection();
            }
            else
            {
                Log.Write("Broadcaster connected!");
                connection.SendAsync(new BrVerify(ServerName));
                connection.SetDisconnectCallback(BroadcasterDisconnected);
                connection.packetHandler = BroadcastReceivedPacket;
                broadcastConnection.ReadAsync();
            }
        }

        private async void BroadcasterDisconnected(NetConnection<BrPacket> net)
        {
            var connection = (BroadcastConnection)net;
            connection.SetDisconnectCallback(null);

            Log.Error("Broadcaster connection disconnected. Reconnecting in 5 seconds");
            await Task.Delay(5000);
            StartBroadcastConnection();
        }

        public void BroadcastReceivedPacket(BrPacket packet)
        {
            switch (packet.Type)
            {
                case BrPacketType.Message:
                    ReceivedBroadcastMessage((BrMessage)packet);
                    break;
                case BrPacketType.GiveGold:
                    ReceivedGiveGold((BrGiveGold)packet);
                    break;
                case BrPacketType.ServerMessage:
                    ReceivedServerMessage((BrServerMessage)packet);
                    break;
            }
        }

        private void ReceivedBroadcastMessage(BrMessage message)
        {
            BroadcastReceivedPacket(message.packet);
        }

        private void ReceivedGiveGold(BrGiveGold giveGold)
        {
            worldManager.DispatchWorldAction(worlds =>
            {
                foreach (var world in worlds)
                {
                    if (!world.objects.TryGetPlayer(giveGold.accountId, out var player)) continue;
                    player.client.account.premiumCurrency += giveGold.amount;
                    broadcastConnection.SendTokenResponseAsync(new BrGiveGoldResponse(true)
                    {
                        Token = giveGold.Token
                    });
                    return;
                }

                broadcastConnection.SendTokenResponseAsync(new BrGiveGoldResponse(false)
                {
                    Token = giveGold.Token
                });
            });
        }

        private void ReceivedServerMessage(BrServerMessage serverMessage)
        {
            worldManager.DispatchWorldAction(worlds =>
            {
                foreach (var world in worlds)
                {
                    foreach (var player in world.objects.players.Values)
                        player.AddChat(ChatData.Info("[SERVER] " + serverMessage.message));
                }
            });
        }

        private DateTime shutdownTime;

        public void StartShutdown()
        {
            closed = true;
            if (!ModularProgram.manifest.local)
            {
                Ec2Instance.FlagAsClosed();
                instanceConnection.SendAsync(new InOverworldClosed());
            }
            shutdownTime = DateTime.Now.AddMinutes(6);
            shutdownTimer = new Timer(OnShutdownTimer, null, 1100 * 60, 1000 * 60);
        }

        private bool nexused = false;

        private void OnShutdownTimer(object state)
        {
            if (DateTime.Now < shutdownTime)
            {
                // warn players of time
                var message = $"World closing in {(int)Math.Round((shutdownTime - DateTime.Now).TotalMinutes)} minutes";
                worldManager.DispatchWorldAction(worlds =>
                {
                    foreach (var world in worlds)
                        foreach (var player in world.objects.players.Values)
                            player.AddChat(ChatData.Info(message));
                });
            }
            else if (!nexused)
            {
                nexused = true;
                worldManager.DispatchWorldAction(worlds =>
                {
                    foreach (var world in worlds)
                    {
                        foreach (var player in world.objects.players.Values.ToArray())
                            player.client.Nexus();
                    }
                });
            }
            else
            {
                if (!ModularProgram.manifest.local)
                {
                    Ec2Instance.Terminate();
                }
            }
        }
    }
}
