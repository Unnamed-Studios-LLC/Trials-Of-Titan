using ServerNode.Net;
using ServerNode.Net.Packets;
using System;
using System.Diagnostics;
using Utils.NET.Modules;
using Utils.NET.Net.Tcp;

namespace ProgramNode
{
    public class ProgramNodeModule : Module
    {
        public override string Name => "program";

        private ProgramConnection connection;

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            connection = new ProgramConnection();
            connection.Connect("127.0.0.1", ProgramConnection.Port);
            connection.Send(new NProgramVerify(ModularProgram.manifest.Value("programType", -1), Process.GetCurrentProcess().Id));
            connection.SetDisconnectCallback(ProgramConnectionDisconnected);
        }

        private void ProgramConnectionDisconnected(NetConnection<NPacket> connection)
        {
            ModularProgram.Exit();
        }

        public override void Stop()
        {
            
        }
    }
}
