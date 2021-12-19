using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using Utils.NET.Geometry;
using World.Map.Objects.Entities;
using World.Map.Objects.Map;
using World.Map.Spawning;

namespace World.Worlds.Gates
{
    public class MannahsFortress : Gate
    {
        public override bool AllowPlayerTeleport => false;

        public override string WorldName => "Mannah's Fortress";

        public override ushort PreferredPortal => 0xa86;

        public override bool LimitSight => false;

        protected override string DefaultMusic => "Mannah's_Fortress";

        protected override string MapFile => "mannahsfortress.mef";

        public override int MaxPlayerCount => 40;

        private Enemy advisor;

        protected override void DoInitWorld()
        {
            base.DoInitWorld();

            AddLogicMethod("unlock_left_hall", UnlockLeftHall);
            AddLogicMethod("unlock_right_hall", UnlockRightHall);
            AddLogicMethod("unlock_chamber", UnlockChamber);
            AddLogicMethod("spawn_ascension_table", SpawnAscensionTable);
        }

        protected override QuestTaskSystem CreateTasks()
        {
            var advisorPosition = GetRegions(Region.Tag3)[0].ToVec2() + 0.5f;
            var mannahPosition = GetRegions(Region.Tag7)[0].ToVec2() + 0.5f;

            advisor = objects.CreateEnemy(0x1064);
            advisor.position.Value = advisorPosition;
            objects.AddObject(advisor);

            return new QuestTaskSystem(this, new BossTask(advisor), new BossTask(0x1014, mannahPosition));
        }

        private void UnlockLeftHall(Entity sender)
        {
            OpenWall(GetRegions(Region.Shop1));
        }

        private void UnlockRightHall(Entity sender)
        {
            OpenWall(GetRegions(Region.Shop2));
        }

        private void UnlockChamber(Entity sender)
        {
            OpenWall(GetRegions(Region.Shop3));
        }

        private void SpawnAscensionTable(Entity sender)
        {
            var info = GameData.objects[0xa93];
            var obj = new AscensionTable();
            obj.Initialize(info);
            obj.position.Value = GetRegions(Region.Tag7)[0].ToVec2() + 0.5f;
            objects.AddObject(obj);
        }

        private void OpenWall(IEnumerable<Int2> points)
        {
            foreach (var point in points)
            {
                var tile = tiles.GetTile(point.x, point.y);
                tile.objectType = 0;
                tile.tileType = 0xb32;
                tiles.SetTileAndBroadcast(tile);
            }
        }

        protected override void OnGateComplete()
        {
            base.OnGateComplete();

            manager.module.StartShutdown();
        }
    }
}
