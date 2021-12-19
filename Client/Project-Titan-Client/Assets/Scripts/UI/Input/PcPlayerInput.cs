using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Net.Packets.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class PcPlayerInput : MonoBehaviour, IPointerDownHandler
{
    public World world;

    private RectTransform gameView;

    private bool attacking = false;

    private float lastMoveAngle = 0;

    public ChatInput chat;

    public Minimap minimap;

    public Slider rageUseGauge;

    public EmoteDial emoteDial;

    private float targetAngle = 0;


    private Option moveUp;

    private Option moveDown;

    private Option moveLeft;

    private Option moveRight;

    private Option rotateLeft;

    private Option rotateRight;

    private Option useAbility;

    private Option useEmote;

    private Option emoteSelection;

    private Option escape;

    private Option walkOption;

    private Option resetCameraOption;

    private Option interactOption;

    private Option autofireOption;

    private Option[] useSlots;

    private Option mapScale;

    private float abilityDownTime;

    private bool abilityPressed;

    private bool emotePressed;

    private float emoteDownTime;

    private EmoteType lastEmote = EmoteType.None;

    private bool autofire = false;

    private void Start()
    {
        gameView = GetComponent<RectTransform>();

        moveUp = Options.Get(OptionType.MoveUp);
        moveDown = Options.Get(OptionType.MoveDown);
        moveLeft = Options.Get(OptionType.MoveLeft);
        moveRight = Options.Get(OptionType.MoveRight);

        rotateLeft = Options.Get(OptionType.RotateLeft);
        rotateRight = Options.Get(OptionType.RotateRight);

        useAbility = Options.Get(OptionType.UseAbility);
        useEmote = Options.Get(OptionType.UseEmote);
        emoteSelection = Options.Get(OptionType.EmoteDial);
        escape = Options.Get(OptionType.Escape);
        walkOption = Options.Get(OptionType.Walk);

        resetCameraOption = Options.Get(OptionType.ResetCameraRotation);
        interactOption = Options.Get(OptionType.Interact);
        autofireOption = Options.Get(OptionType.Autofire);

        useSlots = new Option[8];
        for (int i = 0; i < 8; i++)
            useSlots[i] = Options.Get((OptionType)((int)OptionType.UseSlot1 + i));

        mapScale = Options.Get(OptionType.MapScale);
    }

    // Update is called once per frame
    void Update()
    {
        if (!world.allowControls) return;

        CheckMovementInput();
        CheckAttackingInput();
        CheckAbilityInput();
        CheckChatStart();
        CheckMapZoom();
        CheckEmote();
        CheckEscape();
        CheckInteract();
    }

    private bool UsingInput()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }

    private void CheckInteract()
    {
        if (UsingInput()) return;

        if (Input.GetKeyDown(interactOption.GetKey()))
        {
            world.gameManager.ui.interactPanel.Button0();
        }

        for (int i = 0; i < useSlots.Length; i++)
        {
            var option = useSlots[i];
            if (Input.GetKeyDown(option.GetKey()))
            {
                world.gameManager.ui.playerSlots[i + 4].Activate();
            }
        }
    }

    private void CheckChatStart()
    {
        if (UsingInput()) return;
        if (!Input.GetKeyDown(KeyCode.Return)) return;

        chat.StartChat();
    }

    private void CheckMovementInput()
    {
        world.player.SetMove(0, false, false);

        if (UsingInput()) return;

        int xMove = 0, yMove = 0;
        if (Input.GetKey(moveLeft.GetKey()))
        {
            xMove -= 1;
        }
        if (Input.GetKey(moveRight.GetKey()))
        {
            xMove += 1;
        }
        if (Input.GetKey(moveUp.GetKey()))
        {
            yMove += 1;
        }
        if (Input.GetKey(moveDown.GetKey()))
        {
            yMove -= 1;
        }

        if (Input.GetKeyDown(resetCameraOption.GetKey()))
        {
            targetAngle = 0;
            world.CameraRotation = 0;
        }
        else
        {
            int rotation = 0;
            if (Input.GetKey(rotateLeft.GetKey()))
            {
                rotation -= 1;
            }
            if (Input.GetKey(rotateRight.GetKey()))
            {
                rotation += 1;
            }
            RotateCamera(rotation);
        }

        if (xMove == 0 && yMove == 0) return;

        float moveAngle = Mathf.Atan2(yMove, xMove) * Mathf.Rad2Deg;
        moveAngle -= world.CameraRotation;
        moveAngle *= Mathf.Deg2Rad;

        world.player.SetMove(moveAngle, Input.GetKey(walkOption.GetKey()), true);
    }

    private void CheckAbilityInput()
    {
        if (UsingInput()) return;

        bool down = Input.GetKeyDown(useAbility.GetKey());
        bool held = Input.GetKey(useAbility.GetKey());
        bool up = Input.GetKeyUp(useAbility.GetKey());

        var classType = (ClassType)world.player.info.id;
        if (classType == ClassType.Brewer)
        {
            if (down)
            {
                abilityDownTime = Time.time;
                abilityPressed = true;
            }
            else if (abilityPressed)
            {
                if (held)
                {
                    float heldTime = Time.time - abilityDownTime;
                    if (heldTime >= 0.7f)
                    {
                        world.player.UseAbility(true);
                        abilityPressed = false;
                    }
                }
                else if (up)
                {
                    world.player.IncrementAbilityValue();
                    abilityPressed = false;
                }
            }
        }
        else if (classType == ClassType.Bladeweaver)
        {
            if (down)
            {
                abilityDownTime = Time.time;
                abilityPressed = true;
            }
            else if (abilityPressed)
            {
                float heldTime = Time.time - abilityDownTime;
                int rageToUse = Mathf.Min(Mathf.Clamp((int)(heldTime * 100), 0, world.player.rage), AbilityFunctions.BladeWeaver.Max_Dash_Rage);
                if (held)
                {
                    if (heldTime > 3)
                    {
                        abilityPressed = false;
                        rageUseGauge.value = 0;
                    }
                    else
                        rageUseGauge.value = Mathf.Clamp01(rageToUse / (float)world.player.rage);
                }
                else if (up)
                {
                    world.player.SetAbilityValue((byte)rageToUse);
                    world.player.UseAbility(true);

                    rageUseGauge.value = 0;
                    abilityPressed = false;
                }
            }
        }
        else if (down || (classType == ClassType.Lancer && Input.GetKey(useAbility.GetKey())))
        {
            world.player.UseAbility(down);
        }
    }

    private void RotateCamera(int direction)
    {
        if (world.stopTick) return;

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

    private void CheckAttackingInput()
    {
        if (!UsingInput() && Input.GetKeyDown(autofireOption.GetKey()))
        {
            autofire = !autofire;
        }

        if (attacking || autofire)
        {
            if (!Input.GetMouseButton(0) && !autofire)
            {
                attacking = false;
                world.player.SetAttacking(false, 0, GetWorldPositionOfMouse());
            }
            else
            {
                float aimAngle = GetAimAngle();
                world.player.SetAttacking(true, aimAngle, GetWorldPositionOfMouse());
            }
        }
        else
        {
            world.player.SetAttacking(false, 0, GetWorldPositionOfMouse());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            attacking = true;
            float aimAngle = GetAimAngle();
            world.player.SetAttacking(true, aimAngle, GetWorldPositionOfMouse());

            if (!world.player.WeaponEquipped)
                world.GameChat("No weapon equipped!", ChatType.Error);
        }
    }

    private float GetAimAngle()
    {
        Vector3 aimVector = GetCenterOffset();
        float aimAngle = Mathf.Atan2(aimVector.y, aimVector.x) * Mathf.Rad2Deg - world.CameraRotation;
        if (aimAngle < 0)
            aimAngle += 360;
        return aimAngle;
    }

    private Vector2 GetCenterOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 centerPoint = new Vector3(Screen.width - Screen.height * 0.25f, Screen.height) / 2;
        Vector3 aimVector = mousePos - centerPoint;
        return aimVector;
    }

    private Vector2 GetWorldPositionOfMouse()
    {
        var centerOffset = GetCenterOffset();
        var playerPosition = (Vector2)world.player.transform.position;
        var worldScale = (world.worldCamera.orthographicSize * 2) / Screen.height;
        var universalPosition = playerPosition + centerOffset * worldScale;
        return world.transform.InverseTransformPoint(universalPosition);
    }

    public void CheckMapZoom()
    {
        var scroll = Input.mouseScrollDelta;
        if (scroll.y != 0)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                var direction = scroll.y < 0 ? -1 : 1;
                var scale = mapScale.GetFloat();
                scale = Mathf.Clamp(scale + 0.11f * direction, 0.5f, 1.5f);
                scale = Mathf.RoundToInt(scale * 10) / 10.0f;
                mapScale.SetFloat(scale);
            }
            else
            {
                if (scroll.y < 0)
                    minimap.ZoomOut();
                else
                    minimap.ZoomIn();
            }
        }
    }

    private void CheckEmote()
    {
        if (UsingInput()) return;

        var emoteKey = useEmote.GetKey();
        emoteDownTime += Time.deltaTime;
        if (!emotePressed)
        {
            if (Input.GetKeyDown(emoteKey))
            {
                //emotePressed = true;
                emoteDownTime = 0;
                emoteDial.Show(Input.mousePosition, EmoteSelected);
            }
        }

        /*
        if (emotePressed)
        {
            if (Input.GetKeyUp(emoteKey))
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
                emoteDial.Show(Input.mousePosition, EmoteSelected);
            }
        }
        */
    }

    private void EmoteSelected(int selectedEmote)
    {
        var type = lastEmote;
        if (selectedEmote >= 0)
        {
            var selection = emoteSelection.GetIntArray();
            type = (EmoteType)selection[selectedEmote];
        }
        if (type == EmoteType.None) return;
        world.player.UseEmote(type);
        lastEmote = type;
    }

    private void CheckEscape()
    {
        if (UsingInput()) return;

        if (Input.GetKeyDown(escape.GetKey()))
        {
            world.Escape();
        }
    }
}