using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TitanCore.Net;
using TitanCore.Net.Packets;
using Utils.NET.Logging;
using Utils.NET.Net.Tcp;
using Utils.NET.Net.Udp;

namespace World.Net
{
    public class ClientManager : NetListener<Client, TnPacket>//UdpListener<Client, TnPacket>
    {
        public WorldModule module;

        private int maxClients;

        //private ConcurrentDictionary<IPEndPoint, Client> clients = new ConcurrentDictionary<IPEndPoint, Client>();

        public ClientManager(WorldModule module, int maxClients) : base(NetConstants.Game_Connection_Port)
        {
            this.module = module;
            this.maxClients = maxClients;
        }

        public override void Start()
        {
            base.Start();

            Log.Write("Client Manager listening on port: " + NetConstants.Game_Connection_Port);
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override void HandleConnection(Client connection)
        {
            if (module.closed)
            {
                connection.Disconnect();
                return;
            }

            Log.Write("Connection received: " + connection.RemoteAddress);
            connection.SetManager(this);
            connection.ReadAsync();
        }

        /*
        protected override void HandleDisconnection(Client connection)
        {
            Log.Write("Client disconnected: " + connection.RemoteAddress);
            c = null;
        }
        */
    }
}
