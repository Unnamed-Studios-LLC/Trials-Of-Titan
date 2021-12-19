using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Image buttonImage;

    public string unmutedSprite;

    public string mutedSprite;

    private Option mutedOption;

    private void Awake()
    {
        mutedOption = Options.Get(OptionType.Muted);
        mutedOption.AddBoolCallback(UpdateSprite);
        UpdateSprite(mutedOption.GetBool());
    }

    private void OnDestroy()
    {
        mutedOption.RemoveBoolCallback(UpdateSprite);
    }

    private void UpdateSprite(bool muted)
    {
        buttonImage.sprite = TextureManager.GetUISprite(muted ? mutedSprite : unmutedSprite);
    }

    public void Toggle()
    {
        mutedOption.SetBool(!mutedOption.GetBool());
    }
}
