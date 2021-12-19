using System;
using System.Collections.Generic;
using System.Text;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;

namespace World.Logic.Actions.Conditionals
{
    public class ConditionalValue<T>
    {
        public T value;

        public object[] values;

        public bool lastCondition = true;
    }

    public abstract class Conditional<T> : LogicAction<ConditionalValue<T>>
    {
        private LogicAction[] trueActions;

        private LogicAction[] falseActions;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "true":
                    trueActions = reader.ReadActions<LogicAction>().ToArray();
                    break;
                case "false":
                    falseActions = reader.ReadActions<LogicAction>().ToArray();
                    break;
            }
            return false;
        }

        public override void Init(Entity entity, out ConditionalValue<T> obj, ref StateContext context, ref WorldTime time)
        {
            obj = new ConditionalValue<T>();
            Init(entity, out obj.value, ref context, ref time);
            var condition = Condition(entity, ref obj.value, ref context, ref time);
            if (condition)
            {
                if (trueActions != null)
                {
                    obj.values = new object[trueActions.Length];
                    for (int i = 0; i < trueActions.Length; i++)
                    {
                        var action = trueActions[i];
                        action.Init(entity, out var trueObj, ref context, ref time);
                        obj.values[i] = trueObj;
                    }
                }
            }
            else
            {
                if (falseActions != null)
                {
                    obj.values = new object[falseActions.Length];
                    for (int i = 0; i < falseActions.Length; i++)
                    {
                        var action = falseActions[i];
                        action.Init(entity, out var falseObj, ref context, ref time);
                        obj.values[i] = falseObj;
                    }
                }
            }
            obj.lastCondition = condition;
        }

        public override void Tick(Entity entity, ref ConditionalValue<T> obj, ref StateContext context, ref WorldTime time)
        {
            var condition = Condition(entity, ref obj.value, ref context, ref time);
            if (condition != obj.lastCondition)
            {
                if (condition)
                {
                    if (trueActions != null)
                    {
                        obj.values = new object[trueActions.Length];
                        for (int i = 0; i < trueActions.Length; i++)
                        {
                            var action = trueActions[i];
                            action.Init(entity, out var trueObj, ref context, ref time);
                            obj.values[i] = trueObj;
                        }
                    }
                }
                else
                {
                    if (falseActions != null)
                    {
                        obj.values = new object[falseActions.Length];
                        for (int i = 0; i < falseActions.Length; i++)
                        {
                            var action = falseActions[i];
                            action.Init(entity, out var falseObj, ref context, ref time);
                            obj.values[i] = falseObj;
                        }
                    }
                }
            }

            obj.lastCondition = condition;

            if (condition)
            {
                if (trueActions != null)
                {
                    for (int i = 0; i < trueActions.Length; i++)
                    {
                        var action = trueActions[i];
                        action.Tick(entity, ref obj.values[i], ref context, ref time);
                    }
                }
            }
            else
            {
                if (falseActions != null)
                {
                    for (int i = 0; i < falseActions.Length; i++)
                    {
                        var action = falseActions[i];
                        action.Tick(entity, ref obj.values[i], ref context, ref time);
                    }
                }
            }
        }

        protected abstract void Init(Entity entity, out T obj, ref StateContext context, ref WorldTime time);

        protected abstract bool Condition(Entity entity, ref T obj, ref StateContext context, ref WorldTime time);
    }
}
