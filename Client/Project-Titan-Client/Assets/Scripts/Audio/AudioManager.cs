using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager
{
    private static AudioManagerInstance instance;

    private static AudioPlayer musicPlayer;

    private static AudioPlayer localMusicPlayer;

    public static void Init(AudioManagerInstance instance)
    {
        AudioManager.instance = instance;
        musicPlayer = instance.GetAudioPlayer();
        musicPlayer.SetAudioType(AudioType.Music);

        localMusicPlayer = instance.GetAudioPlayer();
        localMusicPlayer.SetAudioType(AudioType.Music);

    }

    public static AudioPlayer GetBackgroundAudioPlayer() => musicPlayer;

    public static AudioPlayer GetLocalAudioPlayer() => localMusicPlayer;

    public static void PlaySound(Sound sound)
    {
        if (!instance.TryGetAudioSource(out var player)) return;
        player.Play(sound, false);
    }

    public static void PlaySound(string soundName)
    {
        if (!TryGetSound(soundName, out var sound)) return;
        if (!instance.TryGetAudioSource(out var player)) return;
        player.Play(sound, false);
    }

    public static bool TryGetSound(string name, out Sound sound)
    {
        return instance.TryGetSound(name, out sound);
    }
}
