using MySqlConnector;
using System;
using System.Threading;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Logging;
using Utils.NET.Modules;

namespace NameReservationLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            ModularProgram.Run(new NameResModule());
        }

        
    }
}
