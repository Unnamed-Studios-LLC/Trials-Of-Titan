using System;
using UnityEngine;

public abstract class AnimationData
{
    /// <summary>
    /// The current frame index
    /// </summary>
    public int frame;

    /// <summary>
    /// The current frames
    /// </summary>
    public Sprite[] frames;

    /// <summary>
    /// The time remaining in the current frame
    /// </summary>
    public float remainingFrameTime;

    /// <summary>
    /// The current state of the animation
    /// </summary>
    public AnimationState state = AnimationState.Still;

    /// <summary>
    /// The current direction of the animation
    /// </summary>
    public AnimationDirection direction = AnimationDirection.Right;

    /// <summary>
    /// The world object that this animates
    /// </summary>
    public SpriteWorldObject worldObject;

    /// <summary>
    /// The animation data
    /// </summary>
    protected Animation animation;

    protected bool noFlip = false;

    public AnimationData(SpriteWorldObject worldObject, Animation animation)
    {
        this.worldObject = worldObject;
        this.animation = animation;
        ResetTime(0);
        UpdateFrames();
    }

    public virtual void SetState(AnimationState newState, AnimationDirection newDirection, float attackCd)
    {
        if (state == newState)
        {
            SetDirection(newDirection, attackCd);
            return;
        }
        state = newState;
        direction = newDirection;

        UpdateFrames();
    }

    protected virtual void SetDirection(AnimationDirection newDirection, float attackCd)
    {
        if (direction == newDirection) return;
        direction = newDirection;

        ResetTime(attackCd);

        UpdateFrames();
    }

    public virtual void ResetTime(float attackCd)
    {
        frame = 0;
        remainingFrameTime = GetFps();
    }

    protected void UpdateSprite()
    {
        worldObject.SetSprite(frames[Math.Max(Math.Min(frame, frames.Length - 1), 0)], !noFlip && direction == AnimationDirection.Left);
    }

    public void Update(float deltaTime)
    {
        remainingFrameTime -= deltaTime;
        while (remainingFrameTime < 0)
        {
            frame++;
            if (frame >= frames.Length)
                Loop();
            remainingFrameTime += GetFps();
            UpdateSprite();
        }
    }

    protected virtual void Loop()
    {
        frame = 0;
    }

    protected abstract float GetFps();

    protected void UpdateFrames()
    {
        frames = animation.GetFrames(state, direction);
        UpdateSprite();
    }
}
