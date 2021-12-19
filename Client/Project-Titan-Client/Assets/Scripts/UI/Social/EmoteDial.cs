using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Geometry;

public class EmoteDial : MonoBehaviour
{
    public Image dialImage;

    public Image[] emoteImages;

    public Sprite normalSprite;

    public Sprite largeSelectedSprite;

    public Sprite smallSelectedSprite;

    private Action<int> callback;

    private Option emoteSelection;

    private Option emoteUse;

    private int touchId = -1;

    private void Awake()
    {
        emoteSelection = Options.Get(OptionType.EmoteDial);
        emoteUse = Options.Get(OptionType.UseEmote);
    }

    public void Show(Vector2 position, Action<int> callback)
    {
        touchId = -1;
        transform.position = position;
        this.callback = callback;
        gameObject.SetActive(true);

        SetSelected(-1);

        UpdateImages(emoteSelection.GetIntArray());
    }

    public void Show(Vector2 position, int touchId, Action<int> callback)
    {
        this.touchId = touchId;
        transform.position = position;
        this.callback = callback;
        gameObject.SetActive(true);

        SetSelected(-1);

        UpdateImages(emoteSelection.GetIntArray());
    }

    private void UpdateImages(int[] selections)
    {
        for (int i = 0; i < emoteImages.Length; i++)
        {
            var image = emoteImages[i];
            var type = (EmoteType)selections[i];
            if (type == EmoteType.None)
            {
                image.sprite = null;
                image.gameObject.SetActive(false);
            }
            else
            {
                image.sprite = TextureManager.GetEmoteSprite(type);
                image.gameObject.SetActive(true);
            }
        }
    }

    private void Hide(int selected)
    {
        callback?.Invoke(selected);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        var position = Input.mousePosition;
        int selected = GetSelected(position);
        if (IsDone())
        {
            Hide(selected);
        }
        else
        {
            SetSelected(selected);
        }
    }

    private Vector3 GetPosition()
    {
        if (touchId == -1)
            return Input.mousePosition;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        return GetTouch().position;
#else
        return Input.mousePosition;
#endif
    }

    private bool IsDone()
    {
        if (touchId == -1)
            return Input.GetMouseButtonUp(0) || Input.GetKeyUp(emoteUse.GetKey());

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        var t = GetTouch();
        return t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended;
#else
        return Input.GetMouseButtonUp(0) || Input.GetKeyUp(emoteUse.GetKey());
#endif
    }

    private Touch GetTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            if (t.fingerId == touchId)
                return t;
        }
        return default;
    }

    private int GetSelected(Vector2 position)
    {
        var vector = position - (Vector2)transform.position;
        var dialRect = dialImage.rectTransform.rect;
        if (vector.magnitude < dialRect.size.x * 0.1f)
            return -1;

        var angle = Mathf.Atan2(vector.y, vector.x);
        angle += AngleUtils.PI / emoteImages.Length;
        angle = AngleUtils.NormalizeRadians(angle);
        return Mathf.Clamp((int)(angle / (AngleUtils.PI_2 / emoteImages.Length)), 0, emoteImages.Length - 1);
    }

    private void SetSelected(int selected)
    {
        if (selected < 0)
        {
            dialImage.sprite = normalSprite;
        }
        else if ((selected % 2) == 0)
        {
            dialImage.sprite = largeSelectedSprite;
            dialImage.rectTransform.localEulerAngles = new Vector3(0, 0, selected * 45);
        }
        else
        {
            dialImage.sprite = smallSelectedSprite;
            dialImage.rectTransform.localEulerAngles = new Vector3(0, 0, (selected - 1) * 45);
        }
    }
}
