using System.Collections;
using TitanCore.Data;
using TitanCore.Data.Components.Textures;
using UnityEngine;
using TitanCore.Core;
using TitanCore.Net.Packets.Models;
using Utils.NET.Geometry;
using TitanCore.Net.Packets.Client;
using System.Collections.Generic;
using TitanCore.Data.Items;
using TitanCore.Data.Components.Projectiles;
using System.Linq;

public class Character : Entity
{
    private static Color soulGainedColor = new Color(0.4156863f, 0.9254903f, 0.882353f, 1f);

    public override GameObjectType ObjectType => GameObjectType.Character;

    protected override bool Ally => true;

    private Vector3 lastPosition;

    private float lastDirectionAngle = 0;

    protected int shootCooldown = 0;

    protected float attackAngle = 0;

    public long characterId = 0;

    protected Item[] items;

    public string playerName = "";

    public int serverHealth;

    public int maxHealth;

    public int defense;

    public int speed;

    public int attack;

    public int vigor;

    public int maxHealthBonus;

    public int defenseBonus;

    public int speedBonus;

    public int attackBonus;

    public int vigorBonus;

    public int fullSouls;

    public byte classQuests = 0;

    public Rank rank = Rank.Player;

    private Dictionary<StatType, int> statIncreases = new Dictionary<StatType, int>();

    private Dictionary<AlternateStatType, int> alternateStatIncreases = new Dictionary<AlternateStatType, int>();

    private Dictionary<StatusEffect, uint> clientEffects = new Dictionary<StatusEffect, uint>();

    private Option allyTransparency;

    private Option allyProjectiles;

    private Option showHealthBar;

    private WorldHealthBar healthBar;

    protected override void Awake()
    {
        base.Awake();

        allyTransparency = Options.Get(OptionType.AllyTransparency);
        allyProjectiles = Options.Get(OptionType.AllyProjectiles);
        showHealthBar = Options.Get(this is Player ? OptionType.ShowPlayerHealthBar : OptionType.ShowAllyHealthBar);
    }

    public override void Enable()
    {
        base.Enable();

        lastPosition = default;
        characterId = 0;
        lastDirectionAngle = 0;
        shootCooldown = 0;
        attackAngle = 0;
        playerName = "";
        maxHealth = 0;
        serverHealth = 0;
        speed = 0;
        attack = 0;
        defense = 0;
        vigor = 0;
        maxHealthBonus = 0;
        speedBonus = 0;
        attackBonus = 0;
        defenseBonus = 0;
        vigorBonus = 0;
        fullSouls = 0;
        classQuests = 0;
        rank = Rank.Player;
        statIncreases.Clear();
        alternateStatIncreases.Clear();
        clientEffects.Clear();

        OnAllyTransparency(allyTransparency.GetFloat());
        allyTransparency.AddFloatCallback(OnAllyTransparency);

        healthBar = world.gameManager.objectManager.GetHealthBar(this);

        OnShowHealthBar(showHealthBar.GetBool());
        showHealthBar.AddBoolCallback(OnShowHealthBar);
    }

    public override void Disable()
    {
        base.Disable();

        allyTransparency.RemoveFloatCallback(OnAllyTransparency);
        showHealthBar.RemoveBoolCallback(OnShowHealthBar);

        if (healthBar != null)
        {
            world.gameManager.objectManager.ReturnHealthBar(healthBar);
            healthBar = null;
        }
    }

    protected virtual void OnAllyTransparency(float value)
    {
        alpha = value / 10f;
    }

    protected virtual void OnShowHealthBar(bool value)
    {
        if (healthBar == null) return;
        healthBar.gameObject.SetActive(value);
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);

