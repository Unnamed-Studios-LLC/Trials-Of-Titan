using System.Collections;
using System.Collections.Generic;
using TitanCore.Data;
using UnityEngine;
using UnityEngine.UI;

public class ClassPreview : MonoBehaviour
{
    public Image image;

    public AnimationDirection direction = AnimationDirection.Down;

    public AnimationState state = AnimationState.All;

    public bool animated = false;

    public ushort defaultClass = 0;

    public RectTransform rectTransform;

    private Animation currentAnimation;

    private float frameTime = 0.25f;

    private int currentFrame = 0;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (defaultClass != 0)
        {
            SetClass(GameData.objects[defaultClass]);
        }
    }

    public void SetClass(GameObjectInfo info)
    {
        currentAnimation = AnimationManager.GetAnimation(info.textures[0]);
        ResetFrame();
    }

    public void ResetFrame()
    {
        currentFrame = 0;
        frameTime = 0.25f;

        UpdateFrame();
    }

    private void LateUpdate()
    {
        if (!animated) return;

        frameTime -= Time.deltaTime;
        if (frameTime <= 0)
        {
            frameTime = 0.25f;
            currentFrame++;
            UpdateFrame();
        }
    }

    private void UpdateFrame()
    {
        var frames = currentAnimation.GetFrames(AnimationState.All, direction);
        currentFrame = currentFrame % frames.Length;
        var sprite = frames[currentFrame];

        SetImage(sprite);
    }

    private void SetImage(Sprite sprite)
    {
        var aspect = sprite.textureRect.width / sprite.textureRect.height;
        var rect = rectTransform.rect;
        var scale = rect.height / sprite.textureRect.height;
        image.rectTransform.sizeDelta = new Vector2(rect.height * aspect, rect.height);
        image.rectTransform.anchoredPosition = new Vector2(-sprite.pivot.x * scale, 0);

        image.sprite = sprite;
    }
}
