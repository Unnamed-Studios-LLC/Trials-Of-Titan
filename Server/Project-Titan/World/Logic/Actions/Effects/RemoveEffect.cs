using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Effects
{
    public class RemoveEffectValue
    {
        public object cooldownValue;
    }

    public class RemoveEffect : LogicAction<RemoveEffectValue>
    {
        /// <summary>
        /// The effect to apply
        /// </summary>
        public StatusEffect effectType;

        /// <summary>
        /// Cooldown between applying the effect
        /// </summary>
        public Cooldown cooldown = new Cooldown();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "type":
                    effectType = (StatusEffect)Enum.Parse(typeof(StatusEffect), reader.ReadString());
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out RemoveEffectValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new RemoveEffectValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref RemoveEffectValue obj, ref StateContext context, ref WorldTime time)
        {
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                entity.RemoveEffect(effectType);
            }
        }
    }
}
