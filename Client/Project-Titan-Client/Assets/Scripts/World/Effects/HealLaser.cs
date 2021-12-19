using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HealLaser : Effect
{
    private Vector3 target;

    private bool positioned = false;

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        positioned = false;
    }

    protected override void LateUpdate()
    {
        if (system.isPlaying && !positioned)
        {
            transform.localPosition = target;
            positioned = true;
        }
        else
            system.Stop();

        base.LateUpdate();
    }
}
