using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum UISoundEvent
{
    Hover,
    Click
}

public class UISoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public string soundName;

    public UISoundEvent onEvent;

    private Sound sound;

    //private Selectable selectable;

    private void Start()
    {
        //selectable = GetComponent<Selectable>();
        AudioManager.TryGetSound(soundName, out sound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sound == null || !CanPlayEvent(UISoundEvent.Click)) return;
        Play();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_IOS || UNITY_ANDROID
        return;
#else
        if (sound == null || !CanPlayEvent(UISoundEvent.Hover)) return;
        Play();
#endif
    }

    private bool CanPlayEvent(UISoundEvent e)
    {
        return onEvent == e;
    }

    private void Play()
    {
        //if (selectable != null && !selectable.interactable) return;
        AudioManager.PlaySound(sound);
    }
}
