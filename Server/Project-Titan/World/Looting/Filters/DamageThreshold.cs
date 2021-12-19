using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Logging;
using World.Logic.Reader;
using World.Map.Objects.Entities;

/*
namespace World.Logic.Actions.Death.Looting.Filters
{
    public class DamageThreshold : DeathAction, ILootable
    {
        private int damageMin;

        private int damageMax = int.MaxValue;

        private float percentMin;

        private float percentMax = float.MaxValue;

        private Loot[] loots;

        public override bool ReadParameterValue(string name, LogicScriptReader reader)
        {
            switch (name)
            {
                case "damageMin":
                case "damage":
                    damageMin = reader.ReadInt();
                    return true;
                case "damageMax":
                    damageMax = reader.ReadInt();
                    return true;
                case "percentMin":
                case "percent":
                    percentMin = reader.ReadFloat();
                    return true;
                case "percentMax":
                    percentMax = reader.ReadFloat();
                    return true;
                case "items":
                    loots = reader.ReadActions<Loot>().ToArray();
                    return true;
            }
            return false;
        }

        public override void OnDeath(Enemy enemy, Player killer, List<Damager> damagers)
        {

        }

        public void AddItems(List<Item> items, PlayerLootVariables variables)
        {
            if (variables.damage < damageMin || variables.damage >= damageMax) return;
            if (variables.damagePercent < percentMin || variables.damagePercent >= percentMax) return;
            foreach (var loot in loots)
                if (loot is ILootable lootable)
                    lootable.AddItems(items, variables);
        }
    }
}
*/