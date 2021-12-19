using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NET.Geometry;
using Utils.NET.IO;

namespace TitanCore.Core
{
    public enum WorldEffectType
    {
        Blast,
        Bomb,
        BombBlast,
        LevelUp,
        HealLaser,
        WarriorAbility,
        AlchemistAbility,
        LancerAbility,
        CommanderAbility,
        MinisterAbility,
        BerserkerAbility,
        RangerAbility,
        BrewerAbility,
        BladeweaverAbility,
        NomadAbility
    }

    public abstract class WorldEffect
    {
        public abstract WorldEffectType Type { get; }

        public static WorldEffect Read(BitReader r)
        {
            var type = (WorldEffectType)r.ReadUInt8();
            var effect = CreateEffect(type);
            effect.DoRead(r);
            return effect;
        }

        private static Dictionary<WorldEffectType, Type> effectTypes;

        static WorldEffect()
        {
            var baseType = typeof(WorldEffect);
            effectTypes = baseType.Assembly.GetTypes().Where(_ => _.IsSubclassOf(baseType) && !_.IsAbstract).ToDictionary(_ => ((WorldEffect)Activator.CreateInstance(_)).Type);
        }

        private static WorldEffect CreateEffect(WorldEffectType type)
        {
            if (effectTypes.TryGetValue(type, out var effectType))
                return (WorldEffect)Activator.CreateInstance(effectType);
            return null;
        }

        protected abstract void DoRead(BitReader r);

        public void Write(BitWriter w)
        {
            w.Write((byte)Type);
            DoWrite(w);
        }

        protected abstract void DoWrite(BitWriter w);
    }

    public class BlastWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.Blast;

        public Vec2 position;

        public float area;

        public BlastWorldEffect()
        {

        }

        public BlastWorldEffect(Vec2 position, float area)
        {
            this.position = position;
            this.area = area;
        }

