using System;
using System.Collections.Generic;
using System.Text;

namespace ServerNode.Net.Packets
{
    public enum NPacketType
    {
        None,
        NodeVerify,
        VersionCheck,
        VersionCheckResponse,
        Restart,
        Update,
        UpdateResponse,
        ProgramVerify,
        ProgramStop
    }
}
