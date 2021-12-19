using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AudioManagerInstance : MonoBehaviour
{
    public int maxAudioSources;

    public Sound[] sounds;

    private Dictionary<string, Sound> soundsDict;

    private List<AudioPlayer> inUse = new List<AudioPlayer>();

    private Stack<AudioPlayer> audioPlayers = new Stack<AudioPlayer>();

    private void Awake()
    {
        soundsDict = sounds.ToDictionary(_ => _.clip.name);
        for (int i = 0; i < maxAudioSources; i++)
        {
            var player = CreateAudioPlayer();
            player.Enabled = false;
            audioPlayers.Push(player);
        }
        AudioManager.Init(this);
    }

    private AudioPlayer CreateAudioPlayer()
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0;
        return new AudioPlayer(source);
    }

    public bool TryGetSound(string name, out Sound sound)
    {
        return soundsDict.TryGetValue(name, out sound);
    }

    public bool TryGetAudioSource(out AudioPlayer player)
    {
        if (audioPlayers.Count == 0)
        {
            player = null;
            return false;
        }

        player = audioPlayers.Pop();
        player.Enabled = true;
        inUse.Add(player);
        return true;
    }

    public AudioPlayer GetAudioPlayer()
    {
        if (audioPlayers.Count == 0)
        {
            var source = CreateAudioPlayer();
            inUse.Add(source);
            return source;
        }

        var player = audioPlayers.Pop();
        player.Enabled = true;
        inUse.Add(player);
        return player;
    }

    private void LateUpdate()
    {
        for (int i = 0; i < inUse.Count; i++)
        {
            var audioPlayer = inUse[i];

            audioPlayer.Update();
            if (audioPlayer.ReadyToReturn)
            {
                inUse.RemoveAt(i);
                i--;
                audioPlayer.Enabled = false;
                audioPlayers.Push(audioPlayer);
            }
        }
    }
}