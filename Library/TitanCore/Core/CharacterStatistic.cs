using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Core
{
    public enum CharacterStatisticType
    {
        SoulsEarned = 1
    }

    public struct CharacterStatistic
    {
        public CharacterStatisticType type;

        public ulong value;

        public CharacterStatistic(CharacterStatisticType type, ulong value)
        {
            this.type = type;
            this.value = value;
        }

        public CharacterStatistic(ulong binary)
        {
            type = (CharacterStatisticType)(binary & 0xff);
            value = (binary >> 16);// ((binary << 16) >> 16); // clear the type value
        }

        public ulong ToBinary()
        {
            ulong binary = (ulong)type;
            binary |= (value << 16);
            return binary;
        }
    }
}
