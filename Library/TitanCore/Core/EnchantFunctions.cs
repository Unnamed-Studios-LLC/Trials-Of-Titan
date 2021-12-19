using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Core
{
    public static class EnchantFunctions
    {
        public static float Damage(int level) => 1.0f + (level * 8) / 100.0f;
    }
}
