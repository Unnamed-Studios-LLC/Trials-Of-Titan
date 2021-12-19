using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetMusicValue
    {

    }

    public class SetMusic : LogicAction<SetMusicValue>
    {
        private string musicName;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    musicName = reader.ReadString();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out SetMusicValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetMusicValue();
            entity.world.ChangeMusic(musicName);
        }

        public override void Tick(Entity entity, ref SetMusicValue obj, ref StateContext context, ref WorldTime time)
        {

        }
    }
}