        items = CreateItems();
    }

    protected override float GetRelativeScale()
    {
        return 0.8f;
    }

    public override string GetName()
    {
        return playerName;
    }

    protected virtual Item[] CreateItems()
    {
        return new Item[4];
    }

    public override Item GetItem(int index)
    {
        if (index < 4)
            return items[index];
        return base.GetItem(index);
    }

    public override bool IsInvincible()
    {
        return base.IsInvincible() || HasClientEffect(StatusEffect.Dashing);
    }

    public override void SetItem(int index, Item item)
    {
        if (index < 4)
        {
            var lastItem = GetItem(index);
            if (!lastItem.IsBlank) // remove the old item's stat boosts
            {
                var lastItemInfo = lastItem.GetInfo();
                if (lastItemInfo is EquipmentInfo equipInfo && equipInfo.statIncreases.Count != 0)
                {
                    foreach (var increase in equipInfo.statIncreases)
                    {
                        var increaseAmount = statIncreases[increase.Key];
                        increaseAmount -= increase.Value;
                        if (increaseAmount == 0)
                            statIncreases.Remove(increase.Key);
                        else
                            statIncreases[increase.Key] = increaseAmount;
                    }

                    foreach (var increase in equipInfo.alternateStatIncreases)
                    {
                        var increaseAmount = alternateStatIncreases[increase.Key];
                        increaseAmount -= increase.Value;
                        if (increaseAmount == 0)
                            alternateStatIncreases.Remove(increase.Key);
                        else
                            alternateStatIncreases[increase.Key] = increaseAmount;
                    }
                }
            }

            if (!item.IsBlank) // add the new item's stat boosts
            {
                var newItemInfo = item.GetInfo();
                if (newItemInfo is EquipmentInfo equipInfo && equipInfo.statIncreases.Count != 0)
                {
                    foreach (var increase in equipInfo.statIncreases)
                    {
                        if (!statIncreases.TryGetValue(increase.Key, out var increaseAmount))
                            increaseAmount = 0;
                        increaseAmount += increase.Value;
                        statIncreases[increase.Key] = increaseAmount;
                    }

                    foreach (var increase in equipInfo.alternateStatIncreases)
                    {
                        if (!alternateStatIncreases.TryGetValue(increase.Key, out var increaseAmount))
                            increaseAmount = 0;
                        increaseAmount += increase.Value;
                        alternateStatIncreases[increase.Key] = increaseAmount;
                    }
                }
            }
            items[index] = item;
        }
    }

    public override SlotType GetSlotType(int index)
    {
        if (index < 4)
        {
            var charInfo = (TitanCore.Data.Entities.CharacterInfo)info;
            return charInfo.equipSlots[index];
        }
        return base.GetSlotType(index);
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);

        switch (stat.type)
        {
            case ObjectStatType.Name:
                playerName = (string)stat.value;
                UpdateNameLabel();
                break;
            case ObjectStatType.MaxHealth:
                maxHealth = (int)stat.value;
                break;
            case ObjectStatType.Health:
                serverHealth = (int)stat.value;
                break;
            case ObjectStatType.Defense:
                defense = (int)stat.value;
                break;
            case ObjectStatType.Speed:
                speed = (int)stat.value;
                break;
            case ObjectStatType.Attack:
                attack = (int)stat.value;
                break;
            case ObjectStatType.Vigor:
                vigor = (int)stat.value;
                break;
            case ObjectStatType.MaxHealthBonus:
                maxHealthBonus = (int)stat.value;
                break;
            case ObjectStatType.DefenseBonus:
                defenseBonus = (int)stat.value;
                break;
            case ObjectStatType.SpeedBonus:
                speedBonus = (int)stat.value;
                break;
            case ObjectStatType.AttackBonus:
                attackBonus = (int)stat.value;
                break;
            case ObjectStatType.VigorBonus:
                vigorBonus = (int)stat.value;
                break;
            case ObjectStatType.Heal:
                if (this is Player) break;
                var healAmount = (int)stat.value;
                var realizedHeal = (int)Mathf.Min(healAmount, GetStatFunctional(StatType.MaxHealth) - serverHealth);
                if (realizedHeal <= 0) break;
                ShowAlert("+" + realizedHeal, Color.green);
                break;
            case ObjectStatType.Souls:
                int old = fullSouls;
                fullSouls = (int)stat.value;
                if (!first && (fullSouls - old > 0))
                {
                    var dif = fullSouls - old;
                    if (dif < 0)
                    {

                        ShowAlert("--" + dif, World.soulColor, true);
                    }
                    else if (dif > 0)
                    {
                        var effect = world.PlayEffect(EffectType.SoulsGained, Position);
                        effect.SetFollow(transform);
                        ShowAlert("+" + dif, soulGainedColor);
                    }
                }
                break;
            case ObjectStatType.HitDamage:
                if (this is Player) break;
                ShowAlert("-" + (int)stat.value, Color.red);
                break;
            case ObjectStatType.ClassQuest:
                classQuests = (byte)stat.value;
                UpdateNameLabel();
                break;
            case ObjectStatType.Rank:
                rank = (Rank)((byte)stat.value);
                if (rank == Rank.Admin)
                {
                    /*
                    var material = new Material(spriteRenderer.sharedMaterial);
                    material.SetColor("_GlowColor", Color.cyan);
                    material.SetColor("_OutlineColor", Color.cyan);
                    spriteRenderer.material = material;
                    customMaterial = true;
                    */
                    UpdateNameLabel();
                }
                break;
            case ObjectStatType.Skin:
                if (GameData.objects.TryGetValue((ushort)stat.value, out var skin))
                    SetSkin(skin);
                else
                    SetSkin(null);
                break;
        }
    }

    protected virtual void UpdateNameLabel()
    {
        if (string.IsNullOrEmpty(playerName)) return;

        if (rank == Rank.Admin)
            ShowGroundLabel("<sprite name=\"AdminCrown\"> " + Constants.GetClassQuestString(classQuests) + playerName);
        else
            ShowGroundLabel(Constants.GetClassQuestString(classQuests) + playerName);
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        shootCooldown -= (int)delta;
    }

    protected override void LateUpdate()
    {
        if (world.stopTick) return;

        if (!(this is Player))
            UpdateCharacterAnimation();

        base.LateUpdate();

        flashColor = serverHealth < (GetStatFunctional(StatType.MaxHealth)) * 0.2f ? new Color(0.5f, 0.1f, 0.1f, 1) : Color.clear;
    }

    protected virtual bool IsAttacking => shootCooldown > 0;

    protected void UpdateCharacterAnimation()
    {
        var state = AnimationState.Still;
        var direction = lastDirectionAngle;
        var position = Position;

        if (IsAttacking)
        {
            state = AnimationState.Attack;
            direction = attackAngle;
        }
        else if (position != lastPosition)
        {
            var movementVector = position - lastPosition;
            state = AnimationState.Walk;
            direction = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
            lastPosition = position;
        }

        lastDirectionAngle = direction;

        if (animation is CharacterAnimationData characterAnimation)
            characterAnimation.SetState(state, GetCharacterAnimationDirection(direction), shootCooldown / 1000f);
        else if (animation != null)
            animation.SetState(state, GetAnimationDirection(direction), shootCooldown / 1000f);
    }

    private AnimationDirection GetCharacterAnimationDirection(float angle)
    {
        angle += world.CameraRotation;
        angle = angle + Mathf.Ceil(-angle / 360) * 360;

        if (angle > 50 && angle < 130)
        {
            return AnimationDirection.Up;
        }
        else if (angle >= 130 && angle <= 230)
        {
            return AnimationDirection.Left;
        }
        else if (angle > 230 && angle < 310)
        {
            return AnimationDirection.Down;
        }
        else
        {
            return AnimationDirection.Right;
        }
    }

    private AnimationDirection GetAnimationDirection(float angle)
    {
        angle += world.CameraRotation;
        angle = angle + Mathf.Ceil(-angle / 360) * 360;

        if (angle >= 89 && angle < 269)
        {
            return AnimationDirection.Left;
        }
        else
        {
            return AnimationDirection.Right;
        }
    }

    public override bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        bool hit = base.IsHitBy(position, projectile, out killed);
        if (this is Player) return hit;

        if (hit)
        {
            world.gameManager.client.SendAsync(new TnAllyHit(world.clientTickId, projectile.projId, gameId));

            if (!HasStatusEffect(StatusEffect.Invulnerable))
            {
                var damageTaken = GetDamageTaken(projectile.damage);
                ShowAlert("-" + damageTaken, Color.red);
            }
        }
        return hit;
    }

    public override void HitBy(AoeProjectile projectile)
    {
        base.HitBy(projectile);
        if (this is Player || IsInvincible()) return;

        if (!HasStatusEffect(StatusEffect.Invulnerable))
        {
            var damageTaken = GetDamageTaken(projectile.damage);
            ShowAlert("-" + damageTaken, Color.red);
        }
    }

    public override int GetDamageTaken(int damage)
    {
        return StatFunctions.DamageTaken(GetStatFunctional(StatType.Defense), damage, HasStatusEffect(StatusEffect.Fortified));
    }

    public int GetStatFunctional(StatType type)
    {
        return GetStatBase(type) + GetStatBonus(type) + GetStatIncrease(type);
    }

    public int GetStatBase(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth:
                return maxHealth;
            case StatType.Speed:
                return speed;
            case StatType.Attack:
                return attack;
            case StatType.Defense:
                return defense;
            case StatType.Vigor:
                return vigor;
        }
        return 0;
    }

    public int GetStatBonus(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth:
                return maxHealthBonus;
            case StatType.Speed:
                return speedBonus;
            case StatType.Attack:
                return attackBonus;
            case StatType.Defense:
                return defenseBonus;
            case StatType.Vigor:
                return vigorBonus;
        }
        return 0;
    }

    public int GetStatIncrease(StatType type)
    {
        if (!statIncreases.TryGetValue(type, out var amount))
            return 0;
        return amount;
    }

    public int GetAlternateStatIncrease(AlternateStatType type)
    {
        if (!alternateStatIncreases.TryGetValue(type, out var amount))
            return 0;
        return amount;
    }

    protected override float GetZ()
    {
        float z = base.GetZ();
        if (HasStatusEffect(StatusEffect.KnockedBack))
            z -= 0.2f;
        if (HasStatusEffect(StatusEffect.Grounded))
            z += 0.25f;
        return z;
    }

    public void Shoot(AllyAoeProjectile allyProj, uint time)
    {
        if (((Vector2)(world.player.Position - Position)).magnitude > 20 || !allyProjectiles.GetBool()) return;
        var item = GameData.objects[allyProj.item];
        if (!(item is WeaponInfo weapon)) return;

        var projData = weapon.projectiles[allyProj.projectileId % weapon.projectiles.Length];
        var aoeData = (AoeProjectileData)projData;
        var aoe = (ItemAoeProjectile)world.PlayEffect(EffectType.ItemAoeProjectile, Position);
        aoe.Setup(world.enemies, time, aoeData, Position, allyProj.target.ToVector2(), allyProj.damage, allyProj.projectileId, true, weapon);
        world.AddAoeProjectile(aoe);

        shootCooldown = (int)((1000 / weapon.rateOfFire) * 1.1f);
        if (animation is CharacterAnimationData characterAnimation)
            characterAnimation.attackFps = (shootCooldown / 2) / 1000.0f;

        attackAngle = ((Vector2)Position).ToVec2().AngleTo(allyProj.target) * Mathf.Rad2Deg;
    }

    public void Shoot(AllyProjectile allyProj, uint time)
    {
        if (((Vector2)(world.player.Position - Position)).magnitude > 20 || !allyProjectiles.GetBool()) return;
        var item = GameData.objects[allyProj.item];
        if (!(item is WeaponInfo weapon)) return;

        var projData = weapon.projectiles[allyProj.projectileId % weapon.projectiles.Length];

        var proj = world.gameManager.objectManager.GetProjectile();
        proj.Setup(world.hittables, Position, projData, allyProj.angle, allyProj.projectileId, time, allyProj.damage, allyProj.reach);

        shootCooldown = (int)((1000 / weapon.rateOfFire) * 1.1f);
        if (animation is CharacterAnimationData characterAnimation)
            characterAnimation.attackFps = (shootCooldown / 2) / 1000.0f;
        attackAngle = allyProj.angle * Mathf.Rad2Deg;
    }

    public int GetLevel()
    {
        return GetStatBase(StatType.Speed) + 
            GetStatBase(StatType.Attack) +
            GetStatBase(StatType.Defense) +
            GetStatBase(StatType.Vigor) +
            GetStatBase(StatType.MaxHealth) / 10;
    }

    public override bool HasStatusEffect(StatusEffect effect)
    {
        return base.HasStatusEffect(effect) || HasClientEffect(effect);
    }

    protected bool HasClientEffect(StatusEffect effect, uint worldTime = 0)
    {
        if (!clientEffects.TryGetValue(effect, out var time))
            return false;
        return time > (worldTime != 0 ? worldTime : world.clientTime);
    }

    public void AddClientEffect(StatusEffect effect, uint duration)
    {
        uint newEndTime = world.clientTime + duration;
        if (clientEffects.TryGetValue(effect, out var endTime))
        {
            if (newEndTime < endTime) return;
            clientEffects[effect] = newEndTime;
        }
        else
            clientEffects.Add(effect, newEndTime);

        if (effect == StatusEffect.Dashing)
        {
            var dashEffect = (Dash)world.PlayEffect(EffectType.Dash, Position);
            dashEffect.SetInfo(this);
        }
    }
}
