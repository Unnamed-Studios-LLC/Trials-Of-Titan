using System;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnTick : TnIdPacket
    {
        public override TnPacketType Type => TnPacketType.Tick;

        /// <summary>
        /// Server damages
        /// </summary>
        //public ServerDamage[] damages;

        /// <summary>
        /// Array of stats for newly encountered objects
        /// </summary>
        public NewObjectStats[] newObjects;

        /// <summary>
        /// Array of stats for objects that have been updated
        /// </summary>
        public UpdatedObjectStats[] updatedObjects;

        /// <summary>
        /// Array of removed objects
        /// </summary>
        public uint[] removedObjects;

        public TnTick()
        {

        }

        public TnTick(uint tickId, NewObjectStats[] newObjects, UpdatedObjectStats[] updatedObjects, uint[] removedObjects) : base(tickId)
        {
            this.newObjects = newObjects;
            this.updatedObjects = updatedObjects;
            this.removedObjects = removedObjects;
        }

        protected override void Read(BitReader r)
        {
            base.Read(r);

            /*
            damages = new ServerDamage[r.ReadUInt16()];
            for (int i = 0; i < damages.Length; i++)
                damages[i] = new ServerDamage(r);
            */

            newObjects = new NewObjectStats[r.ReadUInt16()];
            for (int i = 0; i < newObjects.Length; i++)
                newObjects[i] = new NewObjectStats(r);

            updatedObjects = new UpdatedObjectStats[r.ReadUInt16()];
            for (int i = 0; i < updatedObjects.Length; i++)
                updatedObjects[i] = new UpdatedObjectStats(r);

            removedObjects = new uint[r.ReadUInt16()];
            for (int i = 0; i < removedObjects.Length; i++)
                removedObjects[i] = r.ReadUInt32();
        }

        protected override void Write(BitWriter w)
        {
            base.Write(w);

            /*
            w.Write((ushort)damages.Length);
            for (int i = 0; i < damages.Length; i++)
                damages[i].Write(w);
            */

            w.Write((ushort)newObjects.Length);
            for (int i = 0; i < newObjects.Length; i++)
                newObjects[i].Write(w);

            w.Write((ushort)updatedObjects.Length);
            for (int i = 0; i < updatedObjects.Length; i++)
                updatedObjects[i].Write(w);

            w.Write((ushort)removedObjects.Length);
            for (int i = 0; i < removedObjects.Length; i++)
                w.Write(removedObjects[i]);
        }
    }
}
