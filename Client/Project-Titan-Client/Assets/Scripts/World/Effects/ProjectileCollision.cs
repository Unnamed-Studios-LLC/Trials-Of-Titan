using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileCollision : Effect
{
    public Color[] colors;

    private bool setColors = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        setColors = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (system.particleCount == 0) return;
        if (setColors) return;
        setColors = true;

        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);
        for (int i = 0; i < particles.Length; i++)
        {
            var color = colors[Random.Range(0, colors.Length)];
            particles[i].startColor = new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }
        system.SetParticles(particles);
    }
}
