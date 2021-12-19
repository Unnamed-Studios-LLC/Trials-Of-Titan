using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class FleeValue
    {
        public Player target;

        public object cooldownValue;
    }

    public class Flee : LogicAction<FleeValue>
    {
        /// <summary>
        /// The speed to flee the player at
        /// </summary>
        private float speed;

        /// <summary>
        /// The minimum distance to be from the player
        /// </summary>
        private float maxDistance;

        /// <summary>
        /// The radius area the enemy to search for players
        /// </summary>
        private float searchRadius = 8;

        /// <summary>
        /// The cooldown to search for new players
        /// </summary>
        private Cooldown searchCooldown = new Cooldown(0.5f, 0);

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "max":
                    maxDistance = reader.ReadFloat();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (searchCooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity entity, out FleeValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new FleeValue();
            searchCooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref FleeValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is NotPlayable notPlayable)) return;

            if (searchCooldown.Tick(ref obj.cooldownValue, ref time))
            {
                obj.target = notPlayable.GetClosestPlayer(searchRadius);
            }

            if (obj.target == null) return;

            var vector = notPlayable.position.Value - obj.target.position.Value;
            var length = vector.Length;
            if (length > maxDistance) return;
            notPlayable.MoveBy(vector.ChangeLength((float)time.deltaTime * speed, length));
        }
    }
}
