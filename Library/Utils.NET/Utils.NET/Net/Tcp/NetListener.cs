using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Utils.NET.Logging;

namespace Utils.NET.Net.Tcp
{
    public abstract class NetListener<TCon, TPacket> 
        where TPacket : Packet
        where TCon : NetConnection<TPacket>
    {
        public IPEndPoint localEndPoint;

        private Socket socket;

        private bool running = false;

        public NetListener(int port)
        {
            localEndPoint = new IPEndPoint(IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            socket.Bind(localEndPoint);

            socket.Listen(5);
        }

        public virtual void Start()
        {
            Log.Write($"{this} is listening on port: {localEndPoint.Port}");
            running = true;
            socket.BeginAccept(OnAcceptCallback, null);
        }

        public virtual void Stop()
        {
            running = false;
            socket.Close();
        }

        private void OnAcceptCallback(IAsyncResult ar)
        {
            Socket remoteSocket;
            try
            {
                remoteSocket = socket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (!running)
            {
                remoteSocket.Dispose();
                return;
            }

            TCon connection = (TCon)Activator.CreateInstance(typeof(TCon), remoteSocket);
            if (connection == null)
            {
                remoteSocket.Close();
                return;
            }
            HandleConnection(connection);

            socket.BeginAccept(OnAcceptCallback, null);
        }

        protected abstract void HandleConnection(TCon connection);
    }
}
