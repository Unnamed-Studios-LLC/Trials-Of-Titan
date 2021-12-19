using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Map;
using UnityEngine;

public class MusicPlayer : StaticObject
{
    public override GameObjectType ObjectType => GameObjectType.MusicPlayer;

    private Sound sound;

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        var musicPlayerInfo = (MusicPlayerInfo)info;
        AudioManager.TryGetSound(musicPlayerInfo.music, out sound);

        if (sound != null)
        {
            var localPlayer = AudioManager.GetLocalAudioPlayer();
            localPlayer.Play(sound, true);
        }
    }

    public override void Disable()
    {
        base.Disable();

        var localPlayer = AudioManager.GetLocalAudioPlayer();
        var backgroundPlayer = AudioManager.GetBackgroundAudioPlayer();

        backgroundPlayer.volumeDampening = 1;
        localPlayer.volumeDampening = 0;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        var localPlayer = AudioManager.GetLocalAudioPlayer();
        var backgroundPlayer = AudioManager.GetBackgroundAudioPlayer();

        if (sound == null)
        {
            backgroundPlayer.volumeDampening = 1;
            localPlayer.volumeDampening = 0;
            return;
        }

        var player = world.player;
        if (player == null)
        {
            backgroundPlayer.volumeDampening = 1;
            localPlayer.volumeDampening = 0;
            return;
        }

        var radius = ((MusicPlayerInfo)info).musicRadius;
        var distance = (player.Position - Position).magnitude;
        if (distance > radius)
        {
            backgroundPlayer.volumeDampening = 1;
            localPlayer.volumeDampening = 0;
            return;
        }

        float fadeRadius = 5;
        float backgroundMusicVolume = Mathf.Max(((MusicPlayerInfo)info).worldMusicMin, (distance - radius + fadeRadius) / fadeRadius);
        float localVolume = 1.0f - backgroundMusicVolume;

        backgroundPlayer.volumeDampening = backgroundMusicVolume;
        localPlayer.volumeDampening = localVolume;
    }
}
