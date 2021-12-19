using System;
namespace World.Logic.States
{
    public class StateContext
    {
        /// <summary>
        /// The current state
        /// </summary>
        public string currentState = null;

        /// <summary>
        /// The parent context (null if none)
        /// </summary>
        public StateContext parentContext = null;
    }
}
