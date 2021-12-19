using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Logging;
using Utils.NET.Utils;
using World.Logic.Reader;

namespace World.Looting
{
    public abstract class Loot
    {
        public static int Chance(double value) => (int)(value * 10_000);
        public static int ChanceT(int value) => value * 1_000;
        public static int ChanceTT(int value) => value * 100;
        public static int ChanceHT(int value) => value * 10;
        public static int ChanceM(int value) => value;

        private int chance;

        public Loot(int chance)
        {
            this.chance = chance;
        }

        protected bool DoChance(float percent)
        {
            var rand = Rand.Next(1_000_000);
            var chanceInvert = 1_000_000 - (int)(chance * percent);
            return rand >= chanceInvert;  // invert chance to prevent giving all loot upon a random break (always returns 0)
        }
    }
}
