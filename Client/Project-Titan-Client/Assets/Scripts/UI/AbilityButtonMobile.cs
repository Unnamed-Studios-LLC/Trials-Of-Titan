using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.NET.Geometry;

public class AbilityButtonMobile : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image placeholder;

    public Image cooldownBlocker;

    public World world;

    public Slider rageUseGauge;

    public Image aimingGizmo;

    private bool full = true;

    private float abilityDownTime;

    private bool abilityPressed;

    private bool abilityDown;

    private bool abilityUp;

    public void OnAbility()
    {
        if (world.player == null) return;
        world.player.UseAbility(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var classType = (ClassType)world.player.info.id;
        if (classType == ClassType.Ranger)
        {
            aimingGizmo.gameObject.SetActive(true);
            aimingGizmo.gameObject.transform.position = eventData.position;
            abilityPressed = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        var classType = (ClassType)world.player.info.id;
        if (classType == ClassType.Ranger || classType == ClassType.Nomad)
        {
            aimingGizmo.gameObject.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var classType = (ClassType)world.player.info.id;
        if (classType == ClassType.Ranger || classType == ClassType.Nomad)
        {
            aimingGizmo.gameObject.SetActive(false);
            var aimPos = WorldPositionFromScreen(eventData.position);
            world.player.SetAbilityAimPosition(aimPos);
            world.player.UseAbility(true);
        }
    }

    private Vector2 GetCenterOffset(Vector3 screenPos)
    {
        Vector3 centerPoint = new Vector3(Screen.width, Screen.height) / 2;
        Vector3 aimVector = screenPos - centerPoint;
        return aimVector;
    }

    private Vector2 WorldPositionFromScreen(Vector2 screenPos)
    {
        var centerOffset = GetCenterOffset(screenPos) * (world.worldCamera.targetTexture.height / (float)Screen.height);
        var playerPosition = (Vector2)world.player.transform.position;
        var worldScale = (world.worldCamera.orthographicSize * 2) / world.worldCamera.targetTexture.height;
        var universalPosition = playerPosition + centerOffset * worldScale;
        return world.transform.InverseTransformPoint(universalPosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        abilityDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        abilityUp = true;
    }

    private void CheckAbilityInput()
    {
        var classType = (ClassType)world.player.info.id;
        if (classType == ClassType.Nomad)
        {
            if (abilityUp && !abilityPressed)
            {
                if (abilityPressed)
                {
                    abilityPressed = false;
                }
                else
                {
                    world.player.SetAbilityAimPosition(world.player.Position);
                    world.player.UseAbility(true);
                }
            }
        }
        else if (classType == ClassType.Ranger)
        {
            if (abilityUp && !abilityPressed)
            {
                if (abilityPressed)
                {
                    abilityPressed = false;
                }
                else
                {
                    world.player.SetAbilityAimPosition((Vector2)world.player.Position + Vec2.FromAngle(world.player.GetAttackAngle() * Mathf.Deg2Rad).Multiply(6).ToVector2());
                    world.player.UseAbility(true);
                }
            }
        }
        else if (classType == ClassType.Brewer)
        {
            if (abilityDown)
            {
                abilityDownTime = Time.time;
                abilityPressed = true;
            }
            else if (abilityPressed)
            {
                if (abilityUp)
                {
                    world.player.IncrementAbilityValue();
                    abilityPressed = false;
                }
                else
                {
                    float heldTime = Time.time - abilityDownTime;
                    if (heldTime >= 0.7f)
                    {
                        world.player.UseAbility(true);
                        abilityPressed = false;
                    }
                }
            }
        }
        else if (classType == ClassType.Bladeweaver)
        {
            if (abilityDown)
            {
                abilityDownTime = Time.time;
                abilityPressed = true;
            }
            else if (abilityPressed)
            {
                float heldTime = Time.time - abilityDownTime;
                int rageToUse = Mathf.Min(Mathf.Clamp((int)(heldTime * 100), 0, world.player.rage), AbilityFunctions.BladeWeaver.Max_Dash_Rage);
                if (abilityUp)
                {
                    world.player.SetAbilityValue((byte)rageToUse);
                    world.player.UseAbility(true);

                    rageUseGauge.value = 0;
                    abilityPressed = false;
                }
                else
                {
                    if (heldTime > 3)
                    {
                        abilityPressed = false;
                        rageUseGauge.value = 0;
                    }
                    else
                        rageUseGauge.value = Mathf.Clamp01(rageToUse / (float)world.player.rage);
                }
            }
        }
        else if (abilityDown || (classType == ClassType.Lancer && abilityPressed))
        {
            world.player.UseAbility(abilityDown);
        }

        if (abilityDown)
            abilityDown = false;
        if (abilityUp)
        {
            abilityPressed = false;
            abilityUp = false;
        }
    }

    private void LateUpdate()
    {
        if (world.player == null) return;

        CheckAbilityInput();

        bool isFull = world.player.cooldown >= world.player.cooldownDuration;

        if (isFull && !full)
        {
            LeanTween.cancel(gameObject);

            var scaleSeq = LeanTween.sequence();
            scaleSeq.append(0.2f);
            scaleSeq.append(LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.1f).setEaseInSine());
            scaleSeq.append(LeanTween.scale(gameObject, Vector3.one, 0.1f).setEaseOutSine());
        }

        full = isFull;

        cooldownBlocker.gameObject.SetActive(!isFull);

        if (isFull || world.player.cooldownDuration == 0) return;
        cooldownBlocker.fillAmount = 1.0f - Mathf.Clamp01((float)world.player.cooldown / world.player.cooldownDuration);
    }
}
