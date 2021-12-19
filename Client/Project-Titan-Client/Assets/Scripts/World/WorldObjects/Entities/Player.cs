using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Components;
using TitanCore.Data.Components.Projectiles;
using TitanCore.Data.Items;
using TitanCore.Data.Map;
using TitanCore.Net;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using UnityEngine;
using Utils.NET.Algorithms;
using Utils.NET.Geometry;
using Utils.NET.Logging;

public class Player : Character
{

    private static Color soulProgressGainedColor = new Color(0.2196079f, 0.8313726f, 0.8313726f, 1f);

    public override GameObjectType ObjectType => GameObjectType.Player;

    public bool attacking = false;

    private uint projIds = 0;

    public float health;

    public int cooldown = 1;

    public int cooldownDuration = 1;

    private bool wantsToUseAbility = false;

    private TileInfo currentTile;

    public Vector2 aimPosition;

    public Vector2 abilityAimPosition;

    public int rage = 0;

    public byte abilityValue = 0;

    private Vec2 positionalEffectVector;

    private bool positionalEffectCollided = false;

    private DateTime lastEmote;

    public Item[] backpack;

    public Vec2 lastSentPosition;

    public uint lastSentPositionTime;

    private Option showPlayerName;

    public uint target;

    private NomadTarget targetEffect;

    private Vector3 lastFixedMove;

    private float moveAngle = 0;

    private bool walk = false;

    private bool moving = false;

    public Vector3 targetUpdatePosition;

    private uint lastMovementTime;

    private float targetUpdateTime;

    public int lockedMaxHealth;

    public int lockedSpeed;

    public int lockedAttack;

    public int lockedDefense;

    public int lockedVigor;

    public int soulGoal;


    protected override bool IsAttacking => attacking && WeaponEquipped;

    public bool WeaponEquipped => !items[0].IsBlank && items[0].GetInfo() is WeaponInfo;

    protected override void Awake()
    {
        base.Awake();

        showPlayerName = Options.Get(OptionType.ShowPlayerName);
    }

    public override void Enable()
    {
        base.Enable();

        attacking = false;
        projIds = 0;
        fullSouls = 0;
        health = 0;
        currentTile = null;
        rage = 0;
        abilityValue = 0;
        world.gameManager.ui.OnSoulsUpdated(fullSouls, soulGoal);
        target = 0;
        moveAngle = 0;
        walk = false;
        moving = false;

        lockedMaxHealth = 0;
        lockedSpeed = 0;
        lockedAttack = 0;
        lockedDefense = 0;
        lockedVigor = 0;

        soulGoal = 0;

        cooldown = 1;
        cooldownDuration = 1;
        backpack = new Item[8];

        wantsToUseAbility = false;
        currentTile = null;

        OnShowPlayerName(showPlayerName.GetBool());
        showPlayerName.AddBoolCallback(OnShowPlayerName);
    }

    public override void Disable()
    {
        base.Disable();

        ReturnTarget();

        showPlayerName.RemoveBoolCallback(OnShowPlayerName);
    }

    private void OnShowPlayerName(bool value)
    {
        UpdateNameLabel();
    }

    protected override void OnAllyTransparency(float value)
    {
        
    }

    protected override void UpdateNameLabel()
    {
        if (!showPlayerName.GetBool())
        {
            ShowGroundLabel(null);
            return;
        }

        base.UpdateNameLabel();
    }

    public override void LoadObjectInfo(GameObjectInfo info)
    {
        base.LoadObjectInfo(info);
    }

    protected override Item[] CreateItems()
    {
        return new Item[12];
    }

    public override void NetUpdate(NetStat[] stats, bool first)
    {
        base.NetUpdate(stats, first);

        health = Mathf.Min(health, GetStatFunctional(StatType.MaxHealth));
        world.gameManager.ui.OnPlayerStatsUpdated(this);
        world.gameManager.ui.SetHealthValue((int)health);
    }

    protected override void ProcessStat(NetStat stat, bool first)
    {
        base.ProcessStat(stat, first);
        switch (stat.type)
        {
            case ObjectStatType.Health:
                if (first)
                {
                    health = (int)stat.value;
                }
                break;
            case ObjectStatType.Souls:
                world.gameManager.ui.OnSoulsUpdated(fullSouls, soulGoal);
                break;
            case ObjectStatType.SoulGoal:
                soulGoal = (int)stat.value;
                world.gameManager.ui.OnSoulsUpdated(fullSouls, soulGoal);
                break;
            case ObjectStatType.PremiumCurrency:
                world.gameManager.ui.SetPremiumCurrency((long)stat.value);
                break;
            case ObjectStatType.DeathCurrency:
                world.gameManager.ui.SetDeathCurrency((long)stat.value);
                break;
            case ObjectStatType.Rage:
                rage = (byte)stat.value;
                break;
            case ObjectStatType.Heal:
                var healAmount = (int)stat.value;
                var realizedHeal = (int)Mathf.Min(healAmount, GetStatFunctional(StatType.MaxHealth) - health);
                health += healAmount;
                if (realizedHeal <= 0) break;
                ShowAlert("+" + realizedHeal, Color.green);
                break;
            case ObjectStatType.Backpack0:
            case ObjectStatType.Backpack1:
            case ObjectStatType.Backpack2:
            case ObjectStatType.Backpack3:
            case ObjectStatType.Backpack4:
            case ObjectStatType.Backpack5:
            case ObjectStatType.Backpack6:
            case ObjectStatType.Backpack7:
                backpack[(int)stat.type - (int)ObjectStatType.Backpack0] = (Item)stat.value;
                break;
            case ObjectStatType.Target:
                ReturnTarget();
                target = (uint)stat.value;
                if (target == 0)
                {
                    ReturnTarget();
                    break;
                }
                if (world.TryGetObject(target, out var targetObj))
                    UpdateTarget(targetObj);
                break;
            case ObjectStatType.MaxHealthLock:
                lockedMaxHealth = (int)stat.value;
                break;
            case ObjectStatType.SpeedLock:
                lockedSpeed = (int)stat.value;
                break;
            case ObjectStatType.AttackLock:
                lockedAttack = (int)stat.value;
                break;
            case ObjectStatType.DefenseLock:
                lockedDefense = (int)stat.value;
                break;
            case ObjectStatType.VigorLock:
                lockedVigor = (int)stat.value;
                break;
        }
    }

    public void ReturnTarget()
    {
        if (targetEffect != null)
        {
            world.effectManager.ReturnEffect(targetEffect);
            targetEffect = null;
        }
    }

    public void UpdateTarget(WorldObject obj)
    {
        ReturnTarget();
        targetEffect = (NomadTarget)world.PlayEffect(EffectType.NomadTarget, obj.transform.position);
        targetEffect.SetFollow(obj.transform);
    }

    public void ConsumeTarget()
    {
        AddRage(5);
        target = 0;
        if (targetEffect != null)
        {
            targetEffect.Consume();
            targetEffect = null;
        }
    }

    protected override void SetCurrentTile(TileInfo tileInfo)
    {
        base.SetCurrentTile(tileInfo);

        if (tileInfo == null || tileInfo.music == null) return;
        if (!world.dynamicMusic || !AudioManager.TryGetSound(tileInfo.music, out var music)) return;
        world.PlayMusic(music);
    }

    public override Item GetItem(int index)
    {
        if (index < 12)
            return items[index];
        return base.GetItem(index);
    }

    public override void SetItem(int index, Item item)
    {
        if (index < 4)
            base.SetItem(index, item);
        else if (index < 12)
            items[index] = item;
    }

    public void SetAttacking(bool attacking, float attackAngle, Vector2 aimPosition)
    {
        this.attacking = attacking;
        this.attackAngle = attackAngle;
        this.aimPosition = aimPosition;
    }

    public void SetAbilityAimPosition(Vector2 abilityAimPosition)
    {
        this.abilityAimPosition = abilityAimPosition;
    }

    public float GetAttackAngle() => attackAngle;

    public void SetMove(float moveAngle, bool walk, bool moving)
    {
        this.moveAngle = moveAngle;
        this.walk = walk;
        this.moving = moving;

        if (world != null && world.stopTick) return;
        if (moving && !HasPositionalEffect() && !HasPositionalEffect(world.clientTime - NetConstants.Client_Delta) && lastFixedMove != targetUpdatePosition)
            Position = lastFixedMove + (targetUpdatePosition - lastFixedMove) * ((Time.time - targetUpdateTime) / 0.016f);
    }

