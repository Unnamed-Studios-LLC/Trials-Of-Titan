using System;
using System.Collections.Generic;
using TitanCore.Data;
using TitanCore.Net;
using TitanDatabase;
using TitanDatabase.Broadcasting.Packets;
using TitanDatabase.Email;
using TitanDatabase.Leaderboards;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Utils;

namespace WebServer
{
    public class WebServerModule : Module
    {
        public override string Name => "Web";

        private WebServer server;

        public override void OnCommand(string command, string[] args)
        {
            switch (command)
            {
                case "msg":
                    var message = StringUtils.ComponentsToString(' ', args);
                    server.broadcastListener.ReceivedMessage(new BrMessage("", new BrServerMessage(message)), null);
                    break;
            }
        }

        public override void Start()
        {
            BugReporter.Setup(Emailer.SendBugReport);

            Log.Write("Web Server Version: " + NetConstants.Build_Version, ConsoleColor.Magenta);

            try
            {
                Database.Initialize().WaitOne();
            }
            catch (FailedToCreateClientException)
            {
                Log.Write("Failed to create database client!");
            }

            GameData.LoadDirectory("Data/Xmls/", false);

            LeaderboardManager.Initialize().WaitOne();

            server = new WebServer();
            server.Start();
        }

        public override void Stop()
        {
            server.Stop();
        }
    }
}
