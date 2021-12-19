using System;
using System.Collections.Generic;
using System.Text;

namespace World.Instances.Packets
{
    public enum InPacketType
    {
        None,
        Verify,
        PlayerCount,
        WorldKeyRequest,
        WorldKeyResult,
        OverworldClosed
    }
}
