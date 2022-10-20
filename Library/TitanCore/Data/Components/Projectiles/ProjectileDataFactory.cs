using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.IO.Xml;
using Utils.NET.Utils;

namespace TitanCore.Data.Components.Projectiles
{
    public class ProjectileDataFactory
    {
        private static TypeFactory<ProjectileType, ProjectileData> factory = new TypeFactory<ProjectileType, ProjectileData>(_ => _.Type);

        public static IEnumerable<ProjectileData> ParseProjectiles(XmlParser xml)
        {
            var list = xml.Elements("Projectile").Select(Create).ToList();
            int skipCount = 0;
            int cycleIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (skipCount <= 0)
                {
                    skipCount = p.amount;
                    cycleIndex = i;
                }
                p.cycleStart = cycleIndex;

                if (!p.definedAmount)
                {
                    for (int j = 1; j < p.amount; j++)
                    {
                        skipCount--;
                        list.Insert(i + j, p);
                    }
                    i += p.amount - 1;
                }
                else
                {
                    skipCount--;
                }
            }
            return list;
        }

        private static ProjectileData Create(XmlParser xml)
        {
            var type = xml.AtrEnum("type", ProjectileType.Linear);
            var info = factory.Create(type) ?? new LinearProjectileData();
            info.Parse(xml);
            return info;
        }
    }
}
