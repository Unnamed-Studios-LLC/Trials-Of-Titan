using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Map;
using UnityEngine;

public class Splash : Effect
{
    public Color[] colors;

    private bool setColors = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        setColors = false;
    }

    public void SetTile(TileInfo info)
    {
        var metaData = TextureManager.GetMetaData(TextureManager.GetDisplaySprite(info));
        colors = metaData.colors;
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
            var color = colors[UnityEngine.Random.Range(0, colors.Length)];
            particles[i].startColor = new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }
        system.SetParticles(particles);
    }
}
