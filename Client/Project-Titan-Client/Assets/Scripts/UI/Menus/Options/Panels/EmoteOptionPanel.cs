using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using UnityEngine;
using UnityEngine.UI;

public class EmoteOptionPanel : OptionPanel
{
    public EmoteType emoteType;

    public Image background;

    public Color selectedColor;

    public Color lockedColor;

    public Color defaultColor;

    public EmoteDial dial;

    public Image emoteImage;

    private int[] Value() => option.GetIntArray();

    protected override void Awake()
    {
        base.Awake();

        option.AddIntArrayCallback(SelectionUpdated);
    }

    private void Start()
    {
        emoteImage.sprite = TextureManager.GetEmoteSprite(emoteType);
    }

    private void OnDestroy()
    {
        option.RemoveIntArrayCallback(SelectionUpdated);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        SelectionUpdated(Value());
    }

    public void OnClick()
    {
        if (!Account.HasUnlockedEmote(emoteType)) return;
        dial.Show((Vector2)background.transform.position + background.rectTransform.rect.size * 0.5f, OnDialSelected);
    }

    public void OnDialSelected(int selectedPosition)
    {
        var selection = Value();
        selection[selectedPosition] = (int)emoteType;
        option.SetIntArray(selection);
    }

    private void SelectionUpdated(int[] value)
    {
        Color color;
        if (!Account.HasUnlockedEmote(emoteType))
        {
            color = lockedColor;
        }
        else if (value.Contains((int)emoteType))
        {
            color = selectedColor;
        }
        else
        {
            color = defaultColor;
        }
        color.a = background.color.a;
        background.color = color;
    }
}