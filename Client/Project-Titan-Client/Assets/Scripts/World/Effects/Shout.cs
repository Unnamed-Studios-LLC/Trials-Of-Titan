using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Shout : Effect
{
    public void SetInfo(float spreadDeg, float angleDeg, float radius)
    {
        transform.localEulerAngles = new Vector3(0, 0, angleDeg - spreadDeg / 2);

        var shape = system.shape;
        shape.arc = spreadDeg;

        var speed = system.velocityOverLifetime;
        speed.speedModifier = radius / 0.4f;
    }
}
