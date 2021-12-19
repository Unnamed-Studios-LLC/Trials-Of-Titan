using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Visual
{
    public class SetFlashValue
    {
        public bool flashing;

        public float flashTime;

        public object cooldownValue;
    }

    public class SetFlash : LogicAction<SetFlashValue>
    {
        /// <summary>
        /// The color to flash
        /// </summary>
        public GameColor color = GameColor.white;

        /// <summary>
        /// The duration of the flash
        /// </summary>
        public float duration;

        /// <summary>
        /// Cooldown to apply flash
        /// </summary>
        public Cooldown cooldown = new Cooldown();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "color":
                    color = GameColor.Parse(reader.ReadString());
                    return true;
                case "duration":
                    duration = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return true;
        }

        public override void Init(Entity enemy, out SetFlashValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new SetFlashValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref SetFlashValue obj, ref StateContext context, ref WorldTime time)
        {
            if (obj.flashing)
            {
                obj.flashTime -= (float)time.deltaTime;
                if (obj.flashTime <= 0)
                {
                    obj.flashing = false;
                    entity.flashColor.Value = GameColor.flashClear;
                }
            }

            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                obj.flashing = true;
                obj.flashTime = duration;
                entity.flashColor.Value = color;
            }
        }
    }
}