        protected override void DoRead(BitReader r)
        {
            position = r.ReadVec2();
            area = r.ReadFloat();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(position);
            w.Write(area);
        }
    }

    public class BombWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.Bomb;

        public bool exactPosition;

        public uint positionGameId;

        public Vec2 position;

        public Vec2 target;

        public float time;

        public BombWorldEffect()
        {

        }

        public BombWorldEffect(Vec2 position, Vec2 target, float time)
        {
            exactPosition = true;
            this.position = position;
            this.target = target;
            this.time = time;
        }

        public BombWorldEffect(uint positionGameId, Vec2 target, float time)
        {
            exactPosition = false;
            this.positionGameId = positionGameId;
            this.target = target;
            this.time = time;
        }

        protected override void DoRead(BitReader r)
        {
            exactPosition = r.ReadBool();
            if (exactPosition)
                position = r.ReadVec2();
            else
                positionGameId = r.ReadUInt32();
            target = r.ReadVec2();
            time = r.ReadFloat();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(exactPosition);
            if (exactPosition)
                w.Write(position);
            else
                w.Write(positionGameId);
            w.Write(target);
            w.Write(time);
        }
    }

    public class BombBlastWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.BombBlast;

        public bool exactPosition;

        public uint positionGameId;

        public Vec2 position;

        public Vec2 target;

        public float area;

        public float time;

        public BombBlastWorldEffect()
        {

        }

        public BombBlastWorldEffect(Vec2 position, Vec2 target, float area, float time)
        {
            exactPosition = true;
            this.position = position;
            this.target = target;
            this.area = area;
            this.time = time;
        }

        public BombBlastWorldEffect(uint positionGameId, Vec2 target, float area, float time)
        {
            exactPosition = false;
            this.positionGameId = positionGameId;
            this.target = target;
            this.area = area;
            this.time = time;
        }

        protected override void DoRead(BitReader r)
        {
            exactPosition = r.ReadBool();
            if (exactPosition)
                position = r.ReadVec2();
            else
                positionGameId = r.ReadUInt32();
            target = r.ReadVec2();
            area = r.ReadFloat();
            time = r.ReadFloat();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(exactPosition);
            if (exactPosition)
                w.Write(position);
            else
                w.Write(positionGameId);
            w.Write(target);
            w.Write(area);
            w.Write(time);
        }
    }

    public class LevelUpWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.LevelUp;

        public uint gameId;

        public LevelUpWorldEffect()
        {

        }

        public LevelUpWorldEffect(uint gameId)
        {
            this.gameId = gameId;
        }

        protected override void DoRead(BitReader r)
        {
            gameId = r.ReadUInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(gameId);
        }
    }

    public class HealLaserWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.HealLaser;

        public uint sourceGameId;

        public uint targetGameId;

        public HealLaserWorldEffect()
        {

        }

        public HealLaserWorldEffect(uint sourceGameId, uint targetGameId)
        {
            this.sourceGameId = sourceGameId;
            this.targetGameId = targetGameId;
        }

        protected override void DoRead(BitReader r)
        {
            sourceGameId = r.ReadUInt32();
            targetGameId = r.ReadUInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(sourceGameId);
            w.Write(targetGameId);
        }
    }

    public class WarriorAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.WarriorAbility;

        public uint ownerGameId;

        public WarriorAbilityWorldEffect()
        {

        }

        public WarriorAbilityWorldEffect(uint ownerGameId)
        {
            this.ownerGameId = ownerGameId;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
        }
    }

    public class AlchemistAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.AlchemistAbility;

        public uint ownerGameId;

        public Vec2 target;

        public byte rage;

        public int attack;

        public AlchemistAbilityWorldEffect()
        {

        }

        public AlchemistAbilityWorldEffect(uint ownerGameId, Vec2 target, byte rage, int attack)
        {
            this.ownerGameId = ownerGameId;
            this.target = target;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            target = r.ReadVec2();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(target);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class LancerAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.LancerAbility;

        public uint ownerGameId;

        public float angle;

        public byte rage;

        public int attack;

        public LancerAbilityWorldEffect()
        {

        }

        public LancerAbilityWorldEffect(uint ownerGameId, float angle, byte rage, int attack)
        {
            this.ownerGameId = ownerGameId;
            this.angle = angle;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            angle = r.ReadFloat();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(angle);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class CommanderAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.CommanderAbility;

        public uint ownerGameId;

        public Vec2 position;

        public byte rage;

        public int attack;

        public CommanderAbilityWorldEffect()
        {

        }

        public CommanderAbilityWorldEffect(uint ownerGameId, Vec2 position, byte rage, int attack)
        {
            this.ownerGameId = ownerGameId;
            this.position = position;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            position = r.ReadVec2();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(position);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class MinisterAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.MinisterAbility;

        public uint ownerGameId;

        public Vec2 position;

        public byte rage;

        public int attack;

        public MinisterAbilityWorldEffect()
        {

        }

        public MinisterAbilityWorldEffect(uint ownerGameId, Vec2 position, byte rage, int attack)
        {
            this.ownerGameId = ownerGameId;
            this.position = position;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            position = r.ReadVec2();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(position);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class BerserkerAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.BerserkerAbility;

        public uint ownerGameId;

        public Vec2 position;

        public float angle;

        public byte rage;

        public int attack;

        public BerserkerAbilityWorldEffect()
        {

        }

        public BerserkerAbilityWorldEffect(uint ownerGameId, Vec2 position, float angle, byte rage, int attack)
        {
            this.ownerGameId = ownerGameId;
            this.position = position;
            this.angle = angle;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            position = r.ReadVec2();
            angle = r.ReadFloat();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(position);
            w.Write(angle);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class RangerAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.RangerAbility;

        public uint[] hit;

        public Vec2 position;

        public byte rage;

        public int attack;

        public RangerAbilityWorldEffect()
        {

        }

        public RangerAbilityWorldEffect(uint[] hit, Vec2 position, byte rage, int attack)
        {
            this.hit = hit;
            this.position = position;
            this.rage = rage;
            this.attack = attack;
        }

        protected override void DoRead(BitReader r)
        {
            hit = new uint[r.ReadUInt8()];
            for (int i = 0; i < hit.Length; i++)
                hit[i] = r.ReadUInt32();
            position = r.ReadVec2();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write((byte)hit.Length);
            for (int i = 0; i < hit.Length; i++)
                w.Write(hit[i]);
            w.Write(position);
            w.Write(rage);
            w.Write(attack);
        }
    }

    public class BrewerAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.BrewerAbility;

        public uint ownerGameId;

        public Vec2 position;

        public byte rage;

        public int attack;

        public byte value;

        public BrewerAbilityWorldEffect()
        {

        }

        public BrewerAbilityWorldEffect(uint ownerGameId, Vec2 position, byte rage, int attack, byte value)
        {
            this.ownerGameId = ownerGameId;
            this.position = position;
            this.rage = rage;
            this.attack = attack;
            this.value = value;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            position = r.ReadVec2();
            rage = r.ReadUInt8();
            attack = r.ReadInt32();
            value = r.ReadUInt8();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(position);
            w.Write(rage);
            w.Write(attack);
            w.Write(value);
        }
    }

    public class BladeweaverAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.BladeweaverAbility;

        public uint ownerGameId;

        public BladeweaverAbilityWorldEffect()
        {

        }

        public BladeweaverAbilityWorldEffect(uint ownerGameId)
        {
            this.ownerGameId = ownerGameId;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
        }
    }

    public class NomadAbilityWorldEffect : WorldEffect
    {
        public override WorldEffectType Type => WorldEffectType.NomadAbility;

        public uint ownerGameId;

        public Vec2 target;

        public NomadAbilityWorldEffect()
        {

        }

        public NomadAbilityWorldEffect(uint ownerGameId, Vec2 target)
        {
            this.ownerGameId = ownerGameId;
            this.target = target;
        }

        protected override void DoRead(BitReader r)
        {
            ownerGameId = r.ReadUInt32();
            target = r.ReadVec2();
        }

        protected override void DoWrite(BitWriter w)
        {
            w.Write(ownerGameId);
            w.Write(target);
        }
    }
}
