using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Components.Textures;
using UnityEngine;

public class EntityAnimationData : AnimationData
{
    /// <summary>
    /// The frames per second of walking
    /// </summary>
    public float walkFps = 1.0f / 3.0f;

    /// <summary>
    /// The attack frames per second
    /// </summary>
    public float attackFps = 1.0f / 5.0f;

    public EntityAnimationData(SpriteWorldObject worldObject, EntityAnimation animation) : base(worldObject, animation)
    {
        noFlip = animation.noFlip;
        UpdateFrames();
    }

    public override void SetState(AnimationState newState, AnimationDirection newDirection, float attackCd)
    {
        if (state == newState)
        {
            SetDirection(newDirection, attackCd);
            return;
        }

        state = newState;
        direction = newDirection;

        ResetTime(attackCd);

        if (state == AnimationState.Attack)
            frame = 1;

        UpdateFrames();
    }

    protected override float GetFps()
    {
        return state == AnimationState.Attack ? attackFps : walkFps;
    }
}

public class EntityAnimation : Animation
{
    /// <summary>
    /// Sprite to display while still
    /// </summary>
    public Sprite[] still;

    /// <summary>
    /// Sprites to be looped while walking
    /// </summary>
    public Sprite[] walk;

    /// <summary>
    /// Sprites to be looped while attacking
    /// </summary>
    public Sprite[] attack;

    /// <summary>
    /// If the texture should not flip based on direction
    /// </summary>
    public bool noFlip;

    public EntityAnimation(EntityTextureData texture)
    {
        noFlip = texture.noFlip;
        int index = 1;
        still = new Sprite[] { TextureManager.GetSprite(texture.spriteSetName + '-' + index) };

        if (texture.separateWalk)
            index++;

        if (texture.noWalk)
        {
            walk = new Sprite[]
            {
                still[0],
                still[0]
            };
            index++;
        }
        else
        {
            walk = new Sprite[]
            {
                TextureManager.GetSprite(texture.spriteSetName + '-' + (index + 1)),
                TextureManager.GetSprite(texture.spriteSetName + '-' + (index))
            };
            index += 2;
        }

        if (texture.noAttack)
            attack = new Sprite[]
            {
                still[0],
                still[0]
            };
        else
            attack = new Sprite[]
            {
                TextureManager.GetSprite(texture.spriteSetName + '-' + index++),
                TextureManager.GetSprite(texture.spriteSetName + '-' + index++)
            };
    }

    public override Sprite[] GetFrames(AnimationState state, AnimationDirection direction)
    {
        switch (state)
        {
            case AnimationState.Walk:
                return walk;
            case AnimationState.Attack:
                return attack;
            default:
                return still;
        };
    }
}
