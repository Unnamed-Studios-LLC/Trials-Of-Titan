using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Net;
using World.Worlds;
using World.Worlds.Gates;

namespace World
{
    public class WorldManager
    {
        /// <summary>
        /// The target amount of ticks needed per second
        /// </summary>
        public const int Ticks_Per_Second = 20;

        /// <summary>
        /// The target amount of clock ticks per each game tick
        /// </summary>
        public const long Clock_Ticks_Per_Game_Tick = TimeSpan.TicksPerSecond / Ticks_Per_Second;

        /// <summary>
        /// The target time to delay after each tick in seconds (assuming the tick took 0 sec to execute)
        /// </summary>
        public const double Target_Tick_Delay = 1.0 / Ticks_Per_Second;

        /// <summary>
        /// The target tick delay but in milliseconds
        /// </summary>
        public const int Target_Tick_Delay_Ms = 1000 / Ticks_Per_Second;



        /// <summary>
        /// Dictionary of all worlds, keyed by their world id
        /// </summary>
        private ConcurrentDictionary<uint, World> worlds = new ConcurrentDictionary<uint, World>();

        /// <summary>
        /// The time used in all worlds
        /// </summary>
        private WorldTime time;

        /// <summary>
        /// Stopwatch used to measure time
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Event used to delay ticks
        /// </summary>
        private ManualResetEvent waitEvent = new ManualResetEvent(false);

        /// <summary>
        /// True if the manager is currently running
        /// </summary>
        private bool running = false;

        /// <summary>
        /// Thread used to run ticks
        /// </summary>
        private Thread tickThread;

        private uint nextWorldId = 100;

        private object worldIdLock = new object();

        public WorldModule module;

        private ConcurrentQueue<Action<IEnumerable<World>>> worldActions = new ConcurrentQueue<Action<IEnumerable<World>>>();

        public WorldManager()
        {
            stopwatch = new Stopwatch();
            time = new WorldTime(0, 0, 0);
        }

        public void Start(IEnumerable<World> startWorlds)
        {
            Log.Write("Running World Manager");

            running = true; // set running flag to true

            // start threads

            tickThread = new Thread(TickWorlds);
            tickThread.Start();

            uint startIds = 1;
            foreach (var world in startWorlds)
            {
                world.worldId = startIds++;
                AddWorld(world);
            }
        }

        public async Task Stop()
        {
            Log.Write("Stopping World Manager");

            running = false; // set running flag to false
            waitEvent.Set(); // signal threads to stop
            tickThread.Join(); // join tick thread

            Log.Write("Logging out players...");

            foreach (var world in worlds.Values.ToArray())
            {
                foreach (var player in world.objects.players.Values.ToArray())
                {
                    var client = player.client;
                    if (client == null) continue;
                    await client.Logout();
                }
            }

            Log.Write("Players logged out");
        }

        /// <summary>
        /// Run the world tick loop. Returns when running flag is false
        /// </summary>
        private void TickWorlds()
        {
            long targetTicks = 0; // the target tick count to delay towards
            long currentTicks = 0; // the current tick count
            do
            {
                if (!stopwatch.IsRunning)
                {
                    // only run on first loop, start watch and init time
                    stopwatch.Start();
                    time.deltaTime = Target_Tick_Delay;
                }
                else
                {
                    currentTicks = stopwatch.ElapsedTicks; // update current ticks

                    var elapsed = stopwatch.Elapsed.TotalSeconds;//(double)currentTicks / TimeSpan.TicksPerSecond; // calculate elapsed time
                    time.deltaTime = elapsed - time.totalTime; // set the elapsed delta time since last tick
                    time.totalTime = elapsed; // set the elapsed time
                }
                time.tickId++; // increment tick id

                // gather and run world tick actions
                var worldsArray = worlds.Values.ToArray();
                while (worldActions.TryDequeue(out var worldAction))
                    worldAction.Invoke(worldsArray);

                var actions = new Action[worldsArray.Length];
                for (int i = 0; i < worldsArray.Length; i++)
                {
                    var world = worldsArray[i];
                    actions[i] = world.Tick; // set world action
                    world.time = new WorldTime(time.tickId, time.totalTime - world.startTime, time.deltaTime); // update the worlds local time
                }

                Parallel.Invoke(actions); // invoke world ticks on parallel threads

                targetTicks += Clock_Ticks_Per_Game_Tick; // increment target ticks by the target clock ticks
                int delayTime = (int)((targetTicks - stopwatch.ElapsedTicks) / TimeSpan.TicksPerMillisecond); // calculate time to delay in milliseconds
                if (delayTime > 0) // only run delay if needed
                    waitEvent.WaitOne(delayTime); // run delay
            }
            while (running); // loop ends when the running flag is FALSE
        }

        public void DispatchWorldAction(Action<IEnumerable<World>> action)
        {
            worldActions.Enqueue(action);
        }

        public uint GetWorldId()
        {
            lock (worldIdLock)
            {
                return nextWorldId++;
            }
        }

        public void AddWorld(World world)
        {
            if (world.worldId == 0)
            {
                world.worldId = GetWorldId();
            }

            world.manager = this;
            world.InitWorld();
            worlds.TryAdd(world.worldId, world);
        }

        public void RemoveWorld(World world)
        {
            worlds.TryRemove(world.worldId, out var worldId);
        }

        /// <summary>
        /// Trys to return a world with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public bool TryGetWorld(uint id, out World world)
        {
            return worlds.TryGetValue(id, out world);
        }

        public bool TryGetReturnWorld(out World returnWorld)
        {
            var worlds = this.worlds.Values.ToArray();
            foreach (var world in worlds)
                if (world is Overworld)
                {
                    returnWorld = world;
                    return true;
                }

            foreach (var world in worlds)
                if (world is Nexus)
                {
                    returnWorld = world;
                    return true;
                }

            returnWorld = null;
            return false;
        }

        public int GetPlayerCount()
        {
            int count = 0;
            foreach (var world in worlds.Values.ToArray())
                count += world.playerCount;
            return count;
        }
    }
}
