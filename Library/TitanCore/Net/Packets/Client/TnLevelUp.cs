using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnLevelUp : TnPacket
    {
        public override TnPacketType Type => TnPacketType.LevelUp;

        public byte maxHealthIncrease;

        public byte speedIncrease;

        public byte attackIncrease;

        public byte defenseIncrease;

        public byte vigorIncrease;

        public TnLevelUp()
        {

        }

        public TnLevelUp(byte maxHealthIncrease, byte speedIncrease, byte attackIncrease, byte defenseIncrease, byte vigorIncrease)
        {
            this.maxHealthIncrease = maxHealthIncrease;
            this.speedIncrease = speedIncrease;
            this.attackIncrease = attackIncrease;
            this.defenseIncrease = defenseIncrease;
            this.vigorIncrease = vigorIncrease;
        }

        protected override void Read(BitReader r)
        {
            maxHealthIncrease = r.ReadUInt8();
            speedIncrease = r.ReadUInt8();
            attackIncrease = r.ReadUInt8();
            defenseIncrease = r.ReadUInt8();
            vigorIncrease = r.ReadUInt8();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(maxHealthIncrease);
            w.Write(speedIncrease);
            w.Write(attackIncrease);
            w.Write(defenseIncrease);
            w.Write(vigorIncrease);
        }
    }
}
