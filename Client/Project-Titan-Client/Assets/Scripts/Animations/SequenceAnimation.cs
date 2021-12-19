using System;
using System.Collections.Generic;
using TitanCore.Data.Components.Textures;
using UnityEngine;

public class SequenceAnimationData : AnimationData
{
    private int loops = 0;

    public SequenceAnimationData(SpriteWorldObject worldObject, Animation animation) : base(worldObject, animation)
    {
        UpdateFrames();
    }

    public override void SetState(AnimationState newState, AnimationDirection newDirection, float attackCd)
    {
        state = newState;
        SetDirection(newDirection, attackCd);
    }

    protected override void SetDirection(AnimationDirection newDirection, float attackCd)
    {
        var data = (SequenceAnimation)animation;
        if (data.noFlip) newDirection = AnimationDirection.Right;
        if (direction == newDirection) return;
        direction = newDirection;

        UpdateFrames();
    }

    protected override void Loop()
    {
        var data = (SequenceAnimation)animation;
        if (data.loops != -1 && ++loops >= data.loops) return;
        base.Loop();
    }

    protected override float GetFps()
    {
        return ((SequenceAnimation)animation).fps;
    }
}

public class SequenceAnimation : Animation
{
    public readonly int loops;

    public readonly float fps;

    public readonly bool noFlip;

    private readonly Sprite[] sequence;

    public SequenceAnimation(SequenceTextureData textureData)
    {
        var sequence = new List<Sprite>();
        Sprite sprite;
        int i = 1;
        while (true)
        {
            sprite = TextureManager.GetSprite(textureData.spriteSetName + "-" + i++);
            if (sprite == null) break;
            sequence.Add(sprite);
        }

        if (textureData.reverse)
            sequence.Reverse();

        this.sequence = sequence.ToArray();
        fps = textureData.framesPerSecondMin;
        loops = textureData.loopCount;
        noFlip = textureData.noFlip;
    }

    public override Sprite[] GetFrames(AnimationState state, AnimationDirection direction)
    {
        return sequence;
    }
}
