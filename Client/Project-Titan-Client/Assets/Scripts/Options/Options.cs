using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum OptionType
{
    DrawMovementArrow,
    DrawAimingArrow,

    DrawShadows,
    WorldDrawStyle,

    SouthpawJoysticks,

    MoveUp,
    MoveLeft,
    MoveRight,
    MoveDown,
    RotateLeft,
    RotateRight,

    UseAbility,

    ReduceParticles,

    CursorStyle,

    MasterVolume,
    MusicVolume,
    SfxVolume,
    Muted,

    QuestBannerStyle,
    Walk,
    UseEmote,
    EmoteDial,

    Escape,

    MapScale,

    ResetCameraRotation,
    Interact,
    Autofire,

    AllyTransparency,
    AllyProjectiles,

    ShowChatBoxes,
    ShowAllySideChat,
    ShowEmotes,

    AllyPetTransparency,

    Vsync,
    TargetFramerate,

    VaultSlotSize,

    MinimapZoomOut,
    MinimapZoomIn,

    ShowPlayerHealthBar,
    ShowAllyHealthBar,
    ShowPlayerName,

    UseSlot1,
    UseSlot2,
    UseSlot3,
    UseSlot4,
    UseSlot5,
    UseSlot6,
    UseSlot7,
    UseSlot8,

    DrawLeftJoystick,
    DrawRightJoystick,
}

public enum WorldDrawStyle
{
    [Description("Outline Glow")]
    OutlineGlow,
    Outline,
    Glow,
    None
}

public enum TargetFramerate
{
    [Description("Off")]
    Off,
    [Description("30")]
    Thirty,
    [Description("60")]
    Sixty,
    [Description("120")]
    OneTwenty,
    [Description("144")]
    OneFortyFour,
}

public enum VaultSlotSize
{
    Small,
    Medium,
    Large
}

public enum QuestBannerStyle
{
    Detailed,
    Simple
}

public static class Options
{
    private static Dictionary<OptionType, Option> options;

    static Options()
    {
        options = new Dictionary<OptionType, Option>();
        foreach (var optionType in (OptionType[])Enum.GetValues(typeof(OptionType)))
        {
            options.Add(optionType, new Option(optionType));
        }
    }

    public static Option Get(OptionType type)
    {
        return options[type];
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static object GetDefault(OptionType type)
    {
        bool mobile = Application.isMobilePlatform;
        switch (type)
        {
            case OptionType.DrawMovementArrow:
            case OptionType.DrawAimingArrow:
            case OptionType.DrawLeftJoystick:
            case OptionType.DrawRightJoystick:
            case OptionType.DrawShadows:
                return true;
            case OptionType.ReduceParticles:
            case OptionType.SouthpawJoysticks:
                return false;
            case OptionType.WorldDrawStyle:
                return WorldDrawStyle.OutlineGlow;

            case OptionType.MoveUp:
                return KeyCode.W;
            case OptionType.MoveRight:
                return KeyCode.D;
            case OptionType.MoveLeft:
                return KeyCode.A;
            case OptionType.MoveDown:
                return KeyCode.S;

            case OptionType.Walk:
                return KeyCode.LeftShift;

            case OptionType.RotateLeft:
                return KeyCode.Q;
            case OptionType.RotateRight:
                return KeyCode.E;

            case OptionType.UseEmote:
                return KeyCode.F;

            case OptionType.Escape:
                return KeyCode.R;

            case OptionType.ResetCameraRotation:
                return KeyCode.Z;

            case OptionType.Interact:
                return KeyCode.G;
            case OptionType.Autofire:
                return KeyCode.I;

            case OptionType.MinimapZoomIn:
                return KeyCode.Equals;
            case OptionType.MinimapZoomOut:
                return KeyCode.Minus;

            case OptionType.UseAbility:
                return KeyCode.Space;

            case OptionType.CursorStyle:
                return "Cursor2";

            case OptionType.MasterVolume:
                return 50f;
            case OptionType.MusicVolume:
            case OptionType.SfxVolume:
                return 100f;
            case OptionType.Muted:
                return false;

            case OptionType.ShowPlayerHealthBar:
                return false;
            case OptionType.ShowAllyHealthBar:
                return false;
            case OptionType.ShowPlayerName:
                return false;

            case OptionType.QuestBannerStyle:
                return mobile ? QuestBannerStyle.Simple : QuestBannerStyle.Detailed;

            case OptionType.EmoteDial:
                return "0,0,0,0,0,0,0,0";

            case OptionType.AllyTransparency:
                return 10f;
            case OptionType.AllyProjectiles:
                return true;
            case OptionType.ShowChatBoxes:
                return true;
            case OptionType.ShowAllySideChat:
                return true;
            case OptionType.ShowEmotes:
                return true;
            case OptionType.AllyPetTransparency:
                return 10f;

            case OptionType.Vsync:
                return true;
            case OptionType.TargetFramerate:
                return TargetFramerate.Off;

            case OptionType.VaultSlotSize:
                return VaultSlotSize.Medium;

            case OptionType.UseSlot1:
                return KeyCode.Alpha1;
            case OptionType.UseSlot2:
                return KeyCode.Alpha2;
            case OptionType.UseSlot3:
                return KeyCode.Alpha3;
            case OptionType.UseSlot4:
                return KeyCode.Alpha4;
            case OptionType.UseSlot5:
                return KeyCode.Alpha5;
            case OptionType.UseSlot6:
                return KeyCode.Alpha6;
            case OptionType.UseSlot7:
                return KeyCode.Alpha7;
            case OptionType.UseSlot8:
                return KeyCode.Alpha8;
            case OptionType.MapScale:
                return 1.0f;

            default:
                return null;
        }
    }
}
