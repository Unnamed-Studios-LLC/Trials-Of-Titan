using Utils.NET.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using Utils.NET.Logging;

namespace ServerBroadcaster
{
    public class ServerBroadcasterModule : Module
    {
        public override string Name => "broadcaster";

        private List<ProgramDefinition> programs = new List<ProgramDefinition>();

        public override void OnCommand(string command, string[] args)
        {
            switch (command)
            {
                case "update":
                    int programType = -1;
                    if (args.Length > 0)
                    {
                        if (!int.TryParse(args[0], out programType))
                        {
                            Log.Error("Failed to parse program type: " + args[0]);
                            break;
                        }
                    }
                    SendUpdate(programType);
                    break;
            }
        }

        private void SendUpdate(int programType)
        {
            foreach (var program in programs)
            {
                if (programType != -1 && program.programType != programType) continue;
                program.PushUpdate();
            }
        }

        public override void Start()
        {
            foreach (var json in ModularProgram.manifest.Value<JArray>("programs", null))
            {
                programs.Add(new ProgramDefinition(json));
            }

            programs = programs.OrderBy(_ => _.programType).ToList();
        }

        public override void Stop()
        {

        }
    }
}
