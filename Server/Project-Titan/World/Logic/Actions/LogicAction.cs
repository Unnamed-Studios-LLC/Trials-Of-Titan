using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions
{
    public abstract class LogicAction : ReadableAction
    {
        /// <summary>
        /// Initializes the action
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        public abstract void Init(Entity entity, out object obj, ref StateContext context, ref WorldTime time);

        /// <summary>
        /// Ticks the action forward in the world
        /// </summary>
        /// <param name="time"></param>
        public abstract void Tick(Entity entity, ref object obj, ref StateContext context, ref WorldTime time);
    }

    public abstract class LogicAction<T> : LogicAction
    {
        public override void Tick(Entity entity, ref object obj, ref StateContext context, ref WorldTime time)
        {
            T o = (T)obj;
            Tick(entity, ref o, ref context, ref time);
        }

        /// <summary>
        /// Ticks the action forward in the world
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        public abstract void Tick(Entity entity, ref T obj, ref StateContext context, ref WorldTime time);

        public override void Init(Entity entity, out object obj, ref StateContext context, ref WorldTime time)
        {
            Init(entity, out T o, ref context, ref time);
            obj = o;
        }

        /// <summary>
        /// Ticks the action forward in the world
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="time"></param>
        public abstract void Init(Entity entity, out T obj, ref StateContext context, ref WorldTime time);
    }
}
