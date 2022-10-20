using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Modules
{
    public abstract class Module
    {
        /// <summary>
        /// The name of the module, used for command dispatching
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Starts the module
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stops the module
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Called when a command is received
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public abstract void OnCommand(string command, string[] args);
    }
}