    private Vector3 Move(float delta)
    {
        if (world.stopTick || HasPositionalEffect()) return lastFixedMove;

        float tilesPerSecond = StatFunctions.TilesPerSecond(GetStatFunctional(StatType.Speed), HasStatusEffect(StatusEffect.Slowed), HasStatusEffect(StatusEffect.Speedy)) * delta * (currentTile?.speed ?? 1);
        if (walk)
            tilesPerSecond *= 0.5f;

        Vector3 moveVector = new Vector3(Mathf.Cos(moveAngle), Mathf.Sin(moveAngle), 0) * tilesPerSecond;
        var position = lastFixedMove;
        var newPosition = position + moveVector;

        bool xOk = CanMoveTo(newPosition.x, position.y);
        bool yOk = CanMoveTo(position.x, newPosition.y);

        if (xOk && yOk)
        {
            if (CanMoveTo(newPosition.x, newPosition.y))
            {
                return newPosition;
            }
        }

        if (xOk)
        {
            return new Vector3(newPosition.x, position.y);
        }
        else if (yOk)
        {
            return new Vector3(position.x, newPosition.y);
        }

        return position;
    }

    private bool CanMoveTo(float x, float y)
    {
        if (world.collision.PlayerCollides(x, y))
            return false;
        if (!world.tilemapManager.CanWalkOn(x, y))
            return false;
        if (!LineOkay(x, y))
            return false;
        return true;
    }

    private bool LineOkay(float x, float y)
    {
        foreach (var position in Bresenham.Line(lastSentPosition.ToInt2(), new Int2((int)x, (int)y)))
            if (world.collision.IsWall(position.x, position.y))
                return false;
        return true;
    }

    public override Vector2 GetPosition()
    {
        return Position;
    }

    protected override Vector2 GetPositionVisual()
    {
        return Position;
    }

    public override void SetPosition(Vec2 position, bool first)
    {
        if (!first) return;
        Position = position.ToVector2();
        lastFixedMove = position.ToVector2();
        targetUpdatePosition = lastFixedMove;
        targetUpdateTime = Time.time;// + 0.016f;
    }

    public override void WorldFixedUpdate(uint time, uint delta)
    {
        base.WorldFixedUpdate(time, delta);

        if (health < GetStatFunctional(StatType.MaxHealth))
        {
            var regen = StatFunctions.HealthRegen(GetStatFunctional(StatType.Vigor), (int)delta, HasStatusEffect(StatusEffect.Healing), HasStatusEffect(StatusEffect.Sick));
            health += regen;
            if (health > GetStatFunctional(StatType.MaxHealth))
                health = GetStatFunctional(StatType.MaxHealth);
            world.gameManager.ui.SetHealthValue((int)health);

            //Debug.Log($"Time: {time / 16}, HP: {health}");
        }

        //Debug.Log(world.clientTickId);
        //Debug.Log(health);

        UpdateMovement(time);

        if (cooldown < cooldownDuration)
        {
            cooldown += (int)delta;
        }

        if (wantsToUseAbility)
        {
            wantsToUseAbility = false;
            DoUseAbility();
        }

        if (attacking)
        {
            Shoot(time);
        }

        UpdateCharacterAnimation();

        targetUpdateTime = Time.time;// + 0.016f;
    }

    public void UpdateMovement(uint time)
    {
        if (time == lastMovementTime) return;
        lastMovementTime = time;

        if (HasPositionalEffect())
        {
            UpdatePositionalEffect(16 / 1000f);
            UpdateTargetUpdatePosition();
        }
        else if (moving)
        {
            Position = Move(16 / 1000f);
            UpdateTargetUpdatePosition();
        }
        else if ((Vector2)Position != (Vector2)targetUpdatePosition)
        {
            Position = targetUpdatePosition;
            lastFixedMove = targetUpdatePosition;
        }
    }

    protected override void Update()
    {
        base.Update();

        //UpdatePositionalEffect();
    }

    public void Goto(Vec2 position)
    {
        Position = position.ToVector2();
        UpdateTargetUpdatePosition();
        targetUpdateTime = Time.time;// + 0.016f;
    }

