using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    ProjectileCollision,
    EntityCollision,
    SoulsGained,
    AreaBlast,
    Bomb,
    Fountain,
    Splash,
    LevelUp,
    AoeProjectile,
    ItemAoeProjectile,
    AlchemistAbility,
    CommanderAbility,
    BerserkerAbility,
    Shout,
    HealLaser,
    RangerArrows,
    BrewerSelection,
    RangerArrowsShoot,
    SleepingZ,
    MusicNotes,
    QuestReceived,
    Dash,
    Emote,
    WarriorAbility,
    NomadTarget,
    MythicPixels,
    MasterOrderPixels
}

public class Effect : MonoBehaviour
{
    public EffectType type;

    public ParticleSystem system;

    protected Transform follow;

    private EffectManager manager;

    protected World world;

    protected virtual void Awake()
    {
        if (system == null)
            system = GetComponent<ParticleSystem>();
    }

    protected virtual void OnEnable()
    {
        if (system != null)
            system.Play();
    }

    public virtual void Init(EffectManager manager, World world)
    {
        this.manager = manager;
        this.world = world;
    }

    public virtual void SetFollow(Transform follow)
    {
        this.follow = follow;
    }

    protected virtual void LateUpdate()
    {
        if (!Running())
        {
            manager.ReturnEffect(this);
            return;
        }

        GotoFollow();
    }

    protected virtual bool Running()
    {
        return system.isPlaying;
    }

    protected virtual void GotoFollow()
    {
        if (follow == null) return;
        transform.position = follow.position;
    }
}
