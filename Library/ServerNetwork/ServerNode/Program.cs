using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Modules;

namespace ServerNode
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ModularProgram.Run(new NodeModule());
        }
    }
}
