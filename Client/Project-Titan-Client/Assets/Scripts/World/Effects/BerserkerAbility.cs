using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BerserkerAbility : Effect
{
    private float endTime;

    private float height = -1;

    public SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();

        //spriteRenderer.sprite = TextureManager.GetSprite(spriteRenderer.sprite.name);
    }

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        height = -1;
        endTime = Time.time + 2f;

        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        var seq = LeanTween.sequence();
        seq.append(transform.LeanScale(new Vector3(1.3f, 1.3f, 1.3f), 0.1f).setEaseOutSine());
        seq.append(transform.LeanScale(new Vector3(0.8f, 0.8f, 0.8f), 0.2f).setEaseInSine());
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        transform.rotation = Quaternion.LookRotation(world.worldCamera.transform.up, -world.worldCamera.transform.forward);
        height -= Time.deltaTime;
        var pos = transform.localPosition;
        transform.localPosition = new Vector3(pos.x, pos.y, height);

        spriteRenderer.color = new Color(1, 1, 1, 1.0f - (((-height) - 1) / 2f));
    }

    protected override bool Running()
    {
        return Time.time < endTime;
    }
}
