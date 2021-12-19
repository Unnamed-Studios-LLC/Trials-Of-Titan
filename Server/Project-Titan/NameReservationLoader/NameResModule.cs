using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Logging;
using Utils.NET.Modules;

namespace NameReservationLoader
{
    public class NameResModule : Module
    {
        public override string Name => "nameres";

        public override void OnCommand(string command, string[] args)
        {

        }

        public override void Start()
        {
            var resetEvent = new ManualResetEvent(false);
            Run(resetEvent);

            resetEvent.WaitOne();
        }

        public override void Stop()
        {

        }

        private async void Run(ManualResetEvent resetEvent)
        {
            try
            {
                Database.Initialize().WaitOne();
            }
            catch (FailedToCreateClientException)
            {
                Log.Write("Failed to create client!");
                resetEvent.Set();
                return;
            }

            var connection = new MySqlConnection(
                $"server=127.0.0.1;" +
                $"uid=root;" +
                $"database=titan_bot;" +
                "SslMode=none;"
            );

            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM name_reservation;";

            var reader = command.ExecuteReader();
            while (reader.HasRows)
            {
                if (!reader.Read()) break;

                var name = reader.GetString("name");
                var token = reader.GetString("token");

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(name)) continue;

                //Log.Write($"{name.PadRight(14)} | {token}");

                var nameReservation = new NameReservation();
                nameReservation.playerName = name.ToLower();
                nameReservation.accountId = 0;
                nameReservation.reservationToken = token;
                nameReservation.creationDate = DateTime.UtcNow;

                var putResponse = await nameReservation.Put();
                if (putResponse.result != Model.RequestResult.Success)
                    Log.Error($"Failed to create reservation for: {name}");
                else
                    Log.Write("Created Reservation");
            }

            resetEvent.Set();
        }
    }
}
