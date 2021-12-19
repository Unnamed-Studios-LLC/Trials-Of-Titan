using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Logging;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class EmoteValue
    {
        public object cooldownValue;
    }

    public class Emote : LogicAction<EmoteValue>
    {
        private EmoteType emoteType;

        private Cooldown cooldown;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "type":
                    var emoteString = reader.ReadString();
                    if (!Enum.TryParse(emoteString, out emoteType))
                    {
                        Log.Write($"Unable to parse emote: " + emoteString);
                        return true;
                    }
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out EmoteValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new EmoteValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref EmoteValue obj, ref StateContext context, ref WorldTime time)
        {
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                entity.emote.Value = emoteType;
            }
        }
    }
}
