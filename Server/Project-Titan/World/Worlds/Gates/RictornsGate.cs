using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using TitanCore.Files;
using TitanCore.Gen;
using Utils.NET.Geometry;
using Utils.NET.Utils;
using World.Map.Spawning;

namespace World.Worlds.Gates
{
    public class RictornsGate : Gate
    {
        public override ushort PreferredPortal => 0xa96;

        public override string WorldName => "Wispering Woods";

        public override bool LimitSight => true;

        protected override string MapFile => "rictornsgate-3.mef";

        protected override string DefaultMusic => "Dark_Recluse";

        protected override int PortalTime => 60;

        protected override int TargetPlayers => 1;

        public override int MaxPlayerCount => 10;

        protected override MapElementFile LoadMap()
        {
            var map = base.LoadMap();
            ApplyEnvironment(map);
            return map;
        }

        private void ApplyEnvironment(MapElementFile file)
        {
            var spawn = Vec2.zero;
            var boss = Vec2.zero;
            foreach (var region in file.regions)
                switch (region.regionType)
                {
                    case Region.Spawn:
                        spawn = new Vec2(region.x, region.y);
                        break;
                    case Region.Tag1:
                        boss = new Vec2(region.x, region.y);
                        break;
                }

            for (int y = 0; y < file.height; y++)
                for (int x = 0; x < file.height; x++)
                {
                    var tile = file.tiles[x, y];
                    if (tile.objectType == 0xa94 && new Vec2(x, y).DistanceTo(spawn) > 15) // crooked tree
                    {
                        var rnd = Rand.Next(1000);

                        if (RndChance(ref rnd, 50))
                        {
                            tile.objectType = 0; // remove tree
                        }
                        else if (RndChance(ref rnd, 16))
                        {
                            tile.objectType = 0x1069; // acolyte tower
                        }
                        else if (RndChance(ref rnd, 10))
                        {
                            tile.objectType = 0x106a; // mage tower
                        }
                        file.tiles[x, y] = tile;
                        continue;
                    }
                    if (tile.objectType > 0 || tile.tileType == 0) continue;
                    tile.objectType = ObjectForTile(ref tile.tileType, new Vec2(x, y), spawn, boss);
                    file.tiles[x, y] = tile;
                }
        }

        private ushort ObjectForTile(ref ushort tile, Vec2 position, Vec2 spawn, Vec2 boss)
        {
            var rnd = Rand.Next(1000);
            if (tile == 0xb40)
            {
                if (RndChance(ref rnd, 30))
                    return 0xa95; // weeds
                if (RndChance(ref rnd, 16))
                    return 0xa97; // flower
                if (RndChance(ref rnd, 16))
                    return 0xa98; // mushroom
                if (position.DistanceTo(spawn) > 4 && position.DistanceTo(boss) > 18 && RndChance(ref rnd, 5))
                {
                    tile = 0;
                    return 0xa94; // crooked tree
                }

                if (position.DistanceTo(spawn) > 10)
                {
                    if (RndChance(ref rnd, 4))
                        return 0x106b; // acolyte of rictorn
                    if (RndChance(ref rnd, 3))
                        return 0x106c; // mage of rictorn
                    if (RndChance(ref rnd, 6))
                        return 0x1068; // mysterious butterfly
                }
            }
            return 0;
        }

        private bool RndChance(ref int rnd, int chance)
        {
            rnd -= chance;
            return rnd < 0;
        }

        protected override QuestTaskSystem CreateTasks()
        {
            return new QuestTaskSystem(this, new BossTask(0x106d, GetRegions(Region.Tag1)[0].ToVec2() + 0.5f));
        }
    }
}
