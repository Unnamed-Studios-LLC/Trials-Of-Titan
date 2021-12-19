using Newtonsoft.Json.Linq;
using ServerNode.Net;
using ServerNode.Net.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Utils.NET.IO;
using Utils.NET.Logging;
using Utils.NET.Modules;

namespace ServerBroadcaster
{
    public class ProgramDefinition
    {
        public string programDirectory;

        public int programType;

        private Manifest programManifest;

        public List<string> endpoints = new List<string>();

        public ProgramDefinition(JToken json)
        {
            programDirectory = json.Value<string>("directory");
            programManifest = Manifest.Load(programDirectory);
            programType = programManifest.Value<int>("programType", -1);

            if (programType == -1)
                Log.Error("Program manifest missing programType definition: " + programDirectory);

            foreach (var value in json.Value<JArray>("endpoints"))
            {
                endpoints.Add(value.Value<string>());
            }

            Log.Write("Loaded program: " + programDirectory);
        }

        private List<NodeConnection> ConnectToNodes()
        {
            var list = new List<NodeConnection>();

            foreach (var endpoint in endpoints)
            {
                var connection = new NodeConnection(true);
                if (!connection.Connect(endpoint, NodeConnection.Port))
                {
                    Log.Error($"Unable to enstablish node connection to endpoint: {endpoint}");
                }
                else
                {
                    Log.Write("Connected to node: " + endpoint, ConsoleColor.Green);
                    connection.Send(new NNodeVerify());
                    connection.ReadAsync();
                    list.Add(connection);
                }
            }

            if (list.Count != endpoints.Count)
            {
                Log.Error("Failed to connect to all nodes");
                return null;
            }

            return list;
        }

        private NUpdate PackageUpdate()
        {
            var update = new NUpdate();

            update.programType = programType;

            string zipPath = "temp.zip";
            ZipFile.CreateFromDirectory(programDirectory, zipPath);

            update.checksum = Checksum.MD5(zipPath);
            update.zip = File.ReadAllBytes(zipPath);

            File.Delete(zipPath);

            return update;
        }

        public void PushUpdate()
        {
            Log.Write("Packaging Program: " + programDirectory);
            Log.Step();

            var update = PackageUpdate();
            var versionCheck = new NVersionCheck(update.programType, update.checksum);

            Log.Write("Connecting to nodes...");
            Log.Step();

            var connections = ConnectToNodes();

            if (connections == null) return;

            Log.Write("Sending version check...");
            Log.Step();

            var resetEvent = new ManualResetEvent(false);
            bool error = false;
            var needsUpdates = new List<NodeConnection>();

            lock (connections)
            {
                foreach (var connection in connections)
                {
                    connection.SendToken(versionCheck, (packet, c) =>
                    {
                        if (!(packet is NVersionCheckResponse response))
                        {
                            Log.Write("Received invalid response from endpoint: " + c.RemoteEndPoint.Address);
                            Log.Step();
                            error = true;
                        }
                        else
                        {
                            if (!response.needsUpdate)
                            {
                                connection.Disconnect();
                                Log.Write($"{connection.RemoteEndPoint.Address} is updated", ConsoleColor.Green);
                                Log.Step();
                            }
                            else
                            {
                                Log.Write($"{connection.RemoteEndPoint.Address} requires an update", ConsoleColor.Yellow);
                                Log.Step();

                                lock (needsUpdates)
                                {
                                    needsUpdates.Add(connection);
                                }
                            }

                            lock (connections)
                            {
                                connections.Remove(connection);
                                if (connections.Count == 0)
                                    resetEvent.Set();
                            }
                        }
                    });
                }
            }

            Log.Write("Awaiting version responses...");
            Log.Step();

            if (!resetEvent.WaitOne(25000))
            {
                Log.Write("Failed to receive a version check response from all connections");
                Log.Step();
                return;
            }

            if (error)
            {
                Log.Write("Failed to successfully version check with all nodes");
                Log.Step();
                return;
            }

            Log.Write("Sending updates...");
            Log.Step();

            lock (needsUpdates)
            {
                foreach (var connection in needsUpdates)
                {
                    connection.SendToken(update, (packet, c) =>
                    {
                        if (!(packet is NUpdateResponse response))
                        {
                            Log.Write("Received invalid response from endpoint: " + c.RemoteEndPoint.Address);
                            error = true;
                        }
                        else
                        {
                            if (response.success)
                            {
                                Log.Write($"{connection.RemoteEndPoint.Address} successfully updated", ConsoleColor.Green);
                                Log.Step();
                            }
                            else
                            {
                                Log.Error($"{connection.RemoteEndPoint.Address} failed to update");
                                Log.Step();
                            }

                            connection.Disconnect();
                            lock (needsUpdates)
                            {
                                needsUpdates.Remove(connection);
                                if (needsUpdates.Count == 0)
                                    resetEvent.Set();
                            }
                        }
                    });
                }
            }

            Log.Write("Awaiting update responses...");
            Log.Step();

            if (!resetEvent.WaitOne(60000))
            {
                Log.Write("Failed to receive a version check response from all connections");
                Log.Step();
                return;
            }

            Log.Write("Completed update of: " + programDirectory);
            Log.Step();
        }
    }
}
