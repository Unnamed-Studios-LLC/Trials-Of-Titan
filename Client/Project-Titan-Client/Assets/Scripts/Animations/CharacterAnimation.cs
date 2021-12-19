using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data.Components.Textures;
using UnityEngine;

public class CharacterAnimationData : AnimationData
{
    /// <summary>
    /// The frames per second of walking
    /// </summary>
    public float walkFps = 1.0f / 3.0f;

    /// <summary>
    /// The attack frames per second
    /// </summary>
    public float attackFps = 1.0f / 4.0f;

    public CharacterAnimationData(SpriteWorldObject worldObject, CharacterAnimation animation) : base(worldObject, animation)
    {
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

        if (state == AnimationState.Attack)
        {
            GetAttackData(attackCd, out frame, out remainingFrameTime);
        }
        else
        {
            ResetTime(attackCd);
        }

        UpdateFrames();
    }

    public override void ResetTime(float attackCd)
    {
        if (state == AnimationState.Attack)
            GetAttackData(attackCd, out frame, out remainingFrameTime);
        else
            base.ResetTime(attackCd);
    }

    protected override void SetDirection(AnimationDirection newDirection, float attackCd)
    {
        if (state == AnimationState.Attack)
        {
            int oldFrame = frame;
            GetAttackData(attackCd, out frame, out remainingFrameTime);
            if (oldFrame != frame && direction == newDirection)
                UpdateFrames();
            else
                base.SetDirection(newDirection, attackCd);
        }
        else
            base.SetDirection(newDirection, attackCd);
    }

    protected override float GetFps()
    {
        return state == AnimationState.Attack ? attackFps : walkFps;
    }

    private void GetAttackData(float attackCd, out int frame, out float remainingFrameTime)
    {
        if (attackCd > attackFps)
        {
            frame = 0;
            remainingFrameTime = attackCd - attackFps;
        }
        else
        {
            frame = 1;
            remainingFrameTime = attackCd;
        }
    }
}

public class CharacterAnimation : Animation
{
    /// <summary>
    /// Still animations
    /// </summary>
    public readonly Sprite[] stillSide;
    public readonly Sprite[] stillUp;
    public readonly Sprite[] stillDown;

    /// <summary>
    /// Walk animations
    /// </summary>
    public readonly Sprite[] walkSide;
    public readonly Sprite[] walkUp;
    public readonly Sprite[] walkDown;

    /// <summary>
    /// Attack animations
    /// </summary>
    public readonly Sprite[] attackSide;
    public readonly Sprite[] attackUp;
    public readonly Sprite[] attackDown;

    /// <summary>
    /// All states
    /// </summary>
    public readonly Sprite[] allSide;
    public readonly Sprite[] allUp;
    public readonly Sprite[] allDown;

    public CharacterAnimation(CharacterTextureData textureData)
    {
        stillSide = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-1") };
        stillUp = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-5") };
        stillDown = new Sprite[] { TextureManager.GetSprite(textureData.spriteSetName + "-10") };

        walkSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-2"),
                stillSide[0]
        };
        walkUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-6"),
                TextureManager.GetSprite(textureData.spriteSetName + "-7")
        };
        walkDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-11"),
                TextureManager.GetSprite(textureData.spriteSetName + "-12")
        };

        attackSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-3"),
                TextureManager.GetSprite(textureData.spriteSetName + "-4")
        };
        attackUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-8"),
                TextureManager.GetSprite(textureData.spriteSetName + "-9")
        };
        attackDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-13"),
                TextureManager.GetSprite(textureData.spriteSetName + "-14")
        };

        allSide = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-1"),
                TextureManager.GetSprite(textureData.spriteSetName + "-2"),
                TextureManager.GetSprite(textureData.spriteSetName + "-4"),
                TextureManager.GetSprite(textureData.spriteSetName + "-3")
        };
        allUp = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-5"),
                TextureManager.GetSprite(textureData.spriteSetName + "-6"),
                TextureManager.GetSprite(textureData.spriteSetName + "-7"),
                TextureManager.GetSprite(textureData.spriteSetName + "-9"),
                TextureManager.GetSprite(textureData.spriteSetName + "-8")
        };
        allDown = new Sprite[]
        {
                TextureManager.GetSprite(textureData.spriteSetName + "-10"),
                TextureManager.GetSprite(textureData.spriteSetName + "-11"),
                TextureManager.GetSprite(textureData.spriteSetName + "-12"),
                TextureManager.GetSprite(textureData.spriteSetName + "-14"),
                TextureManager.GetSprite(textureData.spriteSetName + "-13")
        };
    }

    public override Sprite[] GetFrames(AnimationState state, AnimationDirection direction)
    {
        switch (state)
        {
            case AnimationState.Walk:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return walkUp;
                    case AnimationDirection.Down:
                        return walkDown;
                    default:
                        return walkSide; 
                }
            case AnimationState.Attack:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return attackUp;
                    case AnimationDirection.Down:
                        return attackDown;
                    default:
                        return attackSide;
                }
            case AnimationState.All:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return allUp;
                    case AnimationDirection.Down:
                        return allDown;
                    default:
                        return allSide;
                }
            default:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return stillUp;
                    case AnimationDirection.Down:
                        return stillDown;
                    default:
                        return stillSide;
                }
        }
    }
}