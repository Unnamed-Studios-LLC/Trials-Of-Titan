using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Data.Items;
using Utils.NET.Geometry;
using Utils.NET.IO.Xml;

namespace TitanCore.Data.Components.Projectiles
{
    public abstract class ProjectileData
    {
        public abstract ProjectileType Type { get; }

        /// <summary>
        /// The name of the info to create this projectile from
        /// </summary>
        public string infoName;

        /// <summary>
        /// The rotation of this projectile in Rotations Per Second
        /// </summary>
        public float spin;

        /// <summary>
        /// The speed of the projectile in Tile Per Second
        /// </summary>
        public float speed;

        /// <summary>
        /// The lifetime of this projectile
        /// </summary>
        public float lifetime;

        /// <summary>
        /// The angle gap between multiple projectiles
        /// </summary>
        public float angleGap;

        /// <summary>
        /// The amount of projectiles shot
        /// </summary>
        public int amount;

        /// <summary>
        /// The size of this projectile
        /// </summary>
        public float size;

        /// <summary>
        /// The projectile to used to determine relative values
        /// </summary>
        private ProjectileRef projReference;

        /// <summary>
        /// The minimum damage of this projectile
        /// </summary>
        public float minDamageMod;

        /// <summary>
        /// The maximum damage of this projectile
        /// </summary>
        public float maxDamageMod;

        /// <summary>
        /// If this projectile triggers attack animation or not
        /// </summary>
        public bool passive;

        /// <summary>
        /// True if this projectile does not angle to it's path
        /// </summary>
        public bool noPath;

        /// <summary>
        /// If the projectile "falls through" enemies. (If a projectile will kill the enemy, the projectile will continue and not delete)
        /// </summary>
        public bool fallthrough;

        /// <summary>
        /// The index that this wave starts
        /// </summary>
        public int cycleStart = 0;

        /// <summary>
        /// Status effects to apply on hit
        /// </summary>
        public StatusEffectData[] onHitEffects;

        /// <summary>
        /// The hit checking radius
        /// </summary>
        //public float hitRadius;

        /// <summary>
        /// Does this projectile self define each amount
        /// </summary>
        public bool definedAmount = false;

        public bool ignoreCollision = false;

        public bool ignoreEntity = false;

        public virtual void Parse(XmlParser xml)
        {
            infoName = xml.AtrString("name");
            spin = xml.Float("Spin", 0);
            speed = xml.Float("Speed", 1);
            lifetime = xml.Float("Lifetime", 1);
            angleGap = AngleUtils.Deg2Rad * xml.Float("AngleGap", 0); // convert to radians
            amount = xml.Int("Amount", 1);
            size = xml.Float("Size", 1);
            passive = xml.Exists("Passive");
            noPath = xml.Exists("NoPath");
            fallthrough = xml.Exists("Fallthrough");
            //hitRadius = xml.Float("HitRadius", 1f);
            definedAmount = xml.Exists("DefinedAmount");
            ignoreCollision = xml.Exists("IgnoreCollision");
            ignoreEntity = xml.Exists("IgnoreEntity");

            minDamageMod = xml.Float("DamageMod", 0);
            minDamageMod = xml.Float("MinDamageMod", minDamageMod);
            maxDamageMod = xml.Float("MaxDamageMod", minDamageMod);

            foreach (var reference in xml.Elements("ProjectileRef"))
                projReference = new ProjectileRef(reference);

            onHitEffects = xml.Elements("StatusEffect").Select(_ => new StatusEffectData(_)).ToArray();

            var range = xml.Float("Range", 0);
            if (range > 0)
            {
                lifetime = range / speed;
            }
        }

        public abstract Vec2 GetPosition(float time, float sin, float cos, uint projId);

        public void ProcessReference()
        {
            if (projReference == null) return;
            var referenceObject = (WeaponInfo)GameData.objects[projReference.objectId];
            ProjectileData referenceProj = null;// = referenceObject.projectiles[projReference.index];
            int count = 0;
            for (int i = 0; i <= projReference.index; i++)
            {
                if (i == projReference.index)
                {
                    referenceProj = referenceObject.projectiles[count];
                    break;
                }
                var proj = referenceObject.projectiles[count];
                count += proj.amount;
            }

            minDamageMod *= referenceProj.minDamageMod;
            maxDamageMod *= referenceProj.maxDamageMod;

            projReference = null;
        }
    }
}