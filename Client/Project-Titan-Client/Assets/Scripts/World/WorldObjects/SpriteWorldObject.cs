using UnityEngine;
using System.Collections;
using TitanCore.Data;
using TitanCore.Data.Components.Textures;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using TitanCore.Core;
using TitanCore.Data.Components;
using System;

public abstract class SpriteWorldObject : WorldObject
{
    protected virtual bool IsBillboard => true;

    protected virtual bool HasShadow => true;

    protected override bool IsSprite => true;

    private static Color noFlash = new Color(0.5f, 0.5f, 0.5f, 0);

    public SpriteRenderer spriteRenderer;

    /// <summary>
    /// The shadow attached to this object
    /// </summary>
    public ObjectShadow shadow;

    private GroundObject groundObject;

    private Vector3 position;
    public Vector3 Position
    {
        get => position;
        set
        {
            value.z = GetZ();
            position = value;
            transform.localPosition = value;
        }
    }

    private float sink = 0;

    private float hover = 0;

    protected new AnimationData animation;

    protected int animationIndex = 0;

    public Color flashColor = noFlash;

    public float alpha = 1;

    public float size = 0;

    private Option worldDrawStyle;

    private Option showEmotes;

    private LTDescr scaleAction;

    private Effect effect;

    protected Indicator indicator;

    private bool spawned = false;

    private float height = 1;

    private GameObjectInfo skinInfo;

    protected bool customMaterial;

    protected override void Awake()
    {
        base.Awake();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        worldDrawStyle = Options.Get(OptionType.WorldDrawStyle);
        showEmotes = Options.Get(OptionType.ShowEmotes);

        indicator = GetComponentInChildren<Indicator>();
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        skinInfo = null;

        SetTexture(info.startTexture);

        if (info.size.min == info.size.max)
            SetSize(info.size.min);
        else
            SetSize(UnityEngine.Random.Range(info.size.min, info.size.max));

        SetGroundObject(info.groundObject);
    }

    private void SetGroundObject(ushort type)
    {
        if (type > 0 &&
            GameData.objects.TryGetValue(type, out var groundInfo) &&
            world.gameManager.objectManager.TryGetObject(groundInfo, out var worldObject))
        {
            if (!(worldObject is GroundObject gO))
            {
                world.gameManager.objectManager.ReturnObject(worldObject);
            }
            else
            {
                groundObject = gO;
            }
        }
        else if (groundObject != null)
        {
            ReturnGroundObject();
        }
    }

    private void ReturnGroundObject()
    {
        if (groundObject != null)
        {
            world.gameManager.objectManager.ReturnObject(groundObject);
            groundObject = null;
        }
    }

    public override void Enable()
    {
        base.Enable();

        Position = default;
        sink = 0;
        hover = 0;
        flashColor = noFlash;
        spawned = false;
        customMaterial = false;

        alpha = 1;
        skinInfo = null;

        if (HasShadow)
        {
            shadow = world.gameManager.objectManager.GetShadow(transform);
            SizeShadow();
        }

        SetOutlineGlow((WorldDrawStyle)worldDrawStyle.GetInt());

        worldDrawStyle.AddIntCallback(DrawOutlineGlowChanged);
    }

    public override void Disable()
    {
        base.Disable();

        if (shadow != null)
        {
            world.gameManager.objectManager.ReturnShadow(shadow);
            shadow = null;
        }

        ReturnGroundObject();

        worldDrawStyle.RemoveIntCallback(DrawOutlineGlowChanged);

        if (scaleAction != null)
        {
            LeanTween.cancel(scaleAction.uniqueId);
            scaleAction = null;
        }

        if (customMaterial)
        {
            Destroy(spriteRenderer.material);
        }

        ClearEffect();

        size = 0;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        worldDrawStyle.RemoveIntCallback(DrawOutlineGlowChanged);
    }

    protected virtual void SetOutlineGlow(WorldDrawStyle style)
    {
        spriteRenderer.material = MaterialManager.GetWorldMaterial(style);
    }

    private void DrawOutlineGlowChanged(int value)
    {
        SetOutlineGlow((WorldDrawStyle)value);
    }

    protected virtual float GetZ()
    {
        return sink - hover;
    }

