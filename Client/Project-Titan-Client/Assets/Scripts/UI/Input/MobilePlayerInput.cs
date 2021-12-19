using Mobile;
using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Items;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Geometry;
using static UnityEngine.InputSystem.InputAction;

public class MobilePlayerInput : MonoBehaviour
{
    public World world;

    public Joystick leftJoystick;

    public Joystick rightJoystick;

    public SideMenuManager sideMenuManager;

    public TooltipManager tooltipManager;

    public RectTransform moveArrow;

    public RectTransform attackArrow;

    public SpriteRenderer throwGizmo;

    public SimpleButton rotateClockwiseBtn;

    public SimpleButton rotateCounterClockwiseBtn;

    public SimpleButton emoteBtn;

    public EmoteDial emoteDial;

    private float targetAngle = 0;

    private bool emotePressed;

    private float emoteDownTime;

    private EmoteType lastEmote = EmoteType.None;


    private Vector2 gamepadLeftVector;

    private Vector2 gamepadRightVector;

    private bool rotateClockwise;

    private bool rotateCounterClockwise;

    private Option drawMovementArrow;

    private Option drawAimingArrow;

    private Option southpaw;

    private Option emoteSelection;

    private void Awake()
    {
        SizeArrow(moveArrow);
        SizeArrow(attackArrow);

        drawMovementArrow = Options.Get(OptionType.DrawMovementArrow);
        drawAimingArrow = Options.Get(OptionType.DrawAimingArrow);
        southpaw = Options.Get(OptionType.SouthpawJoysticks);
        emoteSelection = Options.Get(OptionType.EmoteDial);
    }

    private void SizeArrow(RectTransform rect)
    {
        float size = Screen.height * 0.045f;
        rect.sizeDelta = new Vector2(size, size);
    }

    private void Update()
    {
        if (!world.allowControls) return;

        CheckMovementInput();
        CheckAttackingInput();
        CheckRotationInput();
        CheckEmote();
    }

    private void CheckMovementInput()
    {
        var joystick = southpaw.GetBool() ? rightJoystick : leftJoystick;
        var controllerVector = southpaw.GetBool() ? gamepadRightVector : gamepadLeftVector;

        float moveAngle = 0;
        if (joystick.active)
        {
            moveAngle = joystick.angle;
            world.player.SetMove(joystick.angle * Mathf.Deg2Rad - world.CameraRotation * Mathf.Deg2Rad, false, true);

            if (joystick.didActivate)
            {
                sideMenuManager.CloseMenu();
                tooltipManager.HideTooltip();
            }

            moveArrow.gameObject.SetActive(drawMovementArrow.GetBool());
        }
        else if (controllerVector != Vector2.zero)
        {
            moveAngle = Mathf.Atan2(controllerVector.y, controllerVector.x);
            world.player.SetMove(moveAngle - world.CameraRotation * Mathf.Deg2Rad, false, true);
            moveAngle *= Mathf.Rad2Deg;
            sideMenuManager.CloseMenu();
            tooltipManager.HideTooltip();
            moveArrow.gameObject.SetActive(drawMovementArrow.GetBool());
        }
        else
        {
            moveArrow.gameObject.SetActive(false);
            world.player.SetMove(0, false, false);
        }

        PositionArrow(moveArrow, moveAngle);
    }

    private void CheckAttackingInput()
    {
        var joystick = southpaw.GetBool() ? leftJoystick : rightJoystick;
        var controllerVector = southpaw.GetBool() ? gamepadLeftVector : gamepadRightVector;

        float attackAngle = 0;
        if (controllerVector != Vector2.zero)
        {
            attackAngle = Mathf.Atan2(controllerVector.y, controllerVector.x) * Mathf.Rad2Deg;
            var worldAttackAngle = attackAngle - world.CameraRotation;
            world.player.SetAttacking(true, worldAttackAngle, GetWorldAimPosition(worldAttackAngle, joystick.distanceScalar));
            attackArrow.gameObject.SetActive(drawAimingArrow.GetBool() && !throwGizmo.gameObject.activeSelf);
            sideMenuManager.CloseMenu();
        }
        else if (joystick.active)
        {
            attackAngle = joystick.angle;
            var worldAttackAngle = attackAngle - world.CameraRotation;
            world.player.SetAttacking(true, worldAttackAngle, GetWorldAimPosition(worldAttackAngle, joystick.distanceScalar));
            attackArrow.gameObject.SetActive(drawAimingArrow.GetBool() && !throwGizmo.gameObject.activeSelf);

            if (joystick.didActivate)
                sideMenuManager.CloseMenu();
        }
        else
        {
            world.player.SetAttacking(false, rightJoystick.angle, world.player.Position);
            attackArrow.gameObject.SetActive(false);
            throwGizmo.gameObject.SetActive(false);
        }

        PositionArrow(attackArrow, attackAngle);
    }