    private void UpdateTargetUpdatePosition()
    {
        lastFixedMove = Position;
        targetUpdatePosition = Move(16 / 1000f);
    }

    private void Shoot(uint time)
    {
        if (world.stopTick || HasClientEffect(StatusEffect.Dashing) || HasClientEffect(StatusEffect.Grounded) || HasClientEffect(StatusEffect.KnockedBack)) return;

        var item = items[0];
        if (item.IsBlank) return;
        var itemInfo = item.GetInfo();
        if (!(itemInfo is WeaponInfo weaponInfo)) return;
        if (shootCooldown < 0)
        {
            shootCooldown = (int)(1000 / (weaponInfo.rateOfFire * StatFunctions.AttackSpeedModifier(HasStatusEffect(StatusEffect.Fervent), GetAlternateStatIncrease(AlternateStatType.RateOfFire))));
            //Debug.Log($"Time: {world.clientTime} Next Shoot: {world.clientTime + shootCooldown}");

            if (animation is CharacterAnimationData characterAnimation)
                characterAnimation.attackFps = (shootCooldown / 2) / 1000.0f;

            var pos = ((Vector2)Position).ToVec2();
            var target = aimPosition.ToVec2();
            var vector = target - pos;
            var length = vector.Length;
            if (length > 6)
                target = pos + vector.ChangeLength(6, length); // provide current length to prevent a second Sqrt call

            world.gameManager.client.SendAsync(new TnShoot(world.clientTickId, projIds, target, pos));
            projIds = ShootWeapon(item, weaponInfo, target, projIds, pos, time);

            PlaySfxType(SfxType.Shoot);
        }
    }

    private bool HasPositionalEffect(uint worldTime = 0)
    {
        return HasClientEffect(StatusEffect.Charmed, worldTime) || HasClientEffect(StatusEffect.Dashing, worldTime) || HasClientEffect(StatusEffect.KnockedBack, worldTime) || HasClientEffect(StatusEffect.Grounded, worldTime);
    }

    private void UpdatePositionalEffect(float delta)
    {
        UpdatePositional(delta);
    }

    private void AddPositionalEffect(Vec2 vector)
    {
        positionalEffectCollided = false;
        positionalEffectVector = vector;
    }

    private void UpdatePositional(float delta)
    {
        if (!positionalEffectCollided)
        {
            var newPosition = (Vector2)Position + positionalEffectVector.ToVector2() * delta;
            if (!CanMoveTo(newPosition.x, newPosition.y))
                positionalEffectCollided = true;
            else
                Position = newPosition;
        }
    }

    public uint ShootWeapon(Item item, WeaponInfo weapon, Vec2 target, uint projId, Vec2 position, uint time, Action<Projectile> processor = null, float angleOffset = 0)
    {
        var projData = weapon.projectiles[projId % weapon.projectiles.Length];
        bool isPlayer = this is Player;

        var length = position.DistanceTo(target);
        var shootAngle = position.AngleTo(target) + angleOffset;
        foreach (var angle in NetConstants.GetProjectileAngles(shootAngle, projData.angleGap, projData.amount))
        {
            uint id = projId++;
            projData = weapon.projectiles[id % weapon.projectiles.Length];
            ushort damage = PlayerDamage(item.enchantType, item.enchantLevel, weapon.slotType, projData, id);
            if (projData.Type == ProjectileType.Aoe)
            {
                var aoeData = (AoeProjectileData)projData;
                var aoe = (ItemAoeProjectile)world.PlayEffect(EffectType.ItemAoeProjectile, position.ToVector2());
                aoe.Setup(Ally ? world.hittables : world.enemyHittables, time, aoeData, position.ToVector2(), (position + Vec2.FromAngle(angle) * length).ToVector2(), damage, id, true, weapon);
                aoe.players = isPlayer;
                aoe.SetHitSfx("break_potion");
                world.AddAoeProjectile(aoe);
            }
            else
            {
                var proj = world.gameManager.objectManager.GetProjectile();
                proj.Setup(Ally ? world.hittables : world.enemyHittables, position.ToVector2(), projData, angle, id, time, damage, HasStatusEffect(StatusEffect.Reach));
                proj.players = isPlayer;
                processor?.Invoke(proj);
            }
        }
        return projId;
    }

