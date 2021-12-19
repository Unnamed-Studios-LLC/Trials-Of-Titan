using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Actions;
using World.Logic.Reader;
using World.Map.Objects.Entities;

namespace World.Logic.States
{
    public class EntityStateValue
    {
        /// <summary>
        /// The values associated with the states
        /// </summary>
        public object[] actionValues;

        /// <summary>
        /// The state context of this state
        /// </summary>
        public StateContext context;
    }

    public class EntityState : LogicAction<EntityStateValue>
    {
        /// <summary>
        /// Dictionary of all entity states
        /// </summary>
        public static Dictionary<ushort, EntityState> states = new Dictionary<ushort, EntityState>();

        public static void LoadLogic(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.ls", SearchOption.AllDirectories))
            {
                using (var stream = File.OpenRead(file))
                {
                    var reader = new LogicScriptReader(stream);
                    var actions = reader.ReadActions();

                    foreach (var action in actions)
                    {
                        if (!(action is EntityState entityState)) continue;
                        var info = GameData.GetObjectByName(entityState.name);
                        if (info == null)
                        {
                            Log.Error($"'{entityState.name}' does not exist | {Path.GetFileName(file)}");
                        }

                        states.Add(info.id, entityState);
                    }
                }
            }
        }

        /// <summary>
        /// The name of the entity this action runs logic on
        /// </summary>
        public string name;

        /// <summary>
        /// The default state
        /// </summary>
        public string defaultState;

        /// <summary>
        /// The actions ran by this state
        /// </summary>
        private LogicAction[] actions;

        /// <summary>
        /// A dictionary of sub states
        /// </summary>
        private Dictionary<string, State> subStates;

        /// <summary>
        /// The actions ran on death
        /// </summary>
        private DeathAction[] deathActions;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "name":
                    this.name = reader.ReadString();
                    return true;
                case "defaultState":
                    defaultState = reader.ReadString();
                    return true;
                case "actions":
                    actions = reader.ReadActions<LogicAction>().ToArray();
                    subStates = actions.WhereType<State>().ToDictionary(_ => _.name);
                    return true;
                case "death":
                    deathActions = reader.ReadActions<DeathAction>().ToArray();
                    return true;
            }
            return false;
        }


        public override void Init(Entity entity, out EntityStateValue obj, ref StateContext context, ref WorldTime time)
        {
            obj = new EntityStateValue(); // init value

            obj.context = new StateContext(); // init context
            obj.context.currentState = defaultState;
            obj.context.parentContext = context; // set parent context

            if (actions == null) return; // no child actions, ignore creating value array
            obj.actionValues = new object[actions.Length];

            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                action.Init(entity, out obj.actionValues[i], ref obj.context, ref time);
            }
        }

        public override void Tick(Entity entity, ref EntityStateValue obj, ref StateContext context, ref WorldTime time)
        {
            if (obj.actionValues == null) return; // no child values, return..
            for (int i = 0; i < obj.actionValues.Length; i++)
            {
                actions[i].Tick(entity, ref obj.actionValues[i], ref obj.context, ref time); // tick child actions
            }
        }

        public void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {
            if (deathActions == null) return; // no death actions, return..
            for (int i = 0; i < deathActions.Length; i++)
            {
                deathActions[i].OnDeath(enemy, killer, damagers);
            }
        }

        public bool InState(string[] stateNames, int currentIndex, ref object value)
        {
            var stateValue = (EntityStateValue)value;
            var context = stateValue.context;
            if (context.currentState == null || !context.currentState.Equals(stateNames[currentIndex], StringComparison.Ordinal))
                return false;
            currentIndex++;
            if (currentIndex >= stateNames.Length)
                return true;

            if (!TryGetSubState(stateNames[currentIndex - 1], ref stateValue, out var subState, out var subValue))
                return false;
            return subState.InState(stateNames, currentIndex, ref subValue);
        }

        private bool TryGetSubState(string name, ref EntityStateValue stateValue, out State subState, out object value)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (!(action is State state)) continue;
                if (state.name != name) continue;
                subState = state;
                value = stateValue.actionValues[i];
                return true;
            }
            subState = null;
            value = null;
            return false;
        }
    }
}
