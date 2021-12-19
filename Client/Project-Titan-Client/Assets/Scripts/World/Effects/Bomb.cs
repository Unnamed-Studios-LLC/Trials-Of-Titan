using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Bomb : Effect
{
    public SpriteRenderer spriteRenderer;

    private Sprite defaultSprite;

    private Vector2 start;

    private Vector2 vector;

    protected float time = 0;

    private float maxTime;

    private Color blastColor;

    protected float blastArea = 0;

    private Option reduceParticles;

    private float rotation;

    private Action<Bomb> callback;

    private string sfx;

    protected bool expired = false;

    protected override void Awake()
    {
        base.Awake();

        defaultSprite = spriteRenderer.sprite;

        reduceParticles = Options.Get(OptionType.ReduceParticles);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        spriteRenderer.enabled = true;
    }

    public override void Init(EffectManager manager, World world)
    {
        base.Init(manager, world);

        var options = system.main;
        options.simulationSpace = ParticleSystemSimulationSpace.Custom;
        options.customSimulationSpace = world.transform;

        var emission = system.emission;
        emission.rateOverTime = 0;//reduceParticles.GetBool() ? 4 : 10;
        emission.rateOverDistance = 4;//reduceParticles.GetBool() ? 4 : 10;

        spriteRenderer.sprite = defaultSprite;
        expired = false;
        sfx = null;
    }

    public void SetInfo(Color color, Vector2 start, Vector2 end, float time)
    {
        var options = system.main;
        options.startColor = color;

        spriteRenderer.color = color;
        transform.localPosition = start;

        this.start = start;
        vector = end - start;

        this.time = 0;
        maxTime = time;
    }

    public void SetHitSfx(string name)
    {
        sfx = name;
    }

    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = Color.white;
    }

    public void AddBlast(Color color, float area)
    {
        blastColor = color;
        blastArea = area;
    }

    public void AddRotation(float rotation)
    {
        this.rotation = rotation;
    }

    public void SetEndCallback(Action<Bomb> callback)
    {
        this.callback = callback;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        DoRotation();

        time += Time.deltaTime;
        if (!expired)
        {
            if (IsExpired())
            {
                Expire();
            }
            else
            {
                float progress = time / maxTime;
                Position(progress);
            }
        }
    }

    protected void DoRotation()
    {
        if (rotation != 0)
        {
            var angles = transform.localEulerAngles;
            angles.z += rotation * Time.deltaTime;
            transform.localEulerAngles = angles;
        }
    }

    protected void Expire()
    {
        expired = true;
        system.Stop();
        spriteRenderer.enabled = false;

        DoBlast();
        callback?.Invoke(this);

        if (!string.IsNullOrWhiteSpace(sfx))
            AudioManager.PlaySound(sfx);
    }

    protected void DoBlast()
    {
        if (blastArea != 0)
        {
            var blast = (AreaBlast)world.PlayEffect(EffectType.AreaBlast, start + vector);
            blast.SetInfo(blastArea, blastColor);
        }
    }

    protected void Position(float progress)
    {
        Vector3 position = start + vector * progress;
        var toSquare = 2 * progress - 1;
        var height = -(toSquare * toSquare - 1);
        position.z = -height * 3;

        transform.localPosition = position;
    }

    protected virtual bool IsExpired()
    {
        return time >= maxTime;
    }
}