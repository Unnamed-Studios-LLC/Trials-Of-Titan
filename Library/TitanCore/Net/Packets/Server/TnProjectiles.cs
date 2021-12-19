using System;
using System.Collections.Generic;
using System.Text;
using TitanCore.Net.Packets.Models;
using Utils.NET.IO;

namespace TitanCore.Net.Packets.Server
{
    public class TnProjectiles : TnIdPacket
    {
        public override TnPacketType Type => TnPacketType.Projectiles;

        public AllyProjectile[] allyProjectiles;

        public AllyAoeProjectile[] allyAoeProjectiles;

        public EnemyProjectile[] enemyProjectiles;

        public EnemyAoeProjectile[] enemyAoeProjectiles;

        public TnProjectiles()
        {

        }

        public TnProjectiles(uint tickId, AllyProjectile[] allyProjectiles, AllyAoeProjectile[] allyAoeProjectiles, EnemyProjectile[] enemyProjectiles, EnemyAoeProjectile[] enemyAoeProjectiles) : base(tickId)
        {
            this.allyProjectiles = allyProjectiles;
            this.allyAoeProjectiles = allyAoeProjectiles;
            this.enemyProjectiles = enemyProjectiles;
            this.enemyAoeProjectiles = enemyAoeProjectiles;
        }

        protected override void Read(BitReader r)
        {
            base.Read(r);

            allyProjectiles = new AllyProjectile[r.ReadUInt16()];
            for (int i = 0; i < allyProjectiles.Length; i++)
                allyProjectiles[i] = AllyProjectile.Read(r);

            allyAoeProjectiles = new AllyAoeProjectile[r.ReadUInt16()];
            for (int i = 0; i < allyAoeProjectiles.Length; i++)
                allyAoeProjectiles[i] = AllyAoeProjectile.Read(r);

            enemyProjectiles = new EnemyProjectile[r.ReadUInt16()];
            for (int i = 0; i < enemyProjectiles.Length; i++)
                enemyProjectiles[i] = EnemyProjectile.Read(r);

            enemyAoeProjectiles = new EnemyAoeProjectile[r.ReadUInt16()];
            for (int i = 0; i < enemyAoeProjectiles.Length; i++)
                enemyAoeProjectiles[i] = EnemyAoeProjectile.Read(r);
        }

        protected override void Write(BitWriter w)
        {
            base.Write(w);

            w.Write((ushort)allyProjectiles.Length);
            for (int i = 0; i < allyProjectiles.Length; i++)
                allyProjectiles[i].Write(w);

            w.Write((ushort)allyAoeProjectiles.Length);
            for (int i = 0; i < allyAoeProjectiles.Length; i++)
                allyAoeProjectiles[i].Write(w);

            w.Write((ushort)enemyProjectiles.Length);
            for (int i = 0; i < enemyProjectiles.Length; i++)
                enemyProjectiles[i].Write(w);

            w.Write((ushort)enemyAoeProjectiles.Length);
            for (int i = 0; i < enemyAoeProjectiles.Length; i++)
                enemyAoeProjectiles[i].Write(w);
        }
    }
}
