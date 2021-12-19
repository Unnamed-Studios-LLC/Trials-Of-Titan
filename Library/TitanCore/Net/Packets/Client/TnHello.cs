using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Client
{
    public class TnHello : TnPacket
    {
        public override TnPacketType Type => TnPacketType.Hello;

        /// <summary>
        /// The token used to access/login to the game world
        /// </summary>
        public string accessToken;

        /// <summary>
        /// The character to load into the game
        /// </summary>
        public ulong characterId;

        /// <summary>
        /// The type of class to create, 0 if loading a character
        /// </summary>
        public ushort createType;

        /// <summary>
        /// The world to join
        /// </summary>
        public uint worldId;

        /// <summary>
        /// The access token for this world
        /// </summary>
        public ulong worldAccessToken;

        /// <summary>
        /// The version of the game this client is running
        /// </summary>
        public string buildVersion;

        public TnHello()
        {

        }

        public TnHello(string accessToken, ulong characterId, ushort createType, uint worldId, ulong worldAccessToken)
        {
            this.accessToken = accessToken;
            this.characterId = characterId;
            this.createType = createType;
            this.worldId = worldId;
            this.worldAccessToken = worldAccessToken;
        }

        protected override void Read(BitReader r)
        {
            accessToken = r.ReadUTF(256);
            characterId = r.ReadUInt64();
            createType = r.ReadUInt16();
            worldId = r.ReadUInt32();
            worldAccessToken = r.ReadUInt64();
            buildVersion = r.ReadUTF(20);
        }

        protected override void Write(BitWriter w)
        {
            w.Write(accessToken);
            w.Write(characterId);
            w.Write(createType);
            w.Write(worldId);
            w.Write(worldAccessToken);
            w.Write(NetConstants.Build_Version);
        }
    }
}
