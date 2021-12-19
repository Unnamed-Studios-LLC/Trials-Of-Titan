using System;
using System.Collections.Generic;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Models
{
    public struct AllyProjectile
    {
        public uint ownerId;

        public uint projectileId;

        public ushort damage;

        public ushort item;

        public float angle;

        public bool reach;

        public static AllyProjectile Read(BitReader r)
        {
            var proj = new AllyProjectile();
            proj.ownerId = r.ReadUInt32();
            proj.projectileId = r.ReadUInt32();
            proj.damage = r.ReadUInt16();
            proj.item = r.ReadUInt16();
            proj.angle = r.ReadFloat();
            proj.reach = r.ReadBool();
            return proj;
        }

        public void Write(BitWriter w)
        {
            w.Write(ownerId);
            w.Write(projectileId);
            w.Write(damage);
            w.Write(item);
            w.Write(angle);
            w.Write(reach);
        }
    }

    public struct AllyAoeProjectile
    {
        public uint ownerId;

        public uint projectileId;

        public ushort damage;

        public ushort item;

        public Vec2 target;

        public static AllyAoeProjectile Read(BitReader r)
        {
            var proj = new AllyAoeProjectile();
            proj.ownerId = r.ReadUInt32();
            proj.projectileId = r.ReadUInt32();
            proj.damage = r.ReadUInt16();
            proj.item = r.ReadUInt16();
            proj.target = r.ReadVec2();
            return proj;
        }

        public void Write(BitWriter w)
        {
            w.Write(ownerId);
            w.Write(projectileId);
            w.Write(damage);
            w.Write(item);
            w.Write(target);
        }
    }

    public struct EnemyProjectile
    {
        public uint ownerId;

        public uint projectileId;

        public ushort damage;

        public byte index;

        public float angle;

        public Vec2 position;

        public static EnemyProjectile Read(BitReader r)
        {
            var proj = new EnemyProjectile();
            proj.ownerId = r.ReadUInt32();
            proj.projectileId = r.ReadUInt32();
            proj.damage = r.ReadUInt16();
            proj.index = r.ReadUInt8();
            proj.angle = r.ReadFloat();
            proj.position = r.ReadVec2();
            return proj;
        }

        public void Write(BitWriter w)
        {
            w.Write(ownerId);
            w.Write(projectileId);
            w.Write(damage);
            w.Write(index);
            w.Write(angle);
            w.Write(position);
        }
    }

    public struct EnemyAoeProjectile
    {
        public uint ownerId;

        public uint projectileId;

        public ushort damage;

        public byte index;

        public Vec2 target;

        public static EnemyAoeProjectile Read(BitReader r)
        {
            var proj = new EnemyAoeProjectile();
            proj.ownerId = r.ReadUInt32();
            proj.projectileId = r.ReadUInt32();
            proj.damage = r.ReadUInt16();
            proj.index = r.ReadUInt8();
            proj.target = r.ReadVec2();
            return proj;
        }

        public void Write(BitWriter w)
        {
            w.Write(ownerId);
            w.Write(projectileId);
            w.Write(damage);
            w.Write(index);
            w.Write(target);
        }
    }
}
