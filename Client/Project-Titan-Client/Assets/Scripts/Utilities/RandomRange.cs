using UnityEngine;

public class RandomRange
{
    public float min;

    public float max;

    public float Value => min + (max - min) * Random.value;

    public RandomRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}
