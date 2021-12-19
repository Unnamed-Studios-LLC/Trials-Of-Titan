using System;
using Utils.NET.Collections;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;

namespace World.Logic.Components
{
    public struct Cooldown
    {
        private struct CooldownValue
        {
            public float time;

            public CooldownValue(float delay)
            {
                time = delay;
            }
        }
        
        /*
        public float period
        {
            get
            {
                if (periodMin == periodMax) return periodMin;
                return periodMin + (periodMax - periodMin) * Rand.FloatValue();
            }
            set
            {
                periodMin = value;
                periodMax = value;
            }
        }
        */

        public Range period;

        public Range delay;

        public Cooldown(float period, float delay)
        {
            this.period = period;
            this.delay = delay;
        }

        public void Init(out object obj)
        {
            var delay = this.delay.GetRandom();
            obj = new CooldownValue(delay < 0 ? Rand.FloatValue() * period.GetRandom() : delay);
        }

        public bool Tick(ref object obj, ref WorldTime time)
        {
            var value = (CooldownValue)obj;
            var newTime = value.time - (float)time.deltaTime;
            if (value.time >= 0 && newTime < 0)
            {
                var p = period.GetRandom();
                if (p > 0)
                {
                    while (newTime < 0)
                    {
                        newTime += p;
                        p = period.GetRandom();
                    }
                    value.time = newTime;
                }
                else
                    value.time = newTime;
                obj = value;
                return true;
            }
            value.time = newTime;
            obj = value;
            return false;
        }

        public bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "period":
                    period = reader.ReadFloat();
                    return true;
                case "periodMin":
                    period.min = reader.ReadFloat();
                    return true;
                case "periodMax":
                    period.max = reader.ReadFloat();
                    return true;
                case "delay":
                    delay = reader.ReadFloat();
                    return true;
                case "delayMin":
                    delay.min = reader.ReadFloat();
                    return true;
                case "delayMax":
                    delay.max = reader.ReadFloat();
                    return true;
            }
            return false;
        }
    }
}