    private ushort PlayerDamage(ItemEnchantType enchant, int enchantLevel, SlotType slotType, ProjectileData data, uint id)
    {
        var rand = world.GetRand(id);
        WeaponFunctions.GetProjectileDamage(slotType, data, out var minDamage, out var maxDamage);
        var damage = (int)((minDamage + (ushort)((maxDamage - minDamage) * rand)) * StatFunctions.AttackModifier(GetStatFunctional(StatType.Attack), HasStatusEffect(StatusEffect.Damaging)));
        if (enchant == ItemEnchantType.Damaging)
            damage = (int)(damage * EnchantFunctions.Damage(enchantLevel));
        return (ushort)damage;
    }

    public void PositionSent(Vec2 position)
    {
        Int2 tilePos = position;
        var tile = world.tilemapManager.GetTileType(tilePos.x, tilePos.y);
        if (tile == 0)
            currentTile = null;
        else
            currentTile = (TileInfo)GameData.objects[tile];

        var time = world.clientTime;
        var timeDif = time - lastSentPositionTime;
        var tps = lastSentPosition.DistanceTo(position) / (timeDif / 1000f);

        lastSentPositionTime = time;
        lastSentPosition = position;
    }

    public override bool IsHitBy(Vec2 position, Projectile projectile, out bool killed)
    {
        bool hit = base.IsHitBy(position, projectile, out killed);
        if (hit && !HasStatusEffect(StatusEffect.Invulnerable))
        {
            var pos = GetPosition().ToVec2();
            var damageTaken = GetDamageTaken(projectile.damage);
            TakeDamage(damageTaken);
            ApplyOnHitEffects(projectile.data.onHitEffects, pos, position);
            if (damageTaken == 0) return hit;
            world.gameManager.client.SendAsync(new TnHit(world.clientTickId, projectile.projId, gameId, pos));
        }
        return hit;
    }

    public override void HitBy(AoeProjectile projectile)
    {
        base.HitBy(projectile);

        if (IsInvincible() || HasStatusEffect(StatusEffect.Invulnerable)) return;

        var pos = GetPosition().ToVec2();
        var damageTaken = GetDamageTaken(projectile.damage);
        TakeDamage(damageTaken);
        ApplyOnHitEffects(projectile.aoeData.onHitEffects, pos, Vec2.zero);
        if (damageTaken == 0) return;
        world.gameManager.client.SendAsync(new TnHit(world.clientTickId, projectile.projId, gameId, pos));
    }

    public override void TakeDamage(int damageTaken)
    {
        base.TakeDamage(damageTaken);
        PlayHurt();
        if (damageTaken == 0) return;

        health -= damageTaken;

        world.gameManager.ui.SetHealthValue((int)health);
        world.gameManager.ui.PlayerDamageTaken();
    }

    private void PlayHurt()
    {
        AudioManager.PlaySound("hurt-" + UnityEngine.Random.Range(1, 6));
    }

    private void ApplyOnHitEffects(StatusEffectData[] onHits, Vec2 position, Vec2 projPos)
    {
        if (onHits.Length == 0) return;
        for (int i = 0; i < onHits.Length; i++)
        {
            var onHit = onHits[i];
            switch (onHit.type)
            {
                case StatusEffect.Charmed:
                    ShowAlert("Charmed", Color.red, true);
                    AddCharmed(position, projPos, onHit.duration);
                    break;
                case StatusEffect.KnockedBack:
                    ShowAlert("KnockedBack", Color.red, true);
                    AddKnockedBack(position, projPos, onHit.duration);
                    break;
                case StatusEffect.Grounded:
                    ShowAlert("Grounded", Color.red, true);
                    AddGrounded(position, onHit.duration);
                    break;
                case StatusEffect.Slowed:
                    ShowAlert("Slowed", Color.red, true);
                    AddClientEffect(onHit.type, onHit.duration);
                    break;
                case StatusEffect.Mundane:
                    rage = 0;
                    AddClientEffect(onHit.type, onHit.duration);
                    break;
                default:
                    AddClientEffect(onHit.type, onHit.duration);
                    break;
            }
        }
    }

    private void AddCharmed(Vec2 position, Vec2 charmerPosition, uint duration)
    {
        if (HasPositionalEffect()) return;
        AddPositionalEffect(StatusEffectFunctions.GetCharmedPositionVector(position, charmerPosition));
        AddClientEffect(StatusEffect.Charmed, duration);
    }

    private void AddKnockedBack(Vec2 position, Vec2 knockerPosition, uint duration)
    {
        if (HasPositionalEffect()) return;
        AddPositionalEffect(StatusEffectFunctions.GetKnockedBackPositionVector(position, knockerPosition));
        AddClientEffect(StatusEffect.KnockedBack, duration);
    }

    private void AddGrounded(Vec2 position, uint duration)
    {
        if (HasPositionalEffect()) return;
        AddPositionalEffect(Vec2.zero);
        AddClientEffect(StatusEffect.Grounded, duration);
    }

    private void AddDash(Vec2 position, Vec2 target, int rage)
    {
        AddPositionalEffect(AbilityFunctions.BladeWeaver.GetDashPositionVector(position.AngleTo(target), rage));
    }

    public void IncrementAbilityValue()
    {
        switch ((ClassType)info.id)
        {
            case ClassType.Brewer:
                abilityValue = (byte)((abilityValue + 1) % 2);

                var pos = Position;
                pos.z = 0;
                var brewerEffect = (BerserkerAbility)world.PlayEffect(EffectType.BrewerSelection, pos);
                brewerEffect.SetSprite(TextureManager.GetSprite("BrewerPotion-" + (abilityValue + 1)));
                break;
        }
    }

    public void SetAbilityValue(byte value)
    {
        abilityValue = value;
    }

    public void UseAbility(bool first)
    {
        if (cooldown < cooldownDuration)
        {
            if (first)
                world.GameChat("Ability is still on cooldown", ChatType.Error);
            return;
        }

        if (rage == 0 || 
            (info.id == (ushort)ClassType.Lancer && rage < AbilityFunctions.Lancer.Rage_Cost) ||
            (info.id == (ushort)ClassType.Minister && rage < AbilityFunctions.Minister.GetRageCost(rage)) ||
            (info.id == (ushort)ClassType.Nomad && rage < AbilityFunctions.Nomad.Ability_Cost))
        {
            if (first)
                world.GameChat("Not enough rage available! Attack enemies to gain more.", ChatType.Error);
            return;
        }

        wantsToUseAbility = true;
    }

    private void DoUseAbility()
    {
        if (cooldown < cooldownDuration)
        {

            return;
        }

        if (HasPositionalEffect())
        {

            return;
        }

        if (rage == 0 || 
            (info.id == (ushort)ClassType.Lancer && rage < AbilityFunctions.Lancer.Rage_Cost) || 
            (info.id == (ushort)ClassType.Minister && rage < AbilityFunctions.Minister.GetRageCost(rage)) ||
            (info.id == (ushort)ClassType.Nomad && rage < AbilityFunctions.Nomad.Ability_Cost))
        {

            return;
        }

        Vec2 position = ((Vector2)Position).ToVec2();
        var target = abilityAimPosition == Vector2.zero ? aimPosition.ToVec2() : abilityAimPosition.ToVec2();
        abilityAimPosition = Vector2.zero;

        switch ((ClassType)info.id)
        {
            case ClassType.Warrior:
                //world.PlayWarriorAbilityEffect(new WarriorAbilityWorldEffect(gameId, position, (byte)rage, GetStatFunctional(StatType.Attack)));
                var blast = (AreaBlast)world.PlayEffect(EffectType.AreaBlast, position.ToVector2());
                blast.SetInfo(2, Color.white);

                rage = 0;
                break;
            case ClassType.Alchemist:
                world.PlayAlchemistAbilityEffect(new AlchemistAbilityWorldEffect(gameId, target, (byte)rage, GetStatFunctional(StatType.Attack)));
                rage = 0;
                break;
            case ClassType.Lancer:
                var lancerItem = new Item(0x2a1);
                var offset = AbilityFunctions.Lancer.GetAngleOffset(projIds);
                //target = position + Vec2.FromAngle(position.AngleTo(target));

                projIds = ShootWeapon(lancerItem, (WeaponInfo)lancerItem.GetInfo(), target, projIds, position, world.clientTime, proj =>
                {
                    proj.damage = (ushort)AbilityFunctions.Lancer.GetProjectileDamage(rage, GetStatFunctional(StatType.Attack));
                }, offset);
                rage -= AbilityFunctions.Lancer.Rage_Cost;
                break;
            case ClassType.Commander:
                world.PlayCommanderAbilityEffect(new CommanderAbilityWorldEffect(gameId, position, (byte)rage, GetStatFunctional(StatType.Attack)));
                rage = 0;
                break;
            case ClassType.Minister:
                var cost = AbilityFunctions.Minister.GetRageCost(rage);
                world.PlayMinisterAbilityEffect(new MinisterAbilityWorldEffect(gameId, position, cost, GetStatFunctional(StatType.Attack)));
                rage -= cost;
                break;
            case ClassType.Berserker:
                world.PlayBerserkerAbilityEffect(new BerserkerAbilityWorldEffect(gameId, position, position.AngleTo(target) * AngleUtils.Rad2Deg, (byte)rage, GetStatFunctional(StatType.Attack)));
                rage = 0;
                break;
            case ClassType.Ranger:
                world.PlayEffect(EffectType.RangerArrowsShoot, position.ToVector2());
                rage = 0;
                break;
            case ClassType.Brewer:
                AudioManager.PlaySound("drink_potion");
                world.PlayBrewerAbilityEffect(new BrewerAbilityWorldEffect(gameId, position, (byte)rage, GetStatFunctional(StatType.Attack), abilityValue));
                rage = 0;
                break;
            case ClassType.Bladeweaver:
                var rageToUse = abilityValue;
                world.PlayBladeweaverAbilityEffect(new BladeweaverAbilityWorldEffect(gameId));
                AddDash(position, target, rageToUse);
                rage -= rageToUse;

                var bwItem = new Item(0x2a8);
                projIds = ShootWeapon(bwItem, (WeaponInfo)bwItem.GetInfo(), target, projIds, position, world.clientTime, proj =>
                {
                    //proj.SetSize(AbilityFunctions.Lancer.GetProjectileSize(rage));
                    proj.damage = (ushort)AbilityFunctions.BladeWeaver.GetProjectileDamage(rageToUse, GetStatFunctional(StatType.Attack));
                });
                break;
            case ClassType.Nomad:
                world.PlayNomadAbilityEffect(new NomadAbilityWorldEffect(gameId, target));
                rage -= AbilityFunctions.Nomad.Ability_Cost;
                break;
        }

        cooldownDuration = AbilityFunctions.GetAbilityCooldownMs((byte)rage, info.id);
        cooldown = 0;

        world.gameManager.client.SendAsync(new TnUseAbility(world.clientTickId, position, target, abilityValue));
    }

    private static StatType[] statTypes = (StatType[])Enum.GetValues(typeof(StatType));

    public bool CanLevelUp()
    {
        foreach (var type in statTypes)
            if (CanLevelUp(type))
                return true;
        return false;
    }

    private bool CanLevelUp(StatType type)
    {
        if (GetStatBase(type) >= ((TitanCore.Data.Entities.CharacterInfo)info).stats[type].maxValue) return false;
        var cost = StatFunctions.GetLevelUpCost((TitanCore.Data.Entities.CharacterInfo)info, type, GetStatBase(type), 1);
        return cost <= fullSouls;
    }

    public void AddRage(int amount = 1)
    {
        if (HasStatusEffect(StatusEffect.Mundane)) return;
        rage = Math.Min(rage + amount, 100);
    }

    private const float Emote_Cooldown = 0;

    public void UseEmote(EmoteType type)
    {
        var seconds = (float)(DateTime.Now - lastEmote).TotalSeconds;
        if (seconds < Emote_Cooldown)
        {
            world.GameChat($"Emote available in {Mathf.RoundToInt((Emote_Cooldown - seconds) * 10) / 10f} seconds.", ChatType.Error);
            return;
        }
        lastEmote = DateTime.Now;
        world.gameManager.client.SendAsync(new TnEmote(type));
    }

    public int GetStatLock(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth:
                return lockedMaxHealth;
            case StatType.Speed:
                return lockedSpeed;
            case StatType.Attack:
                return lockedAttack;
            case StatType.Defense:
                return lockedDefense;
            case StatType.Vigor:
                return lockedVigor;
        }
        return 0;
    }
}