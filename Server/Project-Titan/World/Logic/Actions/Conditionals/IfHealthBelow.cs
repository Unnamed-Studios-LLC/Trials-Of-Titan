using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfHealthBelowValue
    {
        public bool triggered = false;
    }

    public class IfHealthBelow : Conditional<IfHealthBelowValue>
    {
        private int healthValue = int.MaxValue;

        private float percent = float.MaxValue;

        private bool trigger = false;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "value":
                    healthValue = reader.ReadInt();
                    return true;
                case "percent":
                    percent = reader.ReadFloat();
                    return true;
                case "trigger":
                    trigger = reader.ReadBool();
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void Init(Entity entity, out IfHealthBelowValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfHealthBelowValue();
        }

        protected override bool Condition(Entity entity, ref IfHealthBelowValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is Enemy enemy)) return false;
            if (trigger && obj.triggered) return false;
            var hp = enemy.GetHealth();
            bool value = hp < healthValue && ((float)hp / enemy.maxHealth.Value) < percent;
            obj.triggered = value;
            return value;
        }
    }
}
