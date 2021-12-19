using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;

namespace World
{
    public static class SoulGroupDefinitions
    {
        private static Dictionary<SoulGroup, int> soulValues = new Dictionary<SoulGroup, int>()
        {
            { SoulGroup.OceanBeach, 2000 },
            { SoulGroup.Grasslands, 6000 },
            { SoulGroup.DarkForest, 9000 },
            { SoulGroup.RictornsGate, 12000 },
            { SoulGroup.Desert, 14000 },
            { SoulGroup.Gorge, 20000 },
            { SoulGroup.Lake, 24000 },
            { SoulGroup.Tundra, 37000 },
            { SoulGroup.Mountains, 42000 },
            { SoulGroup.ValdoksForge, 40000 },
            { SoulGroup.Dumir, 40000 },
            { SoulGroup.MannahsFortress, 45000 },
        };

        private static Dictionary<SoulGroup, int> maxHealthValues = new Dictionary<SoulGroup, int>()
        {
            { SoulGroup.OceanBeach, 100 },
            { SoulGroup.Grasslands, 260 },
            { SoulGroup.DarkForest, 380 },
            { SoulGroup.RictornsGate, 400 },
            { SoulGroup.Desert, 700 },
            { SoulGroup.Gorge, 800 },
            { SoulGroup.Lake, 1200 },
            { SoulGroup.Tundra, 2000 },
            { SoulGroup.Mountains, 1800 },
            { SoulGroup.ValdoksForge, 1100 },
            { SoulGroup.Dumir, 1200 },
            { SoulGroup.MannahsFortress, 1600 },
        };

        private static Dictionary<SoulGroup, int> damageValues = new Dictionary<SoulGroup, int>()
        {
            { SoulGroup.OceanBeach, 12 },
            { SoulGroup.Grasslands, 20 },
            { SoulGroup.DarkForest, 35 },
            { SoulGroup.RictornsGate, 28 },
            { SoulGroup.Desert, 48 },
            { SoulGroup.Gorge, 50 },
            { SoulGroup.Lake, 65 },
            { SoulGroup.Tundra, 80 },
            { SoulGroup.Mountains, 90 },
            { SoulGroup.ValdoksForge, 70 },
            { SoulGroup.Dumir, 90 },
            { SoulGroup.MannahsFortress, 110 },
        };

        private static Dictionary<SoulGroup, int> levelValues = new Dictionary<SoulGroup, int>()
        {
            { SoulGroup.OceanBeach, 10 },
            { SoulGroup.Grasslands, 30 },
            { SoulGroup.DarkForest, 50 },
            { SoulGroup.RictornsGate, 70 },
            { SoulGroup.Desert, 90 },
            { SoulGroup.Gorge, 90 },
            { SoulGroup.Lake, 110 },
            { SoulGroup.Tundra, 130 },
            { SoulGroup.Dumir, 130 },
            { SoulGroup.Mountains, 160 },
            { SoulGroup.ValdoksForge, 160 },
            { SoulGroup.MannahsFortress, 180 },
        };

        public static int GetSoulValue(SoulGroup group)
        {
            if (!soulValues.TryGetValue(group, out var value))
                return GetSoulValue(SoulGroup.OceanBeach);
            return value;
        }

        public static int GetMaxHealthValue(SoulGroup group)
        {
            if (!maxHealthValues.TryGetValue(group, out var value))
                return GetMaxHealthValue(SoulGroup.OceanBeach);
            return value;
        }

        public static int GetDamageValue(SoulGroup group)
        {
            if (!damageValues.TryGetValue(group, out var value))
                return GetDamageValue(SoulGroup.OceanBeach);
            return value;
        }

        public static int GetLevelValue(SoulGroup group)
        {
            if (!levelValues.TryGetValue(group, out var value))
                return GetLevelValue(SoulGroup.OceanBeach);
            return value;
        }
    }
}
