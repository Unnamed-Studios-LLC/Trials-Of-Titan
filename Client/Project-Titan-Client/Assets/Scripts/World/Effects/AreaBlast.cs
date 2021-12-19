using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AreaBlast : Effect
{
    private const float Spread_Time = 0.2f;

    public ParticleSystem[] systems;

    public SpriteRenderer[] sprites;

    private int activeCount = 0;

    public float radius = 5;

    private float time;

    private Vector2[] sinCos;

    private Option reduceParticles;

    protected override void Awake()
    {
        system = systems[0];

        reduceParticles = Options.Get(OptionType.ReduceParticles);
    }

    protected override void OnEnable()
    {
        
    }

    public void SetInfo(float radius, Color color)
    {
        this.radius = radius;

        time = Spread_Time;

        activeCount = reduceParticles.GetBool() ? Mathf.Clamp(2 + (int)(radius * 2), 3, 9) : Mathf.Clamp(2 + (int)(radius * 2), 4, 12);

        for (int i = 0; i < activeCount; i++)
        {
            var system = systems[i];
            system.transform.localPosition = Vector3.zero;
            system.gameObject.SetActive(true);

            var main = system.main;
            main.startColor = color;
            system.Play();

            var emission = system.emission;
            emission.rateOverDistance = reduceParticles.GetBool() ? 1.2f : 2f;

            var sprite = sprites[i];
            sprite.enabled = true;
            sprite.color = color;
        }

        for (int i = activeCount; i < systems.Length; i++)
        {
            systems[i].Stop();
            sprites[i].enabled = false;
            systems[i].gameObject.SetActive(false);
        }

        sinCos = new Vector2[activeCount];
        var sectionAngle = Mathf.PI * 2.0f / sinCos.Length;
        for (int i = 0; i < sinCos.Length; i++)
        {
            float angle = sectionAngle / 2 + sectionAngle * i;
            sinCos[i] = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        }
    }

    private void OnDisable()
    {
        foreach (var system in systems)
            system.Stop();
    }

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        foreach (var system in systems)
        {
            var options = system.main;
            options.simulationSpace = ParticleSystemSimulationSpace.Custom;
            options.customSimulationSpace = world.transform;
        }
    }

    protected override void LateUpdate()
    {
        var oldTime = time;
        time -= Time.deltaTime;
        if (oldTime > 0 && time <= 0)
        {
            for (int i = 0; i < activeCount; i++)
            {
                var system = systems[i];
                system.Stop();

                var sprite = sprites[i];
                sprite.enabled = false;
            }
        }

        for (int i = 0; i < activeCount; i++)
        {
            var system = systems[i];
            system.transform.localPosition = new Vector3(sinCos[i].y, sinCos[i].x, 0) * radius * (1.0f - time / Spread_Time);
        }

        base.LateUpdate();
    }
}
