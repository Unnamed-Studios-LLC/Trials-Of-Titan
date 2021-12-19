using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace ServerNode.Net.Packets
{
    public class NProgramVerify : NPacket
    {
        public override NPacketType Type => NPacketType.ProgramVerify;

        private static ulong Verification_Token = 895845505454562386;

        private ulong token;

        public int programType;

        public int processId;

        public NProgramVerify(int programType, int processId)
        {
            this.programType = programType;
            this.processId = processId;
        }

        public NProgramVerify()
        {

        }

        protected override void Read(BitReader r)
        {
            token = r.ReadUInt64();
            programType = r.ReadInt32();
            processId = r.ReadInt32();
        }

        protected override void Write(BitWriter w)
        {
            w.Write(Verification_Token);
            w.Write(programType);
            w.Write(processId);
        }

        public bool IsValid() => token == Verification_Token;
    }
}
