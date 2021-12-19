using UnityEngine;
using System.Collections;
using TitanCore.Data;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using TitanCore.Core;
using System.Collections.Generic;
using TitanCore.Data.Components;

public abstract class WorldObject : MonoBehaviour
{
    protected abstract bool IsSprite { get; }

    public abstract GameObjectType ObjectType { get; }

    /// <summary>
    /// The game world that this object belongs to
    /// </summary>
    public World world;

    /// <summary>
    /// The info of the game object this represents
    /// </summary>
    public GameObjectInfo info;

    /// <summary>
    /// The id of this object within the game
    /// </summary>
    public uint gameId;

    /// <summary>
    /// The chat bubble above this object, null if none
    /// </summary>
    private ChatBox chatBubble;

    /// <summary>
    /// The ground label for this object, if exists
    /// </summary>
    private GroundLabel groundLabel;

    private Alert playerDamageAlert = null;

    private int playerDamage = 0;

    private Option showChatboxes;

    protected virtual void Awake()
    {
        showChatboxes = Options.Get(OptionType.ShowChatBoxes);
    }

    /// <summary>
    /// Loads a given object info
    /// </summary>
    /// <param name="info"></param>
    public virtual void LoadObjectInfo(GameObjectInfo info)
    {
        this.info = info;
    }

    public virtual void Enable()
    {
        playerDamageAlert = null;
        playerDamage = 0;
    }

    public virtual void Disable()
    {
        if (chatBubble != null)
        {
            world.gameManager.objectManager.ReturnChatBubble(chatBubble);
            chatBubble = null;
        }

        ReturnGroundLabel();
    }

    public virtual void OnDestroy()
    {
        
    }

    public virtual string GetName()
    {
        return info.name;
    }

    public virtual void WorldFixedUpdate(uint time, uint delta)
    {

    }

    protected virtual void Update()
    {

    }

    public virtual float GetVisualSize()
    {
        return transform.localScale.z;
    }

    protected virtual void LateUpdate()
    {
        if (chatBubble != null)
        {
            chatBubble.time += Time.deltaTime;
            if (chatBubble.time > 10)
            {
                world.gameManager.objectManager.ReturnChatBubble(chatBubble);
                chatBubble = null;
            }
        }

        if (playerDamageAlert != null && playerDamageAlert.ownerId != gameId)
        {
            playerDamageAlert = null;
            playerDamage = 0;
        }
    }

    public virtual void NetUpdate(NetStat[] stats, bool first)
    {
        for (int i = 0; i < stats.Length; i++)
        {
            ProcessStat(stats[i], first);
        }
    }

    protected virtual void ProcessStat(NetStat stat, bool first)
    {
        switch (stat.type)
        {
            case ObjectStatType.Position:
                SetPosition((Vec2)stat.value, first);
                break;
            case ObjectStatType.Spawned:
                OnSpawn();
                break;
        }
    }

    protected virtual void OnSpawn()
    {
        PlaySfxType(SfxType.Spawn);
    }

    public virtual void SetPosition(Vec2 position, bool first)
    {
        transform.localPosition = position.ToVector2();
    }

    public void ShowChatBubble(string text)
    {
        if (!showChatboxes.GetBool()) return;
        if (chatBubble == null)
        {
            chatBubble = world.gameManager.objectManager.GetChatBubble(this);
        }

        chatBubble.SetText(text, this);
        chatBubble.time = 0;
    }

    public void ShowAlert(string text, Color color, bool statusEffect = false)
    {
        world.gameManager.objectManager.GetAlert(this, text, color, statusEffect);
    }

    public void ShowPlayerDamageAlert(int amount)
    {
        if (playerDamageAlert == null)
        {
            playerDamageAlert = world.gameManager.objectManager.GetAlert(this, "-" + amount, Color.red, false);
            playerDamage = amount;
        }
        else
        {
            playerDamage += amount;
            playerDamageAlert.UpdateText("-" + playerDamage);
        }
    }

    public bool HasGroundLabel() => groundLabel != null;

    public void ShowGroundLabel(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ReturnGroundLabel();
            return;
        }

        if (groundLabel == null)
            groundLabel = world.gameManager.objectManager.GetGroundLabel(this, text);
        else
            groundLabel.Init(this, text);
    }

    private void ReturnGroundLabel()
    {
        if (groundLabel == null) return;
        world.gameManager.objectManager.ReturnGroundLabel(groundLabel);
        groundLabel = null;
    }

    public void PlayLevelUpEffect()
    {
        var levelup = (LevelUp)world.PlayEffect(EffectType.LevelUp, Vector3.zero);
        levelup.SetFollow(transform);

        ShowAlert("Level Up+", World.soulColor);
    }

    public virtual bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        killed = false;
        return false;
    }

    public virtual void HitBy(AoeProjectile projectile)
    {
    }

    public abstract float GetHeight();

    protected virtual string GetSfxForType(SfxType type)
    {
        if (!info.soundEffects.TryGetValue(type, out var list)) return null;
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)].soundName;
    }

    protected virtual void PlaySfxType(SfxType type)
    {
        var name = GetSfxForType(type);
        if (string.IsNullOrEmpty(name)) return;
        AudioManager.PlaySound(name);
    }
}