    private void SizeShadow()
    {
        if (shadow == null) return;

        var sprite = TextureManager.GetDisplaySprite(info);
        float width = GetRelativeScale();
        if (sprite != null)
            width *= (sprite.textureRect.width / sprite.pixelsPerUnit);

        shadow.transform.localScale = new Vector3(width, width, width) * 1.5f * size;
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);
        switch (stat.type)
        {
            case ObjectStatType.Texture:
                SetTexture((int)stat.value);
                break;
            case ObjectStatType.GroundObject:
                SetGroundObject((ushort)stat.value);
                break;
            case ObjectStatType.Hover:
                SetHover((float)stat.value);
                break;
            case ObjectStatType.Size:
                SetSize((float)stat.value);
                break;
            case ObjectStatType.FlashColor:
                flashColor = ((GameColor)stat.value).ToUnityColor();
                break;
            case ObjectStatType.Emote:
                PlayEmote((EmoteType)stat.value);
                break;
        }
    }

    private void PlayEmote(EmoteType emoteType)
    {
        if (!showEmotes.GetBool()) return;
        var emoteEffect = (Emote)world.PlayEffect(EffectType.Emote, Position);
        emoteEffect.SetEmote(emoteType);
    }

    private AnimationData CreateAnimationData(Animation animation)
    {
        switch (animation)
        {
            case CharacterAnimation charAnimation:
                return new CharacterAnimationData(this, charAnimation);
            case EntityAnimation entityAnimation:
                return new EntityAnimationData(this, entityAnimation);
            case SequenceAnimation sequenceAnimation:
                return new SequenceAnimationData(this, sequenceAnimation);
            default:
                return null;
        }
    }


    public override void SetPosition(Vec2 position, bool first)
    {
        //base.SetPosition(position);
        Position = new Vector3(position.x, position.y, this.position.z);
    }

    public void SetSink(float sink)
    {
        this.sink = sink;
        Position = position;
    }

    public void SetHover(float hover)
    {
        this.hover = hover;
        Position = position;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();

        spawned = true;

        SetSize(size);
    }

    public virtual void SetSize(float size)
    {
        var scale = size * GetRelativeScale();

        if (this.size != size || spawned)
            DoSizeAnimation(scale);

        this.size = size;
        SizeShadow();

        SizeIndicator();
    }

    private void SizeIndicator()
    {
        if (indicator == null) return;
        indicator.UpdateSize();
    }

    protected virtual void DoSizeAnimation(float scale)
    {
        if (spawned)
        {
            spawned = false;
            gameObject.LeanCancel();

            transform.localScale = Vector3.zero;
            transform.LeanScale(new Vector3(scale, scale, scale), 0.3f).setEaseOutBack();
        }
        else if (size != 0)
        {
            if (scaleAction != null)
            {
                LeanTween.cancel(scaleAction.uniqueId);
            }
            scaleAction = LeanTween.scale(gameObject, new Vector3(scale, scale, scale), 0.3f);
            scaleAction.setOnComplete(_ =>
            {
                if (scaleAction == _)
                    scaleAction = null;
            }, scaleAction);
        }
        else
            transform.localScale = new Vector3(scale, scale, scale);
    }

    protected virtual float GetRelativeScale() => 1;

    public override float GetVisualSize()
    {
        return size * GetRelativeScale();
    }

    public override float GetHeight()
    {
        //var sprite = spriteRenderer.sprite;
        //if (sprite == null) return 1 * transform.localScale.y;
        //var height = ((sprite.textureRect.height - sprite.pivot.y) / sprite.pixelsPerUnit) * transform.localScale.y;
        return height * transform.localScale.y;
    }

    private void UpdateHeight()
    {
        var sprite = spriteRenderer.sprite;
        if (sprite == null)
            height = 1;
        else
            height = ((sprite.textureRect.height - sprite.pivot.y) / sprite.pixelsPerUnit);
    }

    public void SetSkin(GameObjectInfo info)
    {
        skinInfo = info;
        SetTexture(animationIndex);
    }

    public GameObjectInfo GetSkinInfo()
    {
        return skinInfo ?? info;
    }

    public void SetTexture(int index)
    {
        var info = skinInfo ?? this.info;

        if (index >= info.textures.Length)
        {
            UpdateHeight();
            return;
        }

        animationIndex = index;
        var texture = info.textures[animationIndex];
        if (texture is StillTextureData stillTexture)
        {
            animation = null;
            SetSprite(TextureManager.GetSprite(stillTexture.displaySprite));
        }
        else
        {
            animation = CreateAnimationData(AnimationManager.GetAnimation(texture));
        }

        SetEffect(texture.effect, texture.effectPosition);
        UpdateHeight();
    }

    protected void SetEffect(string effectName, EffectPosition position)
    {
        if (string.IsNullOrEmpty(effectName) || !Enum.TryParse(effectName, out EffectType type))
        {
            ClearEffect();
            return;
        }

        if (effect != null)
        {
            if (effect.type == type) return;
            ClearEffect();
        }

        effect = world.PlayEffect(type, Vector3.zero);
        if (effect == null)
        {
            ClearEffect();
            return;
        }

        effect.transform.SetParent(transform);

        float effectHeight = 0;
        switch (position)
        {
            case EffectPosition.Top:
                effectHeight = GetHeight();
                break;
            case EffectPosition.Middle:
                effectHeight = GetHeight() / 2;
                break;
        }
        effect.transform.localPosition = new Vector3(0, effectHeight, 0);
        effect.transform.localEulerAngles = new Vector3(90, 0, 0);
    }

    protected void ClearEffect()
    {
        if (effect != null)
        {
            effect.transform.SetParent(null);
            effect.system.Stop();
            effect = null;
        }
    }

    public void SetSprite(Sprite sprite, bool flipX = false)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = flipX;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (IsBillboard)
        {
            transform.rotation = Quaternion.LookRotation(world.worldCamera.transform.up, -world.worldCamera.transform.forward);
        }

        animation?.Update(Time.deltaTime);

        var tint = spriteRenderer.color;
        if (flashColor.r != 0 || flashColor.g != 0 || flashColor.b != 0)
            tint = Color.Lerp(noFlash, flashColor, Mathf.Sin(Time.time * 8) / 2 + 0.5f);
        else
            tint = noFlash;
        tint.a = alpha;
        spriteRenderer.color = tint;

        if (groundObject != null)
        {
            var p = Position;
            groundObject.Position = new Vector3(p.x, p.y, 0);
        }
    }
}
