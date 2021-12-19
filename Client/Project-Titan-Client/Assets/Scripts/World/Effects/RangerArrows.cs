using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RangerArrows : Effect
{
    public void SetInfo(float radius)
    {
        var shape = system.shape;
        shape.radius = radius;
    }
}
