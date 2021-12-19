using System;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data.Components;
using TitanCore.Data.Components.Projectiles;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Items
{
    public class WeaponInfo : EquipmentInfo
    {
        public override GameObjectType Type => GameObjectType.Weapon;

        /// <summary>
        /// The stat info of the weapon
        /// </summary>
        //public Dictionary<WeaponStatType, WeaponStatData> stats;

        /// <summary>
        /// Array of the projectiles this weapon shoots
        /// </summary>
        public ProjectileData[] projectiles;

        /// <summary>
        /// The amount of shots per second this weapon fires at
        /// </summary>
        public float rateOfFire;

        /// <summary>
        /// If this weapon was forged in the skyforge
        /// </summary>
        public bool esforged;

        public WeaponInfo() : base()
        {

        }

        public override void Parse(XmlParser xml)
        {
            base.Parse(xml);

            //stats = xml.Elements("WeaponStat").Select(_ => new WeaponStatData(new XmlParser(_))).ToDictionary(_ => _.type);
            projectiles = ProjectileDataFactory.ParseProjectiles(xml).ToArray();

            rateOfFire = xml.Float("RateOfFire", 1);
            esforged = xml.Exists("Esforged");
        }
    }
}
