using System;
using UnityEngine;

public enum AnimationDirection
{
    Right,
    Up,
    Left,
    Down
}

public enum AnimationState
{
    Still,
    Walk,
    Attack,
    All,
    None
}

public abstract class Animation
{
    public abstract Sprite[] GetFrames(AnimationState state, AnimationDirection direction);
}
