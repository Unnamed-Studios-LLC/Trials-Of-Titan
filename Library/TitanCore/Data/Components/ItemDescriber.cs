using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanCore.Core;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Entities;
using TitanCore.Data.Items;
using TitanCore.Net;
using Utils.NET.Utils;

namespace TitanCore.Data.Components
{
    public static class ItemDescriber
    {
        public static void Describe(IDescriber describer, CharacterInfo myClass, bool owned, Item item, int attack)
        {
            describer.Clear();

            var info = item.GetInfo();
            var equip = info as EquipmentInfo;
            var weapon = info as WeaponInfo;

            bool allowsSprites = describer.GetAllowsSprites();
            bool tagTitleAdded = false;

            var neutralColor = describer.GetNeutralColor();
            var enchantColor = describer.GetEnchantColor(item.enchantLevel);

            describer.AddContent(info.description);

            describer.NewLine();

            describer.AddLineSeparator();

            if (item.enchantType != ItemEnchantType.None)
            {
                describer.AddTitle($"{GetEnchantName(item.enchantType)} {StringUtils.ToRoman(item.enchantLevel)}", describer.GetEnchantColor(item.enchantLevel));

                describer.AddElement(GetEnchantText(item.enchantType, item.enchantLevel), describer.GetEnchantColor(item.enchantLevel));
                describer.NewLine();
            }

            if (equip != null)
            {
                if (equip.statIncreases.Count > 0)
                {
                    describer.AddTitle("On Equip");
                    foreach (var increase in equip.statIncreases)
                    {
                        if (increase.Value == 0) continue;
                        describer.NewLine();
                        describer.AddElement($"{(increase.Value > 0 ? "+" : "")}{increase.Value} {GetStatName(increase.Key)}", neutralColor);
                    }
                    foreach (var increase in equip.alternateStatIncreases)
                    {
                        if (increase.Value == 0) continue;
                        describer.NewLine();
                        switch (increase.Key)
                        {
                            case AlternateStatType.RateOfFire:
                                describer.AddElement($"{(increase.Value > 0 ? "+" : "")}{increase.Value}% Rate of Fire", neutralColor);
                                break;
                        }
                    }
                    describer.NewLine();
                }

                if (weapon != null)
                {
                    describer.AddTitle("Rate of fire");

                    describer.AddElement(weapon.rateOfFire.ToString(), neutralColor);

                    describer.NewLine();

                    int projCount = 1;
                    for (int i = 0; i < weapon.projectiles.Length; i += weapon.projectiles[i].amount)
                    {
                        if (weapon.projectiles.Length > weapon.projectiles[0].amount)
                        {
                            if (!describer.GetAllowsEmptyTitle())
                            {
                                describer.AddTitle("Cycle");
                                describer.AddContent("Projectile " + projCount);
                            }
                            else
                            {
                                describer.AddTitle("Projectile " + projCount, neutralColor);
                            }
                            describer.NewLine();
                            projCount++;
                        }

                        var shot = weapon.projectiles[i];

                        describer.AddTitle("Shots");

                        describer.AddElement(shot.amount.ToString(), neutralColor);

                        describer.NewLine();

                        describer.AddTitle("Damage");

                        var baseMinDamage = (int)(WeaponFunctions.GetBaseDamage(equip.slotType) * shot.minDamageMod);
                        var baseMaxDamage = (int)(WeaponFunctions.GetBaseDamage(equip.slotType) * shot.maxDamageMod);

                        if (baseMinDamage == baseMaxDamage)
                            describer.AddElement(baseMinDamage.ToString(), neutralColor);
                        else
                            describer.AddElement($"{baseMinDamage} - {baseMaxDamage}", neutralColor);

                        if (owned)
                        {
                            var attackMod = StatFunctions.AttackModifier(attack, false);
                            var minDamage = (int)(baseMinDamage * attackMod);
                            var maxDamage = (int)(baseMaxDamage * attackMod);

                            if (item.enchantType == ItemEnchantType.Damaging)
                            {
                                var enchantMod = EnchantFunctions.Damage(item.enchantLevel);
                                minDamage = (int)(minDamage * enchantMod);
                                maxDamage = (int)(maxDamage * enchantMod);
                            }

                            if (minDamage == maxDamage)
                                describer.AddElement($"({minDamage})", item.enchantType == ItemEnchantType.Damaging ? enchantColor : neutralColor);
                            else
                                describer.AddElement($"({minDamage} - {maxDamage})", item.enchantType == ItemEnchantType.Damaging ? enchantColor : neutralColor);
                        }

                        describer.NewLine();

                        if (shot is AoeProjectileData aoeData)
                        {
                            describer.AddTitle("Range");

                            describer.AddElement(aoeData.range.ToString(), neutralColor);

                            describer.NewLine();

                            describer.AddTitle("Radius");

                            describer.AddElement(aoeData.radius.ToString(), neutralColor);

                            describer.NewLine();
                        }
                        else
                        {
                            describer.AddTitle("Range");

                            describer.AddElement(((int)Math.Round(shot.lifetime * shot.speed * 100) / 100f).ToString(), neutralColor);

                            describer.NewLine();
                        }

                        if (shot.ignoreEntity)
                        {
                            if (!describer.GetAllowsEmptyTitle())
                            {
                                describer.AddTitle("Passthrough");
                            }

                            describer.AddContent("Pierces enemies", neutralColor);

                            describer.NewLine();
                        }

                        if (shot.fallthrough)
                        {
                            if (!describer.GetAllowsEmptyTitle())
                            {
                                describer.AddTitle("Fallthrough");
                            }

                            describer.AddContent("Pierces enemies on kill", neutralColor);

                            describer.NewLine();
                        }

                        //describer.NewLine();
                    }
                }
            }

            if (info.heals > 0)
            {
                if (!describer.GetAllowsEmptyTitle())
                {
                    describer.AddTitle("Healing");
                    describer.NewLine();
                }

                describer.AddContent($"Restores {info.heals} health", neutralColor);
                describer.NewLine();
            }

            if (equip != null && equip.soulless)
            {
                if (!describer.GetAllowsEmptyTitle())
                {
                    describer.AddTitle("Soulless");
                    describer.NewLine();
                }

                if (allowsSprites)
                    describer.AddContent($"This item costs <color=#4be3de>{NetConstants.Soulless_Cost_Equip} essence</color> to equip and drains <color=#4be3de>{NetConstants.Soulless_Cost_Drain} essence</color> every 5 seconds.");
                else
                    describer.AddContent($"This item costs {NetConstants.Soulless_Cost_Equip} essence to equip and drains {NetConstants.Soulless_Cost_Drain} essence every 5 seconds.");

                describer.NewLine();
            }

            describer.AddLineSeparator();

            var scrollInfo = info as ScrollInfo;
            if (scrollInfo != null)
            {
                describer.AddContent($"Increases {StringUtils.Labelize(scrollInfo.statType.ToString())} by {(scrollInfo.statType == StatType.MaxHealth ? 10 : 1)}");
                describer.NewLine();
            }

            if (info.consumable && owned)
            {
                AddTagTitle(describer, ref tagTitleAdded);

                if (allowsSprites)
                    describer.AddContent($"Consumable (double-{describer.GetClickSprite()} to use)");
                else
                    describer.AddContent("Consumable (double-click to use)");
                describer.NewLine();
            }

            if (item.soulbound)
            {
                AddTagTitle(describer, ref tagTitleAdded);

                describer.AddContent("Soulbound");
                describer.NewLine();
            }

            if (weapon != null)
            {
                if (weapon.esforged)
                {
                    AddTagTitle(describer, ref tagTitleAdded);

                    describer.AddContent("Esforged");
                    describer.NewLine();
                }
            }

            if (equip != null && equip.slotType != SlotType.Accessory && myClass != null)
            {
                if (!myClass.CanUseSloType(equip.slotType))
                {
                    describer.AddContent($"<color=#EF383B>Not usable by {myClass.name}</color>");
                    describer.NewLine();
                }

                describer.AddTitle("Usable by");
                bool first = true;
                foreach (var classInfo in GameData.objects.Values.Where(_ => _ is TitanCore.Data.Entities.CharacterInfo).Select(_ => (TitanCore.Data.Entities.CharacterInfo)_))
                {
                    if (classInfo.CanUseSloType(equip.slotType))
                    {
                        if (!first)
                            describer.AddContent(", " + classInfo.name);
                        else
                            describer.AddElement(classInfo.name);
                        first = false;
                    }
                }
            }
        }

        private static void AddTagTitle(IDescriber describer, ref bool tagTitleAdded)
        {
            if (tagTitleAdded || describer.GetAllowsEmptyTitle()) return;
            tagTitleAdded = true;
            describer.AddTitle("Tags");
        }

        public static string GetStatName(StatType type)
        {
            switch (type)
            {
                case StatType.MaxHealth:
                    return "Max Health";
                default:
                    return type.ToString();
            }
        }

        public static string GetEnchantText(ItemEnchantType type, int level)
        {
            switch (type)
            {
                case ItemEnchantType.Damaging:
                    return $"Damage increased by {(int)Math.Round(EnchantFunctions.Damage(level) * 100 - 100)}%";
                default:
                    return "";
            }
        }

        public static string GetEnchantName(ItemEnchantType type)
        {
            return type.ToString();
        }
    }
}
