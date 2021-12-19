using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.NET.Geometry;

public class LevelUp : Effect
{
    public ParticleSystem blastSystem;

    private float angle;

    private float height;

    private bool blasted = false;

    public override void SetFollow(Transform follow)
    {
        base.SetFollow(follow);

        system.Stop();

        angle = Mathf.PI * 2f;
        height = 0.1f;
        blasted = false;
        GotoOffset();

        system.Play();
    }

    protected override void GotoFollow()
    {
        base.GotoFollow();

        height *= 1 + Time.deltaTime * 0.8f;
        angle *= 1 + Time.deltaTime * 0.6f; //Time.deltaTime * Mathf.PI * 2;

        GotoOffset();

        if (height > 1.5f)
        {
            system.Stop();

            if (!blasted)
            {
                blasted = true;
                blastSystem.Emit(15);
            }
        }
    }

    private void GotoOffset()
    {
        var angleVector = Vec2.FromAngle(angle) * (0.1f + (1.0f - (height / 1.5f)) * 0.6f);
        var offset = new Vector3(angleVector.x, angleVector.y, -height);

        system.transform.localPosition = offset;
    }

    protected override bool Running()
    {
        return base.Running() && blastSystem.isPlaying;
    }
}
