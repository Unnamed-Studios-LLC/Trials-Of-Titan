using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Files;
using TitanCore.Net;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Partitioning;
using Utils.NET.Utils;
using World.GameState;
using World.Map;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Map.Spawning;

namespace World.Worlds
{
    public class Overworld : World
    {

        public override bool LimitSight => false;

        public override bool AllowPlayerTeleport => true;

        protected override string MapFile => "overworld.mef";

        public override string WorldName => worldName;

        public override int MaxPlayerCount => NetConstants.Max_Overworld_Players;

        public override bool AllowGlobalObjects => true;

        protected override string DefaultMusic => "dynamic:First_Adventure";

        public override bool AutoCleanupEnemies => true;

        private Int2 spawn;

        public SpawnSystem spawnSystem;

        public OverworldCycle overworldCycle;

        private string worldName = WorldModule.ServerName.Split('.').Last();

        protected override MapElementFile LoadMap()
        {
            var map = base.LoadMap();
            return map;
        }

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            spawn = GetRandomRegion(Region.Spawn);

            var info = GameData.objects[0xa22];

            var portal = new Portal(ModularProgram.manifest.local ? null : "", 1);
            portal.worldName.Value = "Nexus";
            portal.Initialize(info);
            portal.position.Value = spawn;
            objects.AddObject(portal);

            var fireside = SetPiece.Load("fireside.mef");
            ApplySetPiece(fireside, spawn + new Int2(-7, 9));

            spawnSystem = new SpawnSystem(this);
            spawnSystem.AddNoSpawnZone(spawn, 20);

            overworldCycle = new OverworldCycle(this);
            //titanSpawnSystem = new TitanSpawnSystem(this);
        }

        public override void Tick()
        {
            spawnSystem.Tick(ref time);

            //overworldCycle.Tick(ref time);

            base.Tick();

            if ((time.tickId % (WorldManager.Ticks_Per_Second * 5)) == 0)
            {
                if (manager.module.instanceConnection != null)
                {
                    manager.module.instanceConnection.SendPlayerCount(manager.GetPlayerCount());
                }
            }
        }

        public override void PlayerDiscoveredTile(Player player, int x, int y)
        {
            base.PlayerDiscoveredTile(player, x, y);

            if ((new Vec2(x, y) - player.position.Value).LongerThan(Sight.Player_Sight_Radius - 2))
                spawnSystem.TileDiscovered(player, x, y, (float)time.totalTime);
        }

        public override void AssignQuest(Player player)
        {
            var encounterQuest = spawnSystem.GetEncounterQuest(player);
            if (encounterQuest != null)
                player.SetQuest(encounterQuest);
            else
                base.AssignQuest(player);
        }
    }
}
