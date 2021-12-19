using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Components.Textures;
using UnityEngine;

public abstract class NotPlayable : Entity
{
    private Vector3 lastPosition;

    private float lastDirectionAngle = 0;

    protected int shootCooldown = 0;

    protected float attackAngle = 0;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        if (animation is EntityAnimationData entityAnimation)
            entityAnimation.remainingFrameTime = UnityEngine.Random.value * entityAnimation.walkFps;
    }

    protected override void LateUpdate()
    {
        if (world.stopTick) return;

        base.LateUpdate();

        UpdateEntityAnimation();
    }

    protected virtual bool IsAttacking => shootCooldown > 0;

    protected void UpdateEntityAnimation()
    {
        var state = AnimationState.Still;
        var direction = lastDirectionAngle;
        var position = Position;

        if (IsAttacking)
        {
            state = AnimationState.Attack;
            //direction = attackAngle;
        }
        else if (position != lastPosition)
        {
            var movementVector = position - lastPosition;
            state = AnimationState.Walk;
            if (!stopped)
                direction = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
            lastPosition = position;
        }

        lastDirectionAngle = direction;

        var animDirection = GetAnimationDirection(direction);
        if (animation != null)
            animation.SetState(state, animDirection, shootCooldown / 1000f);
        else
            spriteRenderer.flipX = animDirection == AnimationDirection.Left;
    }

    private AnimationDirection GetAnimationDirection(float angle)
    {
        angle += world.CameraRotation;
        angle = angle + Mathf.Ceil(-angle / 360) * 360;

        if (angle >= 89 && angle < 269)
        {
            return AnimationDirection.Left;
        }
        else
        {
            return AnimationDirection.Right;
        }
    }
}
