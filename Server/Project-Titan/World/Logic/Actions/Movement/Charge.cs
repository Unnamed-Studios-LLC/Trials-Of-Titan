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
    public class ChargeValue
    {
        public bool charging;

        public Vec2 target;

        public Vec2 targetVector;

        public object cooldownValue;
    }

    public class Charge : LogicAction<ChargeValue>
    {
        /// <summary>
        /// The speed of the charge
        /// </summary>
        public float speed;

        /// <summary>
        /// The radius to search for players
        /// </summary>
        public float searchRadius = 10;

        /// <summary>
        /// Player targeting system
        /// </summary>
        public TargetingSystem targetingSystem = new TargetingSystem();

        /// <summary>
        /// Cooldown for the charge
        /// </summary>
        public Cooldown cooldown = new Cooldown();

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            if (targetingSystem.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out ChargeValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ChargeValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ChargeValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is NotPlayable notPlayable)) return;

            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var player = targetingSystem.GetPlayer(entity, searchRadius);
                if (player != null)
                {
                    obj.charging = true;
                    obj.target = targetingSystem.GetTargetPosition(entity, player);
                    obj.targetVector = obj.target - entity.position.Value;
                }
            }
            
            if (obj.charging)
            {
                var moveVector = obj.targetVector * (float)time.deltaTime * (1 / speed);

                var distance = entity.position.Value.DistanceTo(obj.target);
                if (distance < moveVector.Length)
                {
                    entity.position.Value = obj.target;
                    obj.charging = false;
                }
                else
                {
                    notPlayable.MoveBy(moveVector);
                }
            }
        }
    }
}
