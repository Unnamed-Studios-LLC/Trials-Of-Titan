using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Net.Web;
using UnityEngine;

public class ServerPinger
{
    private struct ServerPing
    {
        public Ping ping;

        public WebServerInfo info;

        public ServerPing(WebServerInfo info)
        {
            ping = new Ping(info.pingHost);
            this.info = info;
        }
    }

    public bool isDone = false;

    public WebServerInfo bestServer;

    private List<ServerPing> pings = new List<ServerPing>();

    public ServerPinger()
    {
        foreach (var server in Account.describe.servers)
        {
            pings.Add(new ServerPing(server));
        }
    }

    public void Update()
    {
        if (isDone) return;

        foreach (var server in pings)
        {
            if (server.ping.isDone) continue;
            if (server.ping.time > 300) continue;
            return;
        }

        isDone = true;
        bestServer = pings.OrderBy(_ => GetPriority(_.ping.time, _.info.status)).First().info;
    }

    private int GetPriority(int time, ServerStatus status)
    {
        int priority = 0;
        if (time > 120)
            priority++;
        if (time > 200)
            priority++;

        if (status == ServerStatus.Crowded)
            priority++;
        if (status == ServerStatus.Full)
            priority += 3;
        return priority;
    }
}