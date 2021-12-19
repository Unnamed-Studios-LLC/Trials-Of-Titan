using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Items;

namespace TitanCore.Core
{
    public static class WeaponFunctions
    {
        private static Dictionary<SlotType, ushort> baseWeaponDamages = new Dictionary<SlotType, ushort>()
        {
            { SlotType.Bow, 25 },
            { SlotType.Sword, 50 },
            { SlotType.Claymore, 80 },
            { SlotType.Spear, 15 },
            { SlotType.Elixir, 25 },
            { SlotType.Crossbow, 12 },
            //{ SlotType.LancerAbility, 10 },
        };

        public static ushort GetBaseDamage(SlotType type)
        {
            if (baseWeaponDamages.TryGetValue(type, out var value))
                return value;
            return 0;
        }

        public static void GetProjectileDamage(SlotType slotType, ProjectileData data, out ushort min, out ushort max)
        {
            var baseDamage = GetBaseDamage(slotType);

            min = (ushort)(baseDamage * data.minDamageMod);
            max = (ushort)(baseDamage * data.maxDamageMod);
        }
    }
}