    private Vector2 GetWorldAimPosition(float angle, float distanceScalar)
    {
        if (world.player == null) return Vector2.zero;
        var item = world.player.GetItem(0);
        if (item.IsBlank || !(item.GetInfo() is WeaponInfo weaponInfo))
        {
            throwGizmo.gameObject.SetActive(false);
            return (Vector2)world.player.Position + Vec2.FromAngle(angle * Mathf.Deg2Rad).ToVector2();
        }

        var proj = weaponInfo.projectiles[0];
        if (!(proj is AoeProjectileData aoeData))
        {
            throwGizmo.gameObject.SetActive(false);
            return (Vector2)world.player.Position + Vec2.FromAngle(angle * Mathf.Deg2Rad).ToVector2();
        }

        throwGizmo.gameObject.SetActive(true);
        var position = (Vector2)world.player.Position + Vec2.FromAngle(angle * Mathf.Deg2Rad).Multiply(distanceScalar * 6).ToVector2();
        throwGizmo.transform.localPosition = position;
        return position;
    }

    private void CheckRotationInput()
    {
        if (rotateClockwiseBtn.down)
            rotateClockwise = true;
        else if (rotateClockwiseBtn.up)
            rotateClockwise = false;

        if (rotateCounterClockwiseBtn.down)
            rotateCounterClockwise = true;
        else if (rotateCounterClockwiseBtn.up)
            rotateCounterClockwise = false;

        if (rotateClockwise && rotateCounterClockwise)
            RotateCamera(0);
        else if (rotateClockwise)
            RotateCamera(-1);
        else if (rotateCounterClockwise)
            RotateCamera(1);
        else
            RotateCamera(0);
    }

    private void RotateCamera(int direction)
    {
        var delta = Time.deltaTime * 120;
        var dif = Mathf.Abs(Mathf.DeltaAngle(targetAngle, world.CameraRotation));
        if (world.CameraRotation != targetAngle)
        {
            if (dif > delta)
            {
                world.CameraRotation = Mathf.MoveTowardsAngle(world.CameraRotation, targetAngle, delta);
            }
            else
            {
                world.CameraRotation = targetAngle;
            }
        }
        else
        {
            if (direction == 0) return;
            targetAngle += direction * 15;
        }
    }

    private void PositionArrow(RectTransform arrow, float angleDeg)
    {
        arrow.localEulerAngles = new Vector3(0, 0, -45 + angleDeg);
        var arrowPos = Vec2.FromAngle(angleDeg * Mathf.Deg2Rad).ToVector2() * Screen.height * 0.1f;
        arrowPos.y += Screen.height * 0.015f;
        arrow.anchoredPosition = arrowPos;
    }

    public void OnJoystickMove(CallbackContext context)
    {
        gamepadLeftVector = context.ReadValue<Vector2>();
    }

    public void OnJoystickAttack(CallbackContext context)
    {
        gamepadRightVector = context.ReadValue<Vector2>();
    }

    public void OnRotateClockwise(CallbackContext context)
    {
        rotateClockwise = context.ReadValue<float>() != 0;
    }

    public void OnRotateCounterClockwise(CallbackContext context)
    {
        rotateCounterClockwise = context.ReadValue<float>() != 0;
    }

    private void CheckEmote()
    {
        emoteDownTime += Time.deltaTime;
        if (!emotePressed)
        {
            if (emoteBtn.down)
            {
                emotePressed = true;
                emoteDownTime = 0;
            }
        }

        if (emotePressed)
        {
            if (emoteBtn.up)
            {
                if (lastEmote != EmoteType.None)
                {
                    world.player.UseEmote(lastEmote);
                }
                emotePressed = false;
            }
            else if (emoteDownTime > 0.4f)
            {
                emotePressed = false;
                emoteDial.Show(emoteBtn.transform.position, emoteBtn.touchId, EmoteSelected);
            }
        }
    }

    private void EmoteSelected(int selectedEmote)
    {
        var selection = emoteSelection.GetIntArray();
        var type = (EmoteType)selection[selectedEmote];
        if (type == EmoteType.None) return;
        world.player.UseEmote(type);
        lastEmote = type;
    }
}
