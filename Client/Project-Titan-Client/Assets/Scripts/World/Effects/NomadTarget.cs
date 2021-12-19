using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NomadTarget : Effect
{
    private bool running = true;

    public SpriteRenderer spriteRenderer;

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.zero;
        transform.LeanScale(new Vector3(6, 6, 1), 0.3f).setEaseInBack();
        running = true;

        SetAlpha(1);
    }

    public void Consume()
    {
        SetFollow(null);

        LeanTween.value(gameObject, (v) =>
        {
            SetAlpha(v);
        }, 1, 0, 0.3f).setEaseOutSine();
        transform.LeanScale(new Vector3(12, 12, 1), 0.3f).setOnComplete(() =>
        {
            running = false;
        });
    }

    private void SetAlpha(float a)
    {
        var c = spriteRenderer.color;
        c.a = a;
        spriteRenderer.color = c;
    }

    protected override void GotoFollow()
    {
        base.GotoFollow();

        var pos = transform.position;
        pos.z = 0;
        transform.position = pos;
    }

    protected override bool Running()
    {
        return running;
    }
}
