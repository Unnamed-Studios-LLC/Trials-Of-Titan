using System;
using System.Collections.Generic;
using System.Text;

namespace World.Commands
{
    public class CommandArgs
    {
        /// <summary>
        /// The command to execute
        /// </summary>
        public string command;

        /// <summary>
        /// Arguments for the command
        /// </summary>
        public string[] args;

        public CommandArgs(string command, string[] args)
        {
            this.command = command.ToLower();
            this.args = args;
        }
    }
}
