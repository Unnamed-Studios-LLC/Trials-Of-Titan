using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Net.Web;
using Utils.NET.Modules;

namespace WebServer.Servers
{
    public class ServerList
    {
        private struct ServerInfo
        {
            public WebServerInfo webInfo;

            public DateTime lastUpdated;
        }

        private ConcurrentDictionary<string, ServerInfo> servers = new ConcurrentDictionary<string, ServerInfo>();

        public WebServerInfo[] infos = new WebServerInfo[0];

        public ServerList()
        {
            if (!ModularProgram.manifest.Value("local", false)) return;
            infos = new WebServerInfo[]
            {
                new WebServerInfo("Local", "127.0.0.1", "127.0.0.1", ServerStatus.Normal)
            };
        }

        private void UpdateInfos()
        {
            var list = new List<WebServerInfo>();
            foreach (var info in servers.ToArray().Select(_ => _.Value))
            {
                if ((DateTime.Now - info.lastUpdated).TotalSeconds > 30)
                    servers.TryRemove(info.webInfo.name, out var v);
                else
                    list.Add(info.webInfo);
            }
            infos = list.OrderBy(_ => _.name).ToArray();
        }

        public void PushUpdate(string name, string host, string pingHost, ServerStatus status)
        {
            servers[name] = new ServerInfo()
            {
                webInfo = new WebServerInfo(name, host, pingHost, status),
                lastUpdated = DateTime.Now
            };
            UpdateInfos();
        }
    }
}
