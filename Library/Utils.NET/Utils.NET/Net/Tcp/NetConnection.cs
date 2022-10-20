using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace Utils.NET.Net.Tcp
{
    public abstract class NetConnection<TPacket> where TPacket : Packet
    {
        public delegate void OnConnectCallback(bool success, NetConnection<TPacket> connection);

        public delegate void OnDisconnectCallback(NetConnection<TPacket> connection);

        public delegate void OnTokenPacketCallback(TPacket packet, NetConnection<TPacket> connection);

        private enum ReceiveState
        {
            Size,
            Payload
        }

        public IPAddress RemoteAddress => ((IPEndPoint)socket.RemoteEndPoint).Address;

        public IPEndPoint RemoteEndPoint => (IPEndPoint)socket.RemoteEndPoint;

        /// <summary>
        /// System socket used to send and receive data
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Buffer used to hold received data
        /// </summary>
        private IO.Buffer buffer;

        /// <summary>
        /// Factory used to create packets from received data
        /// </summary>
        private PacketFactory<TPacket> packetFactory;

        /// <summary>
        /// Value used to syncronize disconnection calls
        /// </summary>
        private int disconnected = 0;

        /// <summary>
        /// True if this connection has been disconnected
        /// </summary>
        public bool Disconnected => disconnected == 1;

        /// <summary>
        /// Delegate to be called upon disconnect
        /// </summary>
        private OnDisconnectCallback disconnectCallback;

        /// <summary>
        /// The next token id to assign
        /// </summary>
        private int nextToken = 0;

        /// <summary>
        /// Dictionary containing callbacks for token packets
        /// </summary>
        private ConcurrentDictionary<int, OnTokenPacketCallback> tokenCallbacks = new ConcurrentDictionary<int, OnTokenPacketCallback>();

        /// <summary>
        /// Queue of packets to send async
        /// </summary>
        private Queue<SendPayload> sendQueue = new Queue<SendPayload>();

        /// <summary>
        /// True if currently sending a packet
        /// </summary>
        private bool sending = false;

        public int maxReceiveSize = int.MaxValue;

        public NetConnection(Socket socket)
        {
            this.socket = socket;
            Init();
        }

        public NetConnection()
        {
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            Init();
        }

        private void Init()
        {
            packetFactory = new PacketFactory<TPacket>();
            socket.NoDelay = true;
            buffer = new IO.Buffer(4);
        }

        #region Connection

        #region Connect

        public bool Connect(string host, int port)
        {
            return Connect(new IPEndPoint(IPAddress.Parse(host).MapToIPv6(), port));
        }

        public bool Connect(EndPoint endPoint)
        {
            try
            {
                socket.Connect(endPoint);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ConnectAsync(string host, int port, OnConnectCallback callback)
        {
            ConnectAsync(new IPEndPoint(IPAddress.Parse(host).MapToIPv6(), port), callback);
        }

        public void ConnectAsync(EndPoint endPoint, OnConnectCallback callback)
        {
            try
            {
                socket.BeginConnect(endPoint, OnConnect, callback);
            }
            catch (Exception e)
            {
                Log.Error(e);
                callback(false, this);
            }
        }

        public void OnConnect(IAsyncResult ar)
        {
            var callback = ar.AsyncState as OnConnectCallback;
            try
            {
                socket.EndConnect(ar);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            callback?.Invoke(socket.Connected, this);
        }

        #endregion

        #region Disconnect 

        public bool Disconnect()
        {
            if (Interlocked.CompareExchange(ref disconnected, 1, 0) == 1) return false; // return if this method was already called
            DoDisconnect();
            return true;
        }

        private void DoDisconnect()
        {
            disconnectCallback?.Invoke(this);
            socket.Close();
            OnDisconnect();
        }

        protected abstract void OnDisconnect();

        /// <summary>
        /// Sets the callback to be called upon disconnection
        /// </summary>
        /// <param name="callback"></param>
        public void SetDisconnectCallback(OnDisconnectCallback callback) => disconnectCallback = callback;

        #endregion

        #endregion

        #region Sending

        private IO.Buffer PackagePacket(TPacket packet)
        {
            BitWriter w = new BitWriter();
            w.Write((int)0); // reserve size int space
            w.Write(packet.Id);
            packet.WritePacket(w);

            var payload = w.GetData();
            System.Buffer.BlockCopy(BitConverter.GetBytes(payload.size - 4), 0, payload.data, 0, 4); // insert size int to the start

            return payload;
        }

        private IO.Buffer PackageTokenPacket(TPacket packet)
        {
            BitWriter w = new BitWriter();
            w.Write((int)0); // reserve size int space
            packet.WriteTokenPacket(w);

            var payload = w.GetData();
            System.Buffer.BlockCopy(BitConverter.GetBytes(payload.size - 4), 0, payload.data, 0, 4); // insert size int to the start

            return payload;
        }

        /// <summary>
        /// Creates a token and stores the callback
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private bool TryAssignToken(TPacket packet, OnTokenPacketCallback callback)
        {
            if (!(packet is ITokenPacket tokenPacket))
            {
                Log.Error($"{packet.GetType().Name} is not an ITokenPacket!");
                return false;
            }

            int token = Interlocked.Increment(ref nextToken);
            tokenCallbacks[token] = callback;
            tokenPacket.Token = token;
            return true;
        }

        #region Sync

        public void Send(TPacket packet)
        {
            var payload = PackagePacket(packet);
            SendBuffer(payload, packet);
        }

        public void SendTokenResponse(TPacket packet)
        {
            if (packet is ITokenPacket token)
                token.TokenResponse = true;
            var payload = PackageTokenPacket(packet);
            SendBuffer(payload, packet);
        }

        public void SendToken(TPacket packet, OnTokenPacketCallback callback)
        {
            if (!TryAssignToken(packet, callback)) return;
            var payload = PackageTokenPacket(packet);
            SendBuffer(payload, packet);
        }

        private void SendBuffer(IO.Buffer buffer, TPacket packet)
        {
            SocketError error;
            try
            {
                socket.Send(buffer.data, 0, buffer.size, SocketFlags.None, out error);
            }
            catch (Exception e)
            {
                Log.Error(e);
                error = SocketError.Disconnecting;
            }

            if (CheckError(error))
            {
                Log.Error("SocketError received on Send: " + error);
                if (packet is ITokenPacket token && !token.TokenResponse)
                    tokenCallbacks.TryRemove(token.Token, out var dummy);
                Disconnect();
                return;
            }
        }

        #endregion

        #region Async

        private class SendPayload
        {
            public IO.Buffer buffer;

            public TPacket packet;
        }

        public void SendAsync(TPacket packet)
        {
            var buffer = PackagePacket(packet);
            SendAsync(new SendPayload
            {
                buffer = buffer,
                packet = packet
            });
        }

        public void SendTokenAsync(TPacket packet, OnTokenPacketCallback callback)
        {
            if (!TryAssignToken(packet, callback)) return;
            SendAsync(new SendPayload
            {
                buffer = PackageTokenPacket(packet),
                packet = packet
            });
        }

        public void SendTokenResponseAsync(TPacket packet)
        {
            if (packet is ITokenPacket token)
                token.TokenResponse = true;
            SendAsync(new SendPayload
            {
                buffer = PackageTokenPacket(packet),
                packet = packet
            });
        }

        private void SendAsync(SendPayload payload)
        {
            lock (sendQueue)
            {
                if (sending)
                {
                    sendQueue.Enqueue(payload);
                    return;
                }
                sending = true;
            }

            SendBufferAsync(payload);
        }

        private void DequeuePayload()
        {
            SendPayload payload;
            lock (sendQueue)
            {
                if (sendQueue.Count == 0)
                {
                    sending = false;
                    return;
                }

                payload = sendQueue.Dequeue();
            }

            SendBufferAsync(payload);
        }

        private void SendBufferAsync(SendPayload payload)
        {
            try
            {
                socket.BeginSend(payload.buffer.data, 0, payload.buffer.size, SocketFlags.None, out SocketError error, OnSend, payload);

                if (CheckError(error))
                {
                    Log.Error("SocketError received on SendAsync: " + error);
                    if (payload.packet is ITokenPacket token && !token.TokenResponse)
                        tokenCallbacks.TryRemove(token.Token, out var dummy);
                    Disconnect();
                    return;
                }
            }
            catch (ObjectDisposedException e)
            {
                Disconnect();
                return;
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            var sentLength = socket.EndSend(ar, out SocketError error);
            var payload = (SendPayload)ar.AsyncState;
            if (CheckError(error))
            {
                Log.Error("SocketError received on SendAsync: " + error);
                if (payload.packet is ITokenPacket token && !token.TokenResponse)
                    tokenCallbacks.TryRemove(token.Token, out var dummy);
                Disconnect();
                return;
            }
            else
            {
                DequeuePayload();
            }
        }

        #endregion

        #endregion

        #region Reading

        protected abstract void HandlePacket(TPacket packet);

        private void ReceivedSize()
        {
            int size = BitConverter.ToInt32(buffer.data, 0);
            if (size > maxReceiveSize)
                Disconnect();
            else
                buffer.Reset(size);
        }

        private void ReceivedPayload()
        {
            byte[] data = buffer.data;
            try
            {
                BitReader r = new BitReader(data, data.Length);
                byte id = r.ReadUInt8();
                TPacket packet = packetFactory.CreatePacket(id);
                if (packet == null)
                {
                    Log.Error($"No {typeof(TPacket).Name} for id: {id}");
                    return;
                }
                if (packet is ITokenPacket token)
                {
                    packet.ReadTokenPacket(r);
                    if (token.TokenResponse)
                        HandleTokenPacket(packet, token.Token);
                    else
                        HandlePacket(packet);
                }
                else
                {
                    try
                    {
                        packet.ReadPacket(r);
                        HandlePacket(packet);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        Disconnect();
                    }
                }
            }
            finally
            {
                buffer.Reset(4);
            }
        }

        private void HandleTokenPacket(TPacket packet, int token)
        {
            if (!tokenCallbacks.TryGetValue(token, out var callback)) return;
            callback(packet, this);
        }

        #region Sync

        public void Read()
        {
            while (socket.Connected)
            {
                ReadSize();
                ReadPayload();
            }
        }

        private void ReadSize()
        {
            if (disconnected == 1) return;
            while (buffer.RemainingLength > 0)
            {
                try
                {
                    socket.Receive(buffer.data, buffer.size, buffer.RemainingLength, SocketFlags.None, out SocketError error);
                    if (CheckError(error))
                    {
                        if (Disconnect())
                            Log.Error("SocketError received on ReadSize: " + error);
                        return;
                    }
                }
                catch (ObjectDisposedException) // socket was already disposed
                {
                    return;
                }
            }

            ReceivedSize();
        }

        private void ReadPayload()
        {
            if (disconnected == 1) return;
            while (buffer.RemainingLength > 0)
            {
                try
                {
                    socket.Receive(buffer.data, buffer.size, buffer.RemainingLength, SocketFlags.None, out SocketError error);
                    if (CheckError(error))
                    {
                        if (Disconnect())
                            Log.Error("SocketError received on ReadPayload: " + error);
                        return;
                    }
                }
                catch (ObjectDisposedException) // socket was already disposed
                {
                    return;
                }
            }

            ReceivedPayload();
        }

        #endregion

        #region Async

        public void ReadAsync()
        {
            BeginReadSize();
        }

        private bool TryEndRead(IAsyncResult ar)
        {
            if (disconnected == 1) return false;
            int length = 0;
            try
            {
                length = socket.EndReceive(ar, out SocketError error);
                if (CheckError(error))
                {
                    if (Disconnect())
                        Log.Error("SocketError received on TryEndRead: " + error);
                    return false;
                }
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
                return false;
            }

            if (length <= 0) // closed socket, disconnect
            {
                Disconnect();
                return false;
            }

            buffer.size += length; // data was read into the buffer, increment the size accordingly
            return true;
        }

        private void BeginReadSize()
        {
            if (disconnected == 1) return;
            try
            {
                socket.BeginReceive(buffer.data, buffer.size, buffer.RemainingLength, SocketFlags.None, out SocketError error, OnReadSizeCallback, null);
                if (CheckError(error))
                {
                    if (Disconnect())
                        Log.Error("SocketError received on BeginReadSize: " + error);
                    return;
                }
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
                return;
            }
        }

        private void OnReadSizeCallback(IAsyncResult ar)
        {
            if (!TryEndRead(ar))
                return;

            if (buffer.RemainingLength > 0)
            {
                BeginReadSize(); // still need more data
            }
            else
            {
                ReceivedSize();
                BeginReadPayload();
            }
        }

        private void BeginReadPayload()
        {
            if (disconnected == 1) return;
            try
            {
                socket.BeginReceive(buffer.data, buffer.size, buffer.RemainingLength, SocketFlags.None, out SocketError error, OnReadPayloadCallback, null);
                if (CheckError(error))
                {
                    if (Disconnect())
                        Log.Error("SocketError received on BeginReadPayload: " + error);
                    return;
                }
            }
            catch (ObjectDisposedException) // socket was already disposed
            {
                return;
            }
        }

        private void OnReadPayloadCallback(IAsyncResult ar)
        {
            if (!TryEndRead(ar))
                return;

            if (buffer.RemainingLength > 0)
            {
                BeginReadPayload(); // need more data
            }
            else
            {
                ReceivedPayload();
                BeginReadSize();
            }
        }

        #endregion

        #endregion

        #region Error Handling

        /// <summary>
        /// Checks if the given error qualifies for socket close
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool CheckError(SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                case SocketError.IOPending:
                case SocketError.WouldBlock:
                    return false;
                default:
                    return true;
            }
        }

        #endregion
    }
}
