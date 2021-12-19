using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TitanDatabase.Broadcasting.Packets;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;

namespace TitanDatabase.Broadcasting
{
    public class BroadcastListener : NetListener<BroadcastListenerConnection, BrPacket>
    {
        public const int Port = 9341;

        private ConcurrentDictionary<string, BroadcastListenerConnection> connections = new ConcurrentDictionary<string, BroadcastListenerConnection>();

        private List<BroadcastListenerConnection> unverifiedConnections = new List<BroadcastListenerConnection>();

        public BroadcastListener() : base(Port)
        {
        }

        protected override void HandleConnection(BroadcastListenerConnection connection)
        {
            Log.Write("Received unverified broadcast conneciton.");
            connection.listener = this;
            lock (unverifiedConnections)
            {
                unverifiedConnections.Add(connection);
            }
            connection.ReadAsync();
        }

        public void Verify(BroadcastListenerConnection connection)
        {
            RemoveUnverifiedConnection(connection);
            connections[connection.serverName] = connection;

            Log.Write("Verified broadcast connection: " + connection.serverName);
        }

        public void RemoveUnverifiedConnection(BroadcastListenerConnection connection)
        {
            lock (unverifiedConnections)
            {
                unverifiedConnections.Remove(connection);
            }
        }

        public void ConnectionDisconnected(BroadcastListenerConnection connection)
        {
            connections.TryRemove(connection.serverName, out var d);
        }

        public void ReceivedPacket(BrPacket packet, BroadcastListenerConnection connection)
        {
            switch (packet.Type)
            {
                case BrPacketType.Message:
                    ReceivedMessage((BrMessage)packet, connection);
                    break;
            }
        }

        public void ReceivedMessage(BrMessage message, BroadcastListenerConnection fromConnection)
        {
            if (string.IsNullOrWhiteSpace(message.server))
            {
                foreach (var serverPair in connections.ToArray())
                    if (serverPair.Value != fromConnection)
                        serverPair.Value.SendAsync(message.packet);
            }
            else
            {
                if (connections.TryGetValue(message.server, out var connection))
                    connection.SendAsync(message.packet);
            }
        }

        public bool TryGetConnection(string server, out BroadcastListenerConnection connection)
        {
            return connections.TryGetValue(server, out connection);
        }
    }
}
