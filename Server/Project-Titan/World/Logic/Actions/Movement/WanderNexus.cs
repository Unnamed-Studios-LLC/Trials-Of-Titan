using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class WanderNexusValue
    {
        public Vec2 vector;
        public float nextTime;
    }

    public class WanderNexus : LogicAction<WanderNexusValue>
    {
        /// <summary>
        /// The speed of the wander in tiles per second
        /// </summary>
        private float speed = 1;

        private float period = 0.2f;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "period":
                    period = reader.ReadFloat();
                    return true;
            }
            return false;
        }

        public override void Init(Entity entity, out WanderNexusValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new WanderNexusValue();
            AssignVector(obj, time);
        }

        public override void Tick(Entity entity, ref WanderNexusValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is NotPlayable notPlayable)) return;
            if (time.totalTime > obj.nextTime)
                AssignVector(obj, time);

            notPlayable.MoveBy(obj.vector * (float)time.deltaTime, 0.2f);
        }

        private void AssignVector(WanderNexusValue obj, WorldTime time)
        {
            obj.vector = Vec2.FromAngle(Rand.FloatValue() * AngleUtils.PI_2) * speed;
            obj.nextTime = (float)time.totalTime + period;
        }
    }
}
