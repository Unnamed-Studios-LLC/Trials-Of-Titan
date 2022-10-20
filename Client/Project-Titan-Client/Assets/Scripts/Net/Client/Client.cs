using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;
using TitanCore.Net;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;
using UnityEngine;
using Utils.NET.Crypto;
using Utils.NET.Logging;
using Utils.NET.Net.Tcp;
using Utils.NET.Net.Udp;
using Utils.NET.Net.Udp.Reliability;

public class Client : NetConnection<TnPacket>//UdpClient<TnPacket>
{
    private static Rsa rsa = new Rsa(NetConstants.Rsa_Public_Key, false);

    //private const string Game_Host = "3.17.35.212";

    private const string Game_Host = "127.0.0.1";

    public static string RsaEncrypt(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var encryptedBytes = rsa.Encrypt(bytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Ordered packet channel
    /// </summary>
    //private OrderedReliableChannel<TnPacket> orderedChannel;

    /// <summary>
    /// Called upon connection completion
    /// </summary>
    private Action<bool> connectCallback;

    private string host;

    public HashSet<PacketHandler>[] packetHandlers = new HashSet<PacketHandler>[256];

    public World world;

    public Client(string host, World world)
    {
        this.host = host;
        this.world = world;
        SetupInternalHandlers();
    }

    private void SetupInternalHandlers()
    {
        AddHandler<TnPing>(HandlePing);
    }

    public void Connect(Action<bool> callback)
    {
        connectCallback = callback;
        ConnectAsync(host, NetConstants.Game_Connection_Port, HandleConnected);
    }

    private void HandleConnected(bool connected, NetConnection<TnPacket> connection)
    {
        Debug.Log("Client connection result: " + connected);

        if (connected)
            ReadAsync();

        connectCallback?.Invoke(connected);
        connectCallback = null;
    }

    private void HandlePing(TnPing ping)
    {
        SendAsync(new TnPong(world.clientTickId)); // TODO send real time
    }

    protected override void OnDisconnect()
    {
        Debug.Log("Client disconnected");
    }

    protected override void HandlePacket(TnPacket packet)
    {
        var handlerGroup = packetHandlers[packet.Id];
        if (handlerGroup == null) return;

        lock (handlerGroup)
        {
            foreach (var handler in handlerGroup)
            {
                handler.Handle(packet);
            }
        }
    }

    public void AddHandler<T>(Action<T> func) where T : TnPacket
    {
        var type = GetPacketTypeFromHandler(func);
        var handlerGroup = packetHandlers[(byte)type] ?? CreateHandlerGroup(type);
        var handler = new PacketHandler<T>(func);

        lock (handlerGroup)
        {
            handlerGroup.Add(handler);
        }
    }

    public void RemoveHandler<T>(Action<T> func) where T : TnPacket
    {
        var type = GetPacketTypeFromHandler(func);
        var handlerGroup = packetHandlers[(byte)type];
        var handler = new PacketHandler<T>(func);

        lock (handlerGroup)
        {
            handlerGroup.Remove(handler);
        }
    }

    public void ClearHandlers()
    {
        packetHandlers = new HashSet<PacketHandler>[256];
    }

    private HashSet<PacketHandler> CreateHandlerGroup(TnPacketType type)
    {
        var group = new HashSet<PacketHandler>();
        packetHandlers[(byte)type] = group;
        return group;
    }

    private TnPacketType GetPacketTypeFromHandler<T>(Action<T> func) where T : TnPacket
    {
        var packetType = func.GetType().GetGenericArguments()[0];
        return ((TnPacket)Activator.CreateInstance(packetType)).Type;
    }
}
