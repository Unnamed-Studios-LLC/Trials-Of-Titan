using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Data.Entities;
using Utils.NET.Geometry;

namespace TitanCore.Core
{
    public static class AbilityFunctions
    {
        public struct AbilityEffect
        {
            public StatusEffect type;

            public uint duration;

            public float area;

            public AbilityEffect(StatusEffect type, uint duration, float area)
            {
                this.type = type;
                this.duration = duration;
                this.area = area;
            }
        }

        public static int GetAbilityCooldownMs(byte rage, ushort classId)
        {
            switch ((ClassType)classId)
            {
                case ClassType.Ranger:
                    return 10000;
                case ClassType.Warrior:
                    return 20000;
                case ClassType.Commander:
                    return 10000;
                case ClassType.Lancer:
                    return 80;
                case ClassType.Alchemist:
                    return 10000;
                case ClassType.Minister:
                    return 10000;
                case ClassType.Berserker:
                    return 10000;
                case ClassType.Brewer:
                    return 4000;
                case ClassType.Bladeweaver:
                    return 1000;
                case ClassType.Nomad:
                    return 2000;
                default:
                    return int.MaxValue;
            }
        }

        public static List<AbilityEffect> GetAbilityEffects(byte rage, int attack, byte value, ClassType classType)
        {
            float rageScalar = rage / 100f;
            float attackScalar = 0.5f + attack / 50f;

            var list = new List<AbilityEffect>();
            switch (classType)
            {
                case ClassType.Ranger:
                    break;
                case ClassType.Warrior:
                    /*
                    float warriorArea = 1 + attackScalar + 4 * rageScalar * attackScalar;
                    uint warriorDuration = (uint)(1000 + 1000 * attackScalar + 4000 * rageScalar * attackScalar);
                    list.Add(new AbilityEffect(StatusEffect.Speedy, warriorDuration, warriorArea));
                    */
                    break;
                case ClassType.Commander:
                    list.Add(new AbilityEffect(StatusEffect.Fortified, (uint)(500 + 6000 * rageScalar * attackScalar), 0));
                    list.Add(new AbilityEffect(StatusEffect.Reach, (uint)(2000 + 10000 * rageScalar * attackScalar), 2 + 5 * rageScalar));
                    break;
                case ClassType.Lancer:
                    break;
                case ClassType.Minister:
                    list.Add(new AbilityEffect(StatusEffect.Healing, (uint)(500 + 6000 * rageScalar * rageScalar * attackScalar), 8 * rageScalar * rageScalar * attackScalar));
                    break;
                case ClassType.Berserker:
                    list.Add(new AbilityEffect(StatusEffect.Fervent, (uint)(500 + 6000 * rageScalar * attackScalar), 2 + 6 * rageScalar * attackScalar));
                    //list.Add(new AbilityEffect(StatusEffect.Speedy, (uint)(500 + 6000 * rageScalar * rageScalar * attackScalar), 0));
                    break;
                case ClassType.Brewer:
                    switch (value)
                    {
                        case 0:
                            list.Add(new AbilityEffect(StatusEffect.Fervent, 1000 + (uint)(10000 * rageScalar), 6));
                            break;
                        case 1:
                            list.Add(new AbilityEffect(StatusEffect.Healing, 1000 + (uint)(8000 * rageScalar), 6));
                            break;
                    }
                    break;
                default:
                    break;
            }
            return list;
        }

        public static class Alchemist
        {
            public const float Air_Time = 1;

            public static int GetGroundDurationMs(byte rage)
            {
                return 1000 + (int)((rage / 100.0f) * 10000);
            }

            public static float GetRadius(byte rage)
            {
                return 6;
            }
        }

        public static class Lancer
        {
            private const float Angle_Offset = 5f * (AngleUtils.PI / 180.0f);

            public const int Rage_Cost = 5;

            public static float GetAngleOffset(uint projId)
            {
                var offsetId = projId % 5;
                switch (offsetId)
                {
                    case 0:
                        return Angle_Offset;
                    case 1:
                        return Angle_Offset * -2;
                    case 2:
                        return 0;
                    case 3:
                        return Angle_Offset * -1;
                    case 4:
                        return Angle_Offset * 2;
                }
                return 0;
            }

            public static float GetProjectileSize(int rage)
            {
                return 1f + (rage / 100f);
            }

            public static int GetProjectileDamage(int rage, int attack)
            {
                var damage = 10 + rage;
                return (int)(damage * (0.5f + attack / 50f));
            }
        }

        public static class Minister
        {
            public static byte GetRageCost(int rage)
            {
                if (rage == 100)
                    return 100;
                else if (rage >= 75)
                    return 75;
                else if (rage >= 50)
                    return 50;
                else
                    return 25;
            }

            public static int GetHealAmount(int rage, int attack)
            {
                return rage + (int)(attack * (rage / 100f));
            }

            public static int GetPillarDurationMs(int rage)
            {
                return 8000;
                //return 1000 + (int)((rage / 100.0f) * 8000);
            }

            public static float GetPillarRadius(int rage)
            {
                return 6;// 1 + (rage / 100.0f) * 5;
            }
        }

        public static class Berserker
        {
            public static float GetShoutSpread(int rage, int attack)
            {
                return AngleUtils.PI * 0.25f;
            }

            public static float GetShoutRange(int rage, int attack)
            {
                return 8;
            }

            public static AbilityEffect GetShoutEffect(int rage, int attack)
            {
                return new AbilityEffect(StatusEffect.Slowed, 5, 0);
            }
        }

        public static class Ranger
        {
            public static float GetRadius(int rage, int attack)
            {
                return 2 + (rage / 100f) * 4;
            }

            public static ushort GetDamage(int rage, int attack)
            {
                var attackScalar = 0.5f + attack / 75f;
                var rageScalar = rage / 100f;
                return (ushort)(10 + (80 + attackScalar * 1100) * rageScalar);
            }

            public static AbilityEffect? GetEffect(int rage, int attack)
            {
                return null;
            }
        }

        public static class BladeWeaver
        {
            public static uint Dash_Duration = 150;

            private static float Max_Dash_Distance = 6;

            public static int Max_Dash_Rage = 25;

            public static Vec2 GetDashPositionVector(float angle, int rage)
            {
                float rageScalar = Math.Min(rage / (float)Max_Dash_Rage, 1);
                var speed = rageScalar * Max_Dash_Distance * (1000f / Dash_Duration);
                return Vec2.FromAngle(angle) * speed;
            }

            public static float GetProjectileSize(int rage)
            {
                return 1f + (rage / Max_Dash_Rage);
            }

            public static int GetProjectileDamage(int rage, int attack)
            {
                var damage = 10 + rage * 45;
                return (int)(damage * (0.5f + attack / 75f));
            }
        }

        public static class Warrior
        {
            public const int Heal_Area = 6;

            public static int GetHealAmount(byte rage, int attack)
            {
                return (int)(rage * 0.7f + attack * rage / 100f);
            }

            public static uint GetAbilityDuration(byte rage)
            {
                return 10000;
            }
        }

        public static class Nomad
        {
            public const int Ability_Cost = 35;

            public const float Charm_Air_Time = 0.7f;
        }
    }
}
