using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using World.Logic.Reader;

namespace World.Logic.Actions
{
    public abstract class ReadableAction
    {
        /// <summary>
        /// All logic action types keyed to their name in "logic_action" format
        /// </summary>
        public static Dictionary<string, Type> actionTypes;

        static ReadableAction()
        {
            var baseType = typeof(ReadableAction);
            var types = baseType.Assembly.GetTypes().Where(_ => baseType.IsAssignableFrom(_) && !_.IsAbstract);
            actionTypes = types.ToDictionary(_ => CreateScriptActionName(_.Name));
        }

        private static string CreateScriptActionName(string name)
        {
            var builder = new StringBuilder(name);
            for (int i = 0; i < builder.Length; i++)
            {
                var c = builder[i];
                if (char.IsUpper(c))
                {
                    builder[i] = char.ToLower(c); // transform "ReadableAction" into "readable_action"
                    if (i != 0)
                    {
                        builder.Insert(i, '_');
                        i++;
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Sets the value of a parameter
        /// </summary>
        /// <param name="name"></param>
        public abstract bool ReadParameterValue(string name, LogicScriptReader reader);
    }
}
