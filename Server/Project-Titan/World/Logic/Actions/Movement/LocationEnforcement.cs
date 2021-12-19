using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class LocationEnforcementValue
    {
        public float enforceTimeRemaining;

        public object cooldownValue;

        public object value;
    }

    public abstract class LocationEnforcement<T> : LogicAction<LocationEnforcementValue>
    {
        /// <summary>
        /// The speed to pull the enemy back towards spawn at
        /// </summary>
        protected float speed;

        /// <summary>
        /// The minimum distance to start enforcing the behavior
        /// </summary>
        protected float distance;

        /// <summary>
        /// Time that this behavior is enforced
        /// </summary>
        private float enforceTime;

        /// <summary>
        /// Cooldown for enforcement check
        /// </summary>
        private Cooldown cooldown = new Cooldown();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "distance":
                    distance = reader.ReadFloat();
                    return true;
                case "enforce":
                    enforceTime = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out LocationEnforcementValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new LocationEnforcementValue();
            cooldown.Init(out obj.cooldownValue);
            InitValue(enemy, out var value, ref context, ref time);
            obj.value = value;
        }

        protected abstract void InitValue(Entity enemy, out T obj, ref StateContext context, ref WorldTime time);

        public override void Tick(Entity entity, ref LocationEnforcementValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is NotPlayable notPlayable)) return;

            obj.enforceTimeRemaining -= (float)time.deltaTime;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                obj.enforceTimeRemaining = enforceTime;
            }

            if (obj.enforceTimeRemaining > 0)
            {
                //var vector = entity.spawn - entity.position.Value;
                //if (vector.Length > distance)
                var value = (T)obj.value;
                if (ShouldEnforce(entity, ref value, ref context, ref time, out var vector)) 
                {
                    notPlayable.MoveBy(vector * speed * (float)time.deltaTime);
                }
            }
        }

        protected abstract bool ShouldEnforce(Entity entity, ref T obj, ref StateContext context, ref WorldTime time, out Vec2 vector);
    }
}
