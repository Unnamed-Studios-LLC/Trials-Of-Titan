using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Core;
using Utils.NET.Collections;

namespace World.Map.Spawning
{
    public class TileSpawnDefinition
    {
        public ushort[] tileTypes;

        public int landingSize;

        public int enemiesPerLanding = 1;

        public ushort[] spawnables;

        public ushort[] encounters;

        public float respawnRate;

        public float encounterSpawnRate;

        public float encounterRadius = 40;

        public int maxConcurrentEncounters;

        public SoulGroup soulGroup;

        public static Dictionary<ushort, TileSpawnDefinition> definitions = new Dictionary<ushort, TileSpawnDefinition>();

        private static Dictionary<SoulGroup, TileSpawnDefinition> soulDefinitions = new Dictionary<SoulGroup, TileSpawnDefinition>();

        static TileSpawnDefinition()
        {
            var array = new TileSpawnDefinition[]
            {
                new TileSpawnDefinition() // Ocean
                {
                    tileTypes = new ushort[]
                    {
                        0xb03 // beach sand
                    },
                    landingSize = 14,
                    spawnables = new ushort[]
                    {
                        0x1019, // Pirate Ship
                        0x101e, // Starfish
                    },
                    encounters = new ushort[]
                    {

                    },
                    respawnRate = 50,
                    encounterSpawnRate = 30,
                    encounterRadius = 60,
                    maxConcurrentEncounters = 0,
                    soulGroup = SoulGroup.OceanBeach
                },
                new TileSpawnDefinition() // Beach
                {
                    tileTypes = new ushort[]
                    {
                        0xb04 // beach sand
                    },
                    landingSize = 13,
                    spawnables = new ushort[]
                    {
                        0x1016, // Bandit Leader
                        0x101b, // Beach Snake
                        0x101f, // Beach Crab
                        0x1020, // Beach Snail
                    },
                    encounters = new ushort[]
                    {

                    },
                    respawnRate = 60,
                    encounterSpawnRate = 30,
                    encounterRadius = 60,
                    maxConcurrentEncounters = 0,
                    soulGroup = SoulGroup.OceanBeach
                },
                new TileSpawnDefinition() // Grasslands
                {
                    tileTypes = new ushort[]
                    {
                        0xb02 // simple grass
                    },
                    landingSize = 15,
                    spawnables = new ushort[]
                    {
                        0x1003, // goblin brute
                        0x1004, // flower bug
                        0x1008, // rogue summoner
                    },
                    encounters = new ushort[]
                    {
                        0x1009, // demon brute
                        0x100c, // forest wanderer
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 30,
                    encounterRadius = 60,
                    maxConcurrentEncounters = 5,
                    soulGroup = SoulGroup.Grasslands
                },
                new TileSpawnDefinition() // Dark Forest
                {
                    tileTypes = new ushort[]
                    {
                        0xb05, // marsh grass
                        0xb06, // marsh dirt
                    },
                    landingSize = 15,
                    spawnables = new ushort[]
                    {
                        0x1023, // floating toxin
                        0x1025, // giant spider
                        0x1028, // pixie
                        0x102a, // forest goop
                    },
                    encounters = new ushort[]
                    {
                        0x1027, // guardian ape
                        0x102b, // wispering ent
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 30,
                    encounterRadius = 60,
                    maxConcurrentEncounters = 5,
                    soulGroup = SoulGroup.DarkForest
                },
                new TileSpawnDefinition() // Desert
                {
                    tileTypes = new ushort[]
                    {
                        0xb07, // desert sand
                        0xb29, // desert sand light
                    },
                    landingSize = 12,
                    spawnables = new ushort[]
                    {
                        0x102f, // dust devil
                        0x1034, // scorpion
                        0x1035, // skeletal group
                    },
                    encounters = new ushort[]
                    {
                        0x102d, // dune snake
                        0x1030, // bubra caravan
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 40,
                    encounterRadius = 100,
                    maxConcurrentEncounters = 4,
                    soulGroup = SoulGroup.Desert
                },
                new TileSpawnDefinition() // Shallows
                {
                    tileTypes = new ushort[]
                    {
                        0xb24, // deep lake
                        0xb25, // lake
                    },
                    landingSize = 15,
                    spawnables = new ushort[]
                    {
                        0x103e, // warrior zebt
                        0x1041, // zebt of zornan
                    },
                    encounters = new ushort[]
                    {
                        0x103b, // hydra
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 120,
                    encounterRadius = 100,
                    maxConcurrentEncounters = 2,
                    enemiesPerLanding = 2,
                    soulGroup = SoulGroup.Lake
                },
                new TileSpawnDefinition() // Tundra
                {
                    tileTypes = new ushort[]
                    {
                        0xb1e, // full snow
                        0xb1f, // partial snow
                        0xb21, // frozen lake
                        0xb22, // cracked lake
                        0xb23, // cold lake
                    },
                    landingSize = 10,
                    spawnables = new ushort[]
                    {
                        0x1043, // dumir warrior
                        0x1044, // dumir war mammoth
                        0x1045, // brittlebone
                        0x1046, // frost leech
                    },
                    encounters = new ushort[]
                    {
                        0x1042, // dumir chief
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 120,
                    encounterRadius = 180,
                    maxConcurrentEncounters = 1,
                    enemiesPerLanding = 3,
                    soulGroup = SoulGroup.Tundra
                },
                new TileSpawnDefinition() // Mountains
                {
                    tileTypes = new ushort[]
                    {
                        0xb0d, // mountain rock
                    },
                    landingSize = 10,
                    spawnables = new ushort[]
                    {
                        0x1037, // stranded knight
                        0x1038, // conjurer of doom
                        0x103a, // black knight
                    },
                    encounters = new ushort[]
                    {
                        0x1077 // grand lich
                    },
                    respawnRate = 60,
                    encounterSpawnRate = 120,
                    encounterRadius = 180,
                    maxConcurrentEncounters = 1,
                    enemiesPerLanding = 3,
                    soulGroup = SoulGroup.Mountains
                },
            };
            foreach (var d in array)
                foreach (var t in d.tileTypes)
                    definitions.Add(t, d);
            foreach (var d in array)
                if (d.soulGroup != SoulGroup.OceanBeach)
                    soulDefinitions.Add(d.soulGroup, d);
        }

        public static bool TryGet(ushort tile, out TileSpawnDefinition definition)
        {
            return definitions.TryGetValue(tile, out definition);
        }

        public static bool TryGet(SoulGroup group, out TileSpawnDefinition definition)
        {
            return soulDefinitions.TryGetValue(group, out definition);
        }
    }
}
