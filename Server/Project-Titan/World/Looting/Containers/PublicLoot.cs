using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Map;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Geometry;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic;
using World.Logic.Reader;
using World.Logic.States;
using World.Map.Objects.Entities;
using World.Map.Objects.Map.Containers;

namespace World.Looting
{
    public class PublicLoot : LootContainer
    {

        public PublicLoot(params ILootable[] lootables) : base(lootables)
        {

        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers, Dictionary<ulong, List<Item>> itemBags)
        {
            RunLoot(enemy, new PlayerLootVariables(0, enemy.maxHealth.Value, 1), itemBags);
        }
    }
}
