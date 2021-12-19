using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum AudioType
{
    Music,
    Sfx
}

public class AudioPlayer
{
    private enum PlayState
    {
        NotStarted,
        Playing,
        FadeOut
    }

    private struct QueuedSound
    {
        public Sound sound;

        public bool loops;

        public bool fadeTransition;
    }

    private const float Fade_Out_Duration = 1.5f;

    private const float Delay_Duration = 1.5f;

    public bool ReadyToReturn => !source.isPlaying && playState == PlayState.Playing;

    public bool IsPlaying => source.isPlaying;

    public float volumeDampening = 1;

    private PlayState playState = PlayState.NotStarted;

    private AudioType audioType = AudioType.Sfx;

    private float fadeTime;

    private Sound sound;

    private AudioSource source;

    private Option masterVolume;

    private Option volumeOption;

    private Option muted;

    private Queue<QueuedSound> soundQueue = new Queue<QueuedSound>();

    public bool Enabled
    {
        get => source.enabled;
        set => source.enabled = value;
    }

    public AudioPlayer(AudioSource source)
    {
        this.source = source;

        masterVolume = Options.Get(OptionType.MasterVolume);
        volumeOption = Options.Get(OptionType.SfxVolume);
        muted = Options.Get(OptionType.Muted);
    }

    private float GetVolumeSetting()
    {
        return (muted.GetBool() ? 0 : (masterVolume.GetFloat() / 100f) * (volumeOption.GetFloat() / 100f)) * volumeDampening;
    }

    public void Enqueue(Sound sound, bool loops, bool fadeTransition)
    {
        soundQueue.Enqueue(new QueuedSound
        {
            sound = sound,
            loops = loops,
            fadeTransition = fadeTransition
        });
    }

    public void SetAudioType(AudioType audioType)
    {
        this.audioType = audioType;
        switch (audioType)
        {
            case AudioType.Sfx:
                volumeOption = Options.Get(OptionType.SfxVolume);
                break;
            case AudioType.Music:
                volumeOption = Options.Get(OptionType.MusicVolume);
                break;
        }
    }

    private void PlayNext()
    {
        if (soundQueue.Count == 0) return;
        var next = soundQueue.Dequeue();

        Play(next.sound, next.loops);
    }

    public void Play(Sound sound, bool looping)
    {
        source.Stop();

        this.sound = sound;
        source.clip = sound.clip;
        source.loop = looping;
        source.Play();

        playState = PlayState.Playing;
    }

    public void Stop()
    {
        if (!source.isPlaying || playState == PlayState.NotStarted) return;

        source.Stop();
        playState = PlayState.NotStarted;
        sound = null;
        source.clip = null;
    }

    public void Pause()
    {

    }

    public void Update()
    {
        switch (playState)
        {
            case PlayState.FadeOut:
                fadeTime += Time.deltaTime;
                if (fadeTime >= Fade_Out_Duration + Delay_Duration)
                {
                    PlayNext();
                }
                break;
            case PlayState.Playing:
                if (!source.isPlaying)
                    Next();
                break;
        }

        UpdateVolume();
    }

    private void UpdateVolume()
    {
        if (sound == null)
        {
            source.volume = 0;
            return;
        }

        float volume = GetVolumeSetting();
        switch (playState)
        {
            case PlayState.FadeOut:
                volume *= LeanTween.easeOutSine(0, 1, 1f - (fadeTime / Fade_Out_Duration));
                break;
        }

        source.volume = volume * sound.volume;
    }

    public void Next()
    {
        if (soundQueue.Count == 0) return;
        var next = soundQueue.Peek();

        if (next.fadeTransition && source.isPlaying)
            StartFadeOut();
        else
            PlayNext();
    }

    public void ClearQueue()
    {
        soundQueue.Clear();
    }

    private void StartFadeOut()
    {
        playState = PlayState.FadeOut;
        fadeTime = 0;
    }
}