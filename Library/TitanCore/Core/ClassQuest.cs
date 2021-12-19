using System;
using System.Collections.Generic;
using System.Text;

namespace TitanCore.Core
{
    public struct ClassQuest
    {
        public ushort classId;

        public byte data;

        public ClassQuest(uint binary)
        {
            classId = (ushort)(binary >> 16);
            data = (byte)binary;
        }

        public ClassQuest(ushort classId, byte data)
        {
            this.classId = classId;
            this.data = data;
        }

        public bool HasCompletedQuest(int questIndex)
        {
            return ((data >> questIndex) & 1) == 1;
        }

        public void CompleteQuest(int index)
        {
            data |= (byte)(1 << index);
        }

        public int GetCompletedCount()
        {
            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                if (!HasCompletedQuest(i)) continue;
                count++;
            }
            return count;
        }

        public uint ToBinary()
        {
            uint binary = 0;
            binary |= (uint)(classId << 16);
            binary |= data;
            return binary;
        }
    }
}
