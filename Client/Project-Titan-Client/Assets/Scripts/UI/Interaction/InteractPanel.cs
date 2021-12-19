using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractPanel : MonoBehaviour
{
    private const float Button_Size = 0.4833333f;

    public TextMeshProUGUI title;

    public RectTransform button0;
    public TextMeshProUGUI button0Title;

    public RectTransform button1;
    public TextMeshProUGUI button1Title;

    private IInteractable interactable;

    public void SetInteractable(IInteractable interactable)
    {
        if (this.interactable != interactable && interactable != null)
            UpdateLabels(interactable);

        if (this.interactable != interactable)
        {
            if (this.interactable != null)
                this.interactable.OnExit();

            if (interactable != null)
                interactable.OnEnter();
        }

        this.interactable = interactable;

#if UNITY_IOS || UNITY_ANDROID
        if (interactable is MarketDisplay) return;
#endif
        gameObject.SetActive(interactable != null);
    }

    private void UpdateLabels(IInteractable interactable)
    {
        title.text = interactable.InteractionTitle;

        var options = interactable.InteractionOptions;
        button0Title.text = options[0];

        if (options.Length > 1)
        {
            button1Title.text = options[1];
            button1.gameObject.SetActive(true);

            button0.anchorMin = new Vector2(0, button0.anchorMin.y);
            button0.anchorMax = new Vector2(0.48f, button0.anchorMax.y);
            button0.offsetMin = new Vector2(0, 0);
            button0.offsetMax = new Vector2(0, 0);

            button1.anchorMin = new Vector2(0.52f, button1.anchorMin.y);
            button1.anchorMax = new Vector2(1, button1.anchorMax.y);
            button1.offsetMin = new Vector2(0, 0);
            button1.offsetMax = new Vector2(0, 0);
        }
        else
        {
            button1.gameObject.SetActive(false);

            button0.anchorMin = new Vector2(0.15f, button0.anchorMin.y);
            button0.anchorMax = new Vector2(0.85f, button0.anchorMax.y);
            button0.offsetMin = new Vector2(0, 0);
            button0.offsetMax = new Vector2(0, 0);
        }
    }

    public void Button0()
    {
        if (interactable == null) return;
        interactable.Interact(0);
    }

    public void Button1()
    {
        if (interactable == null) return;
        interactable.Interact(1);
    }
}
