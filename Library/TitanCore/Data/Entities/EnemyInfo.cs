using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Components.Projectiles;
using Utils.NET.IO.Xml;
using Utils.NET.Logging;

namespace TitanCore.Data.Entities
{
    public class EnemyInfo : EntityInfo
    {
        public override GameObjectType Type => GameObjectType.Enemy;

        /// <summary>
        /// The maximum health modification of this enemy
        /// </summary>
        public float healthMod;

        /// <summary>
        /// The amount of defense this enemy has
        /// </summary>
        public int defense;

        /// <summary>
        /// The amount of souls this enemy gives in relation to it's set soul area
        /// </summary>
        public float soulMod;

        /// <summary>
        /// Array of the projectiles this weapon shoots
        /// </summary>
        public ProjectileData[] projectiles;

        /// <summary>
        /// The title of this enemy
        /// </summary>
        public string title;

        /// <summary>
        /// The title of this enemy
        /// </summary>
        public string shortName;

        /// <summary>
        /// The level of this enemy
        /// </summary>
        //public int level;

        /// <summary>
        /// The soul level group this enemy belongs to
        /// </summary>
        public SoulGroup soulGroup;

        /// <summary>
        /// The loot tier that this enemy falls into within the soul group
        /// </summary>
        public int[] lootTiers;

        public bool titan;

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            healthMod = xml.Float("HealthMod", 1);
            defense = xml.Int("Defense", 0);
            soulMod = xml.Float("SoulMod", 0);
            projectiles = ProjectileDataFactory.ParseProjectiles(xml).ToArray();
            title = xml.String("Title", name);
            shortName = xml.String("ShortName", name);
            //level = xml.Int("Level");
            soulGroup = xml.Enum("SoulGroup", SoulGroup.OceanBeach);
            lootTiers = xml.Elements("LootTier").Select(_ => _.intValue).ToArray();
            titan = xml.Exists("Titan");
        }
    }
}
