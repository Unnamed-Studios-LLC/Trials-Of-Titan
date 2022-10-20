using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace Utils.NET.Net
{
    public class LengthCheckFailedException : Exception
    {
        public LengthCheckFailedException() : base() { }
        public LengthCheckFailedException(string message) : base(message) { }
    }

    public abstract class Packet
    {
        public abstract byte Id { get; }

        public void WritePacket(BitWriter w)
        {
            Write(w);
        }

        public void ReadPacket(BitReader r)
        {
            Read(r);
        }

        public void WriteTokenPacket(BitWriter w)
        {
            w.Write(Id);
            if (this is ITokenPacket token)
            {
                w.Write(token.Token);
                w.Write(token.TokenResponse);
            }
            Write(w);
        }

        public void ReadTokenPacket(BitReader r)
        {
            if (this is ITokenPacket token)
            {
                token.Token = r.ReadInt32();
                token.TokenResponse = r.ReadBool();
            }
            Read(r);
        }

        protected abstract void Write(BitWriter w);

        protected abstract void Read(BitReader r);
    }
}
