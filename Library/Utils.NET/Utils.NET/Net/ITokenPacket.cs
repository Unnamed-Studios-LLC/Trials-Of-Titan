using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.NET.Net
{
    public interface ITokenPacket
    {
        int Token { get; set; }

        bool TokenResponse { get; set; }
    }
}
