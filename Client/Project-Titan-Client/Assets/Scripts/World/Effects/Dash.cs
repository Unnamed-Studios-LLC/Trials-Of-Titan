using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using UnityEngine;

public class Dash : Effect
{
    private Entity entity;

    private ParticleSystemRenderer systemRenderer;

    protected override void Awake()
    {
        base.Awake();

        systemRenderer = GetComponent<ParticleSystemRenderer>();
    }

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        var options = system.main;
        options.simulationSpace = ParticleSystemSimulationSpace.Custom;
        options.customSimulationSpace = world.transform;
    }

    public void SetInfo(Entity entity)
    {
        this.entity = entity;
        SetFollow(entity.transform);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (entity != null && entity.gameObject)
        {
            var textureSheet = system.textureSheetAnimation;
            textureSheet.SetSprite(0, entity.spriteRenderer.sprite);

            systemRenderer.flip = new Vector3(entity.spriteRenderer.flipX ? 1 : 0, 0, 0);
        }

        if (system.isPlaying && !entity.HasStatusEffect(StatusEffect.Dashing))
        {
            system.Stop();
        }
    }
}