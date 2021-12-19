using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Logic.Components;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Movement
{
    public class ChaseAngularValue
    {
        public Vec2 moveVector;

        public object cooldownValue;
    }

    public class ChaseAngular : LogicAction<ChaseAngularValue>
    {
        /// <summary>
        /// The speed to chase the player at
        /// </summary>
        public float speed;

        /// <summary>
        /// The angular range that the enemy can offset by
        /// </summary>
        public float angleRange;

        /// <summary>
        /// The distance used to search for players
        /// </summary>
        public float searchRadius = 8;

        /// <summary>
        /// The cooldown of the vector assignment
        /// </summary>
        public Cooldown cooldown = new Cooldown();


        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "speed":
                    speed = reader.ReadFloat();
                    return true;
                case "angleRange":
                    angleRange = reader.ReadAngle();
                    return true;
                case "searchRadius":
                    searchRadius = reader.ReadFloat();
                    return true;
            }
            if (cooldown.ReadParameterValue(name, reader))
                return true;
            return false;
        }

        public override void Init(Entity enemy, out ChaseAngularValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ChaseAngularValue();
            cooldown.Init(out obj.cooldownValue);
        }

        public override void Tick(Entity entity, ref ChaseAngularValue obj, ref StateContext context, ref WorldTime time)
        {
            if (!(entity is NotPlayable notPlayable)) return;
            if (cooldown.Tick(ref obj.cooldownValue, ref time))
            {
                var target = entity.GetClosestPlayer(searchRadius);
                if (target == null)
                {
                    obj.moveVector = Vec2.zero;
                }
                else
                {
                    var angleToPlayer = entity.AngleTo(target);
                    float angleOffset = Rand.FloatValue() * angleRange - angleRange / 2;
                    obj.moveVector = Vec2.FromAngle(angleToPlayer + angleOffset) * speed;
                }
            }

            notPlayable.MoveBy(obj.moveVector * (float)time.deltaTime);
        }
    }
}
