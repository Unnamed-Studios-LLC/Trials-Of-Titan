using ServerNode.Net;
using System;
using Utils.NET.Logging;
using Utils.NET.Modules;

namespace ServerNode
{
    public class NodeModule : Module
    {
        public override string Name => "node";

        private NodeListener nodeListener;

        public ProgramListener programListener;

        public override void OnCommand(string command, string[] args)
        {
            int type;
            switch (command)
            {
                case "update":
                    type = int.Parse(args[0]);
                    programListener.EndProgram(type, ProgramState.Updating);
                    break;
                case "start":
                    type = int.Parse(args[0]);
                    programListener.StartProgram(type);
                    break;
                case "preupdate":
                    type = int.Parse(args[0]);

                    break;
            }
        }

        public override void Start()
        {
            nodeListener = new NodeListener();
            nodeListener.Start();

            Log.Write("Listening for Node broadcasts on port: " + NodeConnection.Port);

            programListener = new ProgramListener();
            programListener.Start();

            Log.Write("Listening for Programs on port: " + ProgramConnection.Port);

            nodeListener.programListener = programListener;
        }

        public override void Stop()
        {
            nodeListener.Stop();
            programListener.Stop();
        }
    }
}
