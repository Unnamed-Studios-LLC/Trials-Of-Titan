using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.NET.Geometry;

public class AlchemistAbility : Effect
{
    private float endTime;

    private Vector2 position;

    private float radius;

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        var options = system.main;
        options.simulationSpace = ParticleSystemSimulationSpace.Custom;
        options.customSimulationSpace = world.transform;

        transform.localEulerAngles = Vector3.zero;
    }

    public void Setup(Vector2 position, float radius, float duration)
    {
        this.position = position;
        this.radius = radius;
        endTime = Time.time + duration;

        UpdatePosition();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (Time.time >= endTime)
        {
            system.Stop();
        }
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.localPosition = position + Vec2.FromAngle((endTime - Time.time) * 5).Multiply(radius).ToVector2();
    }
}
