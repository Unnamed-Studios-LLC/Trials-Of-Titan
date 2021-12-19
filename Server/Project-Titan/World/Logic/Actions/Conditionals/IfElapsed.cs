using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class IfElapsedValue
    {
        public double elapsedTarget;
    }

    public class IfElapsed : Conditional<IfElapsedValue>
    {
        private double duration
        {
            get
            {
                return durMin;
            }
            set
            {
                durMin = value;
                durMax = value;
            }
        }

        private double durMin = 0;

        private double durMax = 0;

        private bool loop = false;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "ms":
                    duration += reader.ReadFloat() / 1000.0f;
                    return true;
                case "sec":
                    duration += reader.ReadFloat();
                    return true;
                case "min":
                    duration += reader.ReadFloat() * 60f;
                    return true;
                case "hour":
                    duration += reader.ReadFloat() * 60f * 60f;
                    return true;
                case "msMin":
                    durMin += reader.ReadFloat() / 1000.0f;
                    return true;
                case "secMin":
                    durMin += reader.ReadFloat();
                    return true;
                case "minMin":
                    durMin += reader.ReadFloat() * 60f;
                    return true;
                case "hourMin":
                    durMin += reader.ReadFloat() * 60f * 60f;
                    return true;
                case "msMax":
                    durMax += reader.ReadFloat() / 1000.0f;
                    return true;
                case "secMax":
                    durMax += reader.ReadFloat();
                    return true;
                case "minMax":
                    durMax += reader.ReadFloat() * 60f;
                    return true;
                case "hourMax":
                    durMax += reader.ReadFloat() * 60f * 60f;
                    return true;
                case "loop":
                    loop = true;
                    return true;
            }
            return base.ReadParameterValue(name, reader);
        }

        protected override void Init(Entity entity, out IfElapsedValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new IfElapsedValue();
            obj.elapsedTarget = time.totalTime + durMin + (durMax - durMin) * Rand.FloatValue();
        }

        protected override bool Condition(Entity entity, ref IfElapsedValue obj, ref StateContext context, ref WorldTime time)
        {
            return time.totalTime >= obj.elapsedTarget;
        }
    }
}
