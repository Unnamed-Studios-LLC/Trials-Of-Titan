using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TitanCore.Core;
using TitanCore.Data;
using TitanCore.Data.Items;
using TitanCore.Data.Map;
using TitanCore.Net;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Models;
using TitanCore.Net.Packets.Server;
using UnityEngine;
using UnityEngine.UI;
using Utils.NET.Geometry;
using Utils.NET.Utils;

public class World : MonoBehaviour
{
    public const uint Fixed_Delta = 16;

    public static Color soulColor = new Color(106f / 255f, 236f / 255f, 225f / 255f, 1);

    public GameManager gameManager;

    public EffectManager effectManager;

    public TilemapManager tilemapManager;

    public GameMenuManager menuManager;

    public Camera worldCamera;

    public Camera minimapCamera;

    public Player player;

    public CollisionMap collision;

    public bool allowControls = false;

    public bool stopTick = false;

    public SideChat sideChat;

    public Image lowHpVignette;

    public QuestBanner questBanner;

    public DeathMenu deathMenu;

    [HideInInspector]
    public TnMapInfo mapInfo;

    private ConcurrentQueue<TnPacket> packetQueue = new ConcurrentQueue<TnPacket>();

    [HideInInspector]
    public uint clientTickId = 0;

    public uint clientTime => clientTickId * NetConstants.Client_Delta;

    [HideInInspector]
    public uint serverTickId = 0;

    [HideInInspector]
    public TnQuest quest;

    private Dictionary<uint, WorldObject> objects = new Dictionary<uint, WorldObject>();

    private List<Projectile> projectiles = new List<Projectile>();

    private List<AoeProjectile> aoeProjectiles = new List<AoeProjectile>();

    [HideInInspector]
    public List<Enemy> enemies = new List<Enemy>();

    [HideInInspector]
    public List<WorldObject> hittables = new List<WorldObject>();

    [HideInInspector]
    public List<WorldObject> enemyHittables = new List<WorldObject>();

    private List<IContainer> containers = new List<IContainer>();

    private List<IInteractable> interactables = new List<IInteractable>();

    public List<Character> characters = new List<Character>();

    public System.Random playerRand;

    private double[] randArray;

    private IContainer nearestContainer;

    [HideInInspector]
    public bool dynamicMusic = false;

    private Sound currentMusic;

    private float cameraRotation = 0;
    public float CameraRotation
    {
        get => cameraRotation;
        set
        {
            cameraRotation = value;

            var angles = transform.eulerAngles;
            angles.z = value;
            transform.eulerAngles = angles;
        }
    }

    private void Awake()
    {

    }

    private void Start()
    {

    }

    public void SetupClient()
    {
        gameManager.client.AddHandler<TnTick>(HandlePacketQueue);
        gameManager.client.AddHandler<TnProjectiles>(HandlePacketQueue);
        gameManager.client.AddHandler<TnTiles>(HandlePacketQueue);
        gameManager.client.AddHandler<TnChats>(HandlePacketQueue);
        gameManager.client.AddHandler<TnTradeRequest>(HandlePacketQueue);
        gameManager.client.AddHandler<TnTradeStart>(HandlePacketQueue);
        gameManager.client.AddHandler<TnTradeUpdate>(HandlePacketQueue);
        gameManager.client.AddHandler<TnTradeResult>(HandlePacketQueue);
        gameManager.client.AddHandler<TnPlayEffect>(HandlePacketQueue);
        gameManager.client.AddHandler<TnDeath>(HandlePacketQueue);
        gameManager.client.AddHandler<TnQuest>(HandlePacketQueue);
        gameManager.client.AddHandler<TnQuestDamage>(HandlePacketQueue);
        gameManager.client.AddHandler<TnGoto>(HandlePacketQueue);
        gameManager.client.AddHandler<TnEmoteUnlocked>(HandlePacketQueue);
        gameManager.client.AddHandler<TnChangeMusic>(HandlePacketQueue);
        gameManager.client.AddHandler<TnWorldChange>(HandlePacketQueue);
        gameManager.client.AddHandler<TnSkinUnlocked>(HandlePacketQueue);
    }

    public void NewMap(TnMapInfo mapInfo)
    {
        stopTick = false;
        enemyHittables.Clear();
        var objs = objects.Values.ToArray();
        foreach (var obj in objs)
        {
            RemoveObject(obj);
        }
        player = null;

        serverTickId = 0;

        this.mapInfo = mapInfo;
        allowControls = false;

        quest = null;
        playerRand = new System.Random(mapInfo.seed);
        MakeRandArray();

        SetNearestContainer(null);
        SetNearestInteractable(null);
        packetQueue = new ConcurrentQueue<TnPacket>();

        NomadCharm.consumedCharms.Clear();

        gameManager.ui.NewWorld(mapInfo);
        tilemapManager.Initialize(mapInfo.width, mapInfo.height);
        collision = new CollisionMap(mapInfo.width, mapInfo.height);

        if (!Application.isMobilePlatform)
        {
            //Screen.SetResolution(1000, 800, false);
        }

        gameManager.client.SendAsync(new TnStartTick());

        SetWorldMusic(mapInfo.music);
    }

    private void SetWorldMusic(string musicKey)
    {
        dynamicMusic = false;

        var split = musicKey.Split(':');
        for (int i = 0; i < split.Length - 1; i++)
        {
            switch(split[i])
            {
                case "dynamic":
                    dynamicMusic = true;
                break;
            }
        }

        if (!AudioManager.TryGetSound(split[split.Length - 1], out var sound)) return;

        PlayMusic(sound);
    }

    public void PlayMusic(Sound sound)
    {
        if (currentMusic == sound) return;

        var musicPlayer = AudioManager.GetBackgroundAudioPlayer();
        musicPlayer.ClearQueue();
        if (AudioManager.TryGetSound(sound.clip.name + "-intro", out var intro))
        {
            musicPlayer.Enqueue(intro, false, true);
            musicPlayer.Enqueue(sound, true, false);
        }
        else
        {
            musicPlayer.Enqueue(sound, true, true);
        }
        musicPlayer.Next();
        currentMusic = sound;
    }

    private void FixedUpdate()
    {
        if (gameManager.mapEditor) return;

        clientTickId++;

        ProcessPacketQueue();

        if (stopTick) return;

        foreach (var obj in objects.Values)
            obj.WorldFixedUpdate(clientTickId * Fixed_Delta, Fixed_Delta);

        foreach (var proj in projectiles.ToArray())
            proj.WorldFixedUpdate(clientTickId * Fixed_Delta);

        foreach (var proj in aoeProjectiles.ToArray())
            proj.WorldFixedUpdate(clientTickId * Fixed_Delta);

        questBanner.UpdateQuest();
    }

    private void LateUpdate()
    {
        if (gameManager.mapEditor) return;

        if (stopTick) return;

        PositionCameraOnPlayer();

        UpdateContainer();
        UpdateInteractable();
        UpdateLowHpVignette();

        gameManager.ui.PostLateUpdate();
    }

    private void UpdateLowHpVignette()
    {
        if (lowHpVignette == null || player == null) return;
        bool showing = player.health < (player.GetStatFunctional(StatType.MaxHealth)) * 0.2f;
        lowHpVignette.gameObject.SetActive(showing);
        if (showing)
        {
            var color = Color.red;
            color.a = (Mathf.Sin(Time.time * Mathf.PI) + 1f) / 2f;
            lowHpVignette.color = color;
        }
    }

    private void UpdateContainer()
    {
        var closest = containers.Closest(_ =>
        {
            if (!_.ShowLootMenu()) return float.MaxValue;

            var p = _.Position;
            var playerPos = player.Position;
            return new Vec2(p.x, p.y).DistanceTo(new Vec2(playerPos.x, playerPos.y));
        });
        if (closest == null || !closest.ShowLootMenu())
            SetNearestContainer(null);
        else
        {
            var p = closest.Position;
            var playerPos = player.Position;
            var distance = new Vec2(p.x, p.y).DistanceTo(new Vec2(playerPos.x, playerPos.y));
            if (distance > 0.7f)
                SetNearestContainer(null);
            else
                SetNearestContainer(closest);
        }
    }

    private void SetNearestContainer(IContainer container)
    {
        nearestContainer = container;
        if (gameManager.ui == null) return;
        if (container == null)
            gameManager.ui.HideContainer();
        else
            gameManager.ui.ShowContainer(container);
    }

    private void UpdateInteractable()
    {
        if (player == null) return;
        var closest = interactables.Closest(_ =>
        {
            var p = ((WorldObject)_).transform.localPosition;
            var playerPos = player.Position;
            return new Vec2(p.x, p.y).DistanceTo(new Vec2(playerPos.x, playerPos.y));
        });

        if (closest == null)
            SetNearestInteractable(null);
        else
        {
            var p = ((WorldObject)closest).transform.localPosition;
            var playerPos = player.Position;
            var distance = new Vec2(p.x, p.y).DistanceTo(new Vec2(playerPos.x, playerPos.y));
            if (distance > 0.7f)
                SetNearestInteractable(null);
            else
                SetNearestInteractable(closest);
        }
    }

    private void SetNearestInteractable(IInteractable interactable)
    {
        gameManager.ui.SetInteractable(interactable);
    }

    private void ProcessPacketQueue()
    {
        while (packetQueue.TryDequeue(out var packet) && !stopTick)
        {
            ProcessPacket(packet);
        }
    }

    private void ProcessPacket(TnPacket packet)
    {
        switch (packet)
        {
            case TnTick tick:
                ProcessTick(tick);
                break;
            case TnProjectiles projectiles:
                ProcessProjectiles(projectiles);
                break;
            case TnTiles tiles:
                ProcessTiles(tiles);
                break;
            case TnChats chats:
                ProcessChats(chats);
                break;
            case TnTradeRequest tradeRequest:
                ProcessTradeRequest(tradeRequest);
                break;
            case TnTradeStart tradeStart:
                ProcessTradeStart(tradeStart);
                break;
            case TnTradeUpdate tradeUpdate:
                ProcessTradeUpdate(tradeUpdate);
                break;
            case TnTradeResult tradeResult:
                ProcessTradeResult(tradeResult);
                break;
            case TnPlayEffect playEffect:
                ProcessPlayEffect(playEffect);
                break;
            case TnDeath death:
                ProcessDeath(death);
                break;
            case TnQuest quest:
                ProcessQuest(quest);
                break;
            case TnQuestDamage questDamage:
                ProcessQuestDamage(questDamage);
                break;
            case TnGoto gotoPacket:
                ProcessGoto(gotoPacket);
                break;
            case TnEmoteUnlocked emoteUnlocked:
                ProcessEmoteUnlocked(emoteUnlocked);
                break;
            case TnChangeMusic changeMusic:
                ProcessChangeMusic(changeMusic);
                break;
            case TnWorldChange worldChange:
                ProcessWorldChange(worldChange);
                break;
            case TnSkinUnlocked skinUnlocked:
                ProcessSkinUnlocked(skinUnlocked);
                break;
        }
    }

    private void PositionCameraOnPlayer()
    {
        if (player == null) return;
        var pos = player.transform.position;
        pos.z = worldCamera.transform.position.z;

        var offset = new Vector3(Screen.height * 0.125f, 0, 0);
        offset *= (worldCamera.orthographicSize * 2) / Screen.height;
        worldCamera.transform.position = pos;// + offset;

        worldCamera.transparencySortAxis = worldCamera.transform.up;

        var localPos = player.transform.localPosition;
        tilemapManager.SetFocus(Mathf.FloorToInt(localPos.x), Mathf.FloorToInt(localPos.y));
    }

    public void CreateMapEditorMap()
    {
        tilemapManager.Initialize(1000, 1000);
    }

    private void MakeRandArray()
    {
        randArray = new double[500];
        for (int i = 0; i < randArray.Length; i++)
            randArray[i] = playerRand.NextDouble();
    }

    public double GetRand(uint index)
    {
        return randArray[index % randArray.Length];
    }

    private Player CreatePlayer(ushort type)
    {
        player = gameManager.objectManager.GetPlayer(GameData.objects[type]);
        enemyHittables.Add(player);

        for (int i = 0; i < gameManager.ui.playerSlots.Length; i++)
        {
            var slot = gameManager.ui.playerSlots[i];
            slot.SetOwner(player, i);
        }

        allowControls = true;

        return player;
    }

    public Effect PlayEffect(EffectType effectType, Vector3 position)
    {
        var effect = effectManager.GetEffect(effectType);
        effect.Init(effectManager, this);
        effect.transform.SetParent(transform);
        effect.transform.localPosition = position;
        return effect.GetComponent<Effect>();
    }

    public bool TryGetObject(uint gameId, out WorldObject worldObject)
    {
        return objects.TryGetValue(gameId, out worldObject);
    }

    public void GameChat(string text, ChatType type)
    {
        if (sideChat == null) return;
        sideChat.AddChat(text, type);
    }

    public void GameChat(string owner, string text, ChatType type, byte classQuests = 0, Rank rank = Rank.Player)
    {
        if (sideChat == null) return;
        sideChat.AddChat(owner, text, type, classQuests, rank);
    }

    #region Packet Handlers

    private void HandlePacketQueue(TnPacket tick)
    {
        packetQueue.Enqueue(tick);
    }

    #endregion

    #region Packet Processors

    private void ProcessTick(TnTick tick)
    {
        serverTickId = tick.tickId;

        foreach (var newObjectStats in tick.newObjects)
        {
            CreateObject(newObjectStats);
        }

        foreach (var updatedObjectStats in tick.updatedObjects)
        {
            UpdateObject(updatedObjectStats);
        }

        foreach (var gameId in tick.removedObjects)
        {
            RemoveObject(gameId);
        }

        player.UpdateMovement(clientTime);

        var playerPos = new Vec2(player.transform.localPosition.x, player.transform.localPosition.y);
        gameManager.client.SendAsync(new TnMove(clientTickId, tick.tickId, playerPos));
        player.PositionSent(playerPos);

        //Debug.Log(clientTickId);
        //Debug.Log(player.health);
    }

    private void ProcessProjectiles(TnProjectiles projectiles)
    {
        foreach (var allyProjectile in projectiles.allyProjectiles)
        {
            ProcessAllyProjectile(allyProjectile);
        }

        foreach (var allyAoeProjectile in projectiles.allyAoeProjectiles)
        {
            ProcessAllyAoeProjectile(allyAoeProjectile);
        }

        foreach (var enemyProjectile in projectiles.enemyProjectiles)
        {
            ProcessEnemyProjectile(enemyProjectile);
        }

        foreach (var enemyAoeProjectile in projectiles.enemyAoeProjectiles)
        {
            ProcessEnemyAoeProjectile(enemyAoeProjectile);
        }

        gameManager.client.SendAsync(new TnProjectilesAck(clientTickId, projectiles.tickId));
    }

    private void ProcessTiles(TnTiles tiles)
    {
        foreach (var tile in tiles.tiles)
        {
            tilemapManager.ProcessMapTile(tile);
        }
    }

    private void ProcessChats(TnChats chats)
    {
        foreach (var chat in chats.chats)
        {
            if (chat.ownerGameId < 0)
            {
                switch ((ChatData.ChatOwner)chat.ownerGameId)
                {
                    case ChatData.ChatOwner.Error:
                        GameChat(chat.text, ChatType.Error);
                        break;
                    case ChatData.ChatOwner.Info:
                        GameChat(chat.text, ChatType.ServerInfo);
                        break;
                    case ChatData.ChatOwner.Mannah:
                        GameChat("Mannah the Malevolent", chat.text, ChatType.Enemy);
                        break;
                }
                continue;
            }

            if (!TryGetObject((uint)chat.ownerGameId, out var worldObject)) continue;
            worldObject.ShowChatBubble(chat.text);
            if (worldObject is Character character)
                GameChat(worldObject.GetName(), chat.text, ChatType.Player, character.classQuests, character.rank);
            else
                GameChat(worldObject.GetName(), chat.text, ChatType.Enemy);
        }
    }

    private void ProcessTradeRequest(TnTradeRequest tradeRequest)
    {
        gameManager.ui.ShowRequest($"{tradeRequest.fromPlayer} would like to trade", new TradeRequest(this, tradeRequest));
    }

    private void ProcessTradeStart(TnTradeStart tradeStart)
    {
        if (!TryGetObject(tradeStart.otherGameId, out var other) || !(other is Character otherCharacter))
        {
            gameManager.ui.tradeMenu.Cancel();
            return;
        }
        gameManager.ui.tradeMenu.StartTrade(tradeStart, otherCharacter, player);
    }

    private void ProcessTradeUpdate(TnTradeUpdate tradeUpdate)
    {
        gameManager.ui.tradeMenu.UpdateTrade(tradeUpdate);
    }

    private void ProcessTradeResult(TnTradeResult tradeResult)
    {
        gameManager.ui.tradeMenu.gameObject.SetActive(false);
    }

    private void ProcessPlayEffect(TnPlayEffect playEffect)
    {
        switch (playEffect.effect.Type)
        {
            case WorldEffectType.Bomb:
                PlayBombEffect((BombWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.LevelUp:
                PlayLevelUpEffect((LevelUpWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.BombBlast:
                PlayBombBlastEffect((BombBlastWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.HealLaser:
                PlayHealLaserEffect((HealLaserWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.WarriorAbility:
                PlayWarriorAbilityEffect((WarriorAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.AlchemistAbility:
                PlayAlchemistAbilityEffect((AlchemistAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.CommanderAbility:
                PlayCommanderAbilityEffect((CommanderAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.MinisterAbility:
                PlayMinisterAbilityEffect((MinisterAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.BerserkerAbility:
                PlayBerserkerAbilityEffect((BerserkerAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.RangerAbility:
                PlayRangerAbilityEffect((RangerAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.BrewerAbility:
                PlayBrewerAbilityEffect((BrewerAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.BladeweaverAbility:
                PlayBladeweaverAbilityEffect((BladeweaverAbilityWorldEffect)playEffect.effect);
                break;
            case WorldEffectType.NomadAbility:
                PlayNomadAbilityEffect((NomadAbilityWorldEffect)playEffect.effect);
                break;
        }
    }

    private void ProcessDeath(TnDeath death)
    {
        stopTick = true;
        lowHpVignette.gameObject.SetActive(false);
        deathMenu.Show(player, death);
    }

    public void ProcessQuest(TnQuest quest)
    {
        //questBanner.SetQuest(quest);
        this.quest = quest;
        if (!GameData.objects.TryGetValue(quest.objectType, out var info)) return;
        if (player != null)
        {
            player.ShowAlert("Quest Received!", Color.green);

            var questEffect = (BerserkerAbility)PlayEffect(EffectType.QuestReceived, player.Position);
            questEffect.SetSprite(TextureManager.GetDisplaySprite(info));
        }
    }

    public void ProcessQuestDamage(TnQuestDamage questDamage)
    {
        questBanner.SetDamage(questDamage.damage);
    }

    public void ProcessGoto(TnGoto gotoPacket)
    {
        player.Goto(gotoPacket.position);
        player.AddClientEffect(StatusEffect.Invulnerable, 1000);
        gameManager.client.SendAsync(new TnGotoAck(clientTickId));
    }

    public void ProcessEmoteUnlocked(TnEmoteUnlocked emoteUnlocked)
    {
        var emoteInfo = (EmoteUnlockerInfo)GameData.objects[emoteUnlocked.emoteType];
        player.UseEmote(emoteInfo.emoteType);
        Account.UnlockEmote(emoteInfo.emoteType);
    }

    public void ProcessChangeMusic(TnChangeMusic changeMusic)
    {
        SetWorldMusic(changeMusic.musicName);
    }

    public void ProcessWorldChange(TnWorldChange worldChange)
    {
        stopTick = true;
        packetQueue = new ConcurrentQueue<TnPacket>();
        gameManager.ui.WorldLoading();
    }

    public void ProcessSkinUnlocked(TnSkinUnlocked skinUnlocked)
    {
        Account.UnlockItem(skinUnlocked.skinType);
    }

    #endregion

    private void CreateObject(NewObjectStats newObject)
    {
        WorldObject worldObject;
        if (newObject.gameId == mapInfo.playerGameId)
        {
            worldObject = CreatePlayer(newObject.type);
            worldObject.gameId = newObject.gameId;
            ProcessStats(worldObject, newObject.stats, true);
            gameManager.ui.WorldLoaded();
        }
        else if (!gameManager.objectManager.TryGetObject(newObject.type, out worldObject))
        {
            Debug.Log("Failed to create object of type: 0x" + newObject.type.ToString("X"));
            return;
        }
        else
        {
            worldObject.gameId = newObject.gameId;
            ProcessStats(worldObject, newObject.stats, true);
        }

        SortObject(worldObject);
    }

    private void UpdateObject(UpdatedObjectStats updatedObject)
    {
        if (!TryGetObject(updatedObject.gameId, out var worldObject))
        {

            return; // failed to find the object
        }
        ProcessStats(worldObject, updatedObject.stats, false);
    }

    private void ProcessStats(WorldObject worldObject, NetStat[] stats, bool first)
    {
        worldObject.NetUpdate(stats, first);
    }

    private void RemoveObject(uint gameId)
    {
        if (!TryGetObject(gameId, out var worldObject))
        {

            return; // failed to find the object
        }
        RemoveObject(worldObject);
    }

    public void RemoveObject(WorldObject worldObject)
    {
        UnsortObject(worldObject);

        gameManager.objectManager.ReturnObject(worldObject);
    }

    private void ProcessAllyProjectile(AllyProjectile allyProjectile)
    {
        if (!objects.TryGetValue(allyProjectile.ownerId, out var owner))
        {
            return;
        }

        if (!(owner is Character character)) return;
        character.Shoot(allyProjectile, clientTickId * Fixed_Delta);
    }

    private void ProcessAllyAoeProjectile(AllyAoeProjectile allyProjectile)
    {
        if (!objects.TryGetValue(allyProjectile.ownerId, out var owner))
        {
            return;
        }

        if (!(owner is Character character)) return;
        character.Shoot(allyProjectile, clientTickId * Fixed_Delta);
    }

    private void ProcessEnemyProjectile(EnemyProjectile enemyProjectile)
    {
        if (!objects.TryGetValue(enemyProjectile.ownerId, out var owner))
        {
            return;
        }

        if (!(owner is Enemy enemy)) return;

        enemy.Shoot(enemyProjectile, clientTickId * Fixed_Delta);
    }

    private void ProcessEnemyAoeProjectile(EnemyAoeProjectile enemyProjectile)
    {
        if (!objects.TryGetValue(enemyProjectile.ownerId, out var owner))
        {
            return;
        }

        if (!(owner is Enemy enemy)) return;

        enemy.Shoot(enemyProjectile, clientTickId * Fixed_Delta);
    }

    private void SortObject(WorldObject worldObject)
    {
        objects.Add(worldObject.gameId, worldObject);

        switch (worldObject)
        {
            case Enemy enemy:
                enemies.Add(enemy);
                hittables.Add(enemy);
                break;
            case Character character:
                characters.Add(character);
                break;
            case StaticObject staticObject:
                var staticInfo = (StaticObjectInfo)staticObject.info;
                if (staticInfo.collidable)
                {
                    var pos = staticObject.Position;
                    collision.Set((int)pos.x, (int)pos.y, CollisionType.Object);
                }
                break;
        }

        if (worldObject is IContainer container)
            containers.Add(container);

        if (worldObject is IInteractable interactable)
            interactables.Add(interactable);

        if (player != null && player.target != 0 && player.target == worldObject.gameId)
        {
            player.UpdateTarget(worldObject);
        }
    }

    private void UnsortObject(WorldObject worldObject)
    {
        objects.Remove(worldObject.gameId);

        switch (worldObject)
        {
            case Enemy enemy:
                enemies.Remove(enemy);
                hittables.Remove(enemy);
                break;
            case Character character:
                characters.Remove(character);
                break;
            case StaticObject staticObject:
                var staticInfo = (StaticObjectInfo)staticObject.info;
                if (staticInfo.collidable)
                {
                    var pos = staticObject.Position;
                    collision.Set((int)pos.x, (int)pos.y, CollisionType.None);
                }
                break;
        }

        if (worldObject is IContainer container)
            containers.Remove(container);

        if (worldObject is IInteractable interactable)
            interactables.Remove(interactable);

        if (player != null && player.target != 0 && player.target == worldObject.gameId)
        {
            player.ReturnTarget();
        }
    }

    public void AddProjectile(Projectile proj)
    {
        projectiles.Add(proj);
    }

    public void RemoveProjectile(Projectile proj)
    {
        projectiles.Remove(proj);
    }

    public void AddAoeProjectile(AoeProjectile proj)
    {
        aoeProjectiles.Add(proj);
    }

    public void RemoveAoeProjectile(AoeProjectile proj)
    {
        aoeProjectiles.Remove(proj);
    }

    public void Teleport(Character character)
    {
        if (character == null) return;
        gameManager.client.SendAsync(new TnChat("/teleport " + character.playerName));
    }

    public void Trade(Character character)
    {
        if (character == null) return;
        gameManager.client.SendAsync(new TnChat("/trade " + character.playerName));
    }

    public void Escape()
    {
        gameManager.client.SendAsync(new TnEscape());
    }

    public void UsePotion()
    {
        for (int i = 4; i < gameManager.ui.playerSlots.Length; i++)
        {
            var item = player.GetItem(i);
            if (item.IsBlank) continue;
            var slot = gameManager.ui.playerSlots[i];
            var info = item.GetInfo();
            if (info.heals > 0 && info.consumable)
            {
                slot.Activate();
                return;
            }
        }
    }

    #region World Effects

    private Color GetBlastColorForEffect(StatusEffect effectType)
    {
        switch (effectType)
        {
            case StatusEffect.Reach:
                return new Color(1, 0.5f, 0); // orange
            case StatusEffect.Healing:
                return new Color(1, 0, 0.2f);
            case StatusEffect.Speedy:
                return Color.green;
            default:
                return Color.white;
        }
    }

    public void PlayBombEffect(BombWorldEffect bombEffect)
    {
        Vector2 start;
        if (bombEffect.exactPosition)
            start = bombEffect.position.ToVector2();
        else
        {
            if (!TryGetObject(bombEffect.positionGameId, out var obj))
                start = bombEffect.target.ToVector2();
            else
                start = obj.transform.localPosition;
        }

        var bomb = (Bomb)PlayEffect(EffectType.Bomb, start);
        bomb.SetInfo(Color.yellow, start, bombEffect.target.ToVector2(), bombEffect.time);
    }

    public void PlayBombBlastEffect(BombBlastWorldEffect bombEffect)
    {
        Vector2 start;
        if (bombEffect.exactPosition)
            start = bombEffect.position.ToVector2();
        else
        {
            if (!TryGetObject(bombEffect.positionGameId, out var obj))
                start = bombEffect.target.ToVector2();
            else
                start = obj.transform.localPosition;
        }

        var bomb = (Bomb)PlayEffect(EffectType.Bomb, start);
        bomb.SetInfo(Color.yellow, start, bombEffect.target.ToVector2(), bombEffect.time);
        bomb.AddBlast(Color.yellow, bombEffect.area);
    }

    public void PlayLevelUpEffect(LevelUpWorldEffect levelUpEffect)
    {
        if (!TryGetObject(levelUpEffect.gameId, out var worldObject)) return;
        worldObject.PlayLevelUpEffect();
    }

    public void PlayHealLaserEffect(HealLaserWorldEffect healLaserEffect)
    {
        if (!TryGetObject(healLaserEffect.sourceGameId, out var source)) return;
        if (!TryGetObject(healLaserEffect.targetGameId, out var target)) return;
        var sourcePosition = source.transform.localPosition;
        sourcePosition.z = -source.GetHeight();

        var targetPosition = target.transform.localPosition;
        targetPosition.z = -target.GetHeight() / 2;

        var effect = (HealLaser)PlayEffect(EffectType.HealLaser, sourcePosition);
        effect.SetTarget(targetPosition);
    }

    public void PlayWarriorAbilityEffect(WarriorAbilityWorldEffect warriorEffect)
    {
        if (!TryGetObject(warriorEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        var pos = ownerCharacter.Position;
        pos.z = 0;

        PlayEffect(EffectType.WarriorAbility, pos);
    }

    public void PlayAlchemistAbilityEffect(AlchemistAbilityWorldEffect alchemistEffect)
    {
        if (!TryGetObject(alchemistEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        float radius = AbilityFunctions.Alchemist.GetRadius(alchemistEffect.rage);

        var healthBomb = (Bomb)PlayEffect(EffectType.Bomb, (Vector2)ownerCharacter.Position);
        healthBomb.SetInfo(Color.magenta, ownerCharacter.Position, alchemistEffect.target.ToVector2(), AbilityFunctions.Alchemist.Air_Time);
        healthBomb.AddBlast(Color.magenta, radius);
        healthBomb.SetEndCallback((bomb) =>
        {
            var alchemistAbility = (AlchemistAbility)PlayEffect(EffectType.AlchemistAbility, alchemistEffect.target.ToVector2());
            alchemistAbility.Setup(alchemistEffect.target.ToVector2(), radius, AbilityFunctions.Alchemist.GetGroundDurationMs(alchemistEffect.rage) / 1000f);
        });
    }

    public void PlayCommanderAbilityEffect(CommanderAbilityWorldEffect commanderEffect)
    {
        PlayEffect(EffectType.CommanderAbility, commanderEffect.position.ToVector2());

        var effects = AbilityFunctions.GetAbilityEffects(commanderEffect.rage, commanderEffect.attack, 0, ClassType.Commander);

        foreach (var abilityEffect in effects)
        {
            if (abilityEffect.area == 0) continue;
            var blast = (AreaBlast)PlayEffect(EffectType.AreaBlast, commanderEffect.position.ToVector2());
            blast.SetInfo(abilityEffect.area, GetBlastColorForEffect(abilityEffect.type));
        }

        if (!TryGetObject(commanderEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        foreach (var effect in effects)
        {
            ownerCharacter.AddClientEffect(effect.type, effect.duration);
        }
    }

    public void PlayMinisterAbilityEffect(MinisterAbilityWorldEffect ministerEffect)
    {
        var effects = AbilityFunctions.GetAbilityEffects(ministerEffect.rage, ministerEffect.attack, 0, ClassType.Minister);

        foreach (var abilityEffect in effects)
        {
            if (abilityEffect.area == 0) continue;
            var blast = (AreaBlast)PlayEffect(EffectType.AreaBlast, ministerEffect.position.ToVector2());
            blast.SetInfo(abilityEffect.area, GetBlastColorForEffect(abilityEffect.type));
        }

        if (!TryGetObject(ministerEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        foreach (var effect in effects)
        {
            ownerCharacter.AddClientEffect(effect.type, effect.duration);
        }
    }

    public void PlayBerserkerAbilityEffect(BerserkerAbilityWorldEffect berserkerEffect)
    {
        PlayEffect(EffectType.BerserkerAbility, berserkerEffect.position.ToVector2());

        var shoutSpread = AbilityFunctions.Berserker.GetShoutSpread(berserkerEffect.rage, berserkerEffect.attack) * AngleUtils.Rad2Deg;
        var shoutRadius = AbilityFunctions.Berserker.GetShoutRange(berserkerEffect.rage, berserkerEffect.attack);

        var shout = (Shout)PlayEffect(EffectType.Shout, berserkerEffect.position.ToVector2());
        shout.SetInfo(shoutSpread, berserkerEffect.angle, shoutRadius);

        var effects = AbilityFunctions.GetAbilityEffects(berserkerEffect.rage, berserkerEffect.attack, 0, ClassType.Berserker);

        foreach (var abilityEffect in effects)
        {
            if (abilityEffect.area == 0) continue;
            var blast = (AreaBlast)PlayEffect(EffectType.AreaBlast, berserkerEffect.position.ToVector2());
            blast.SetInfo(abilityEffect.area, GetBlastColorForEffect(abilityEffect.type));
        }

        if (!TryGetObject(berserkerEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        foreach (var effect in effects)
        {
            ownerCharacter.AddClientEffect(effect.type, effect.duration);
        }
    }

    public void PlayRangerAbilityEffect(RangerAbilityWorldEffect rangerEffect)
    {
        PlayEffect(EffectType.RangerArrows, rangerEffect.position.ToVector2());

        var rangerRadius = AbilityFunctions.Ranger.GetRadius(rangerEffect.rage, rangerEffect.attack);
        var rangerDamage = AbilityFunctions.Ranger.GetDamage(rangerEffect.rage, rangerEffect.attack);

        Vector3 position = rangerEffect.position.ToVector2();
        position.z = -4;

        var arrows = (RangerArrows)PlayEffect(EffectType.RangerArrows, position);
        arrows.SetInfo(rangerRadius);

        foreach (var hit in rangerEffect.hit)
        {
            if (!TryGetObject(hit, out var worldObject)) continue;
            if (!(worldObject is Enemy enemy)) continue;
            worldObject.ShowAlert("-" + enemy.GetDamageTaken(rangerDamage), Color.red);
        }
    }

    public void PlayBrewerAbilityEffect(BrewerAbilityWorldEffect brewerEffect)
    {
        var effects = AbilityFunctions.GetAbilityEffects(brewerEffect.rage, brewerEffect.attack, brewerEffect.value, ClassType.Brewer);

        foreach (var abilityEffect in effects)
        {
            if (abilityEffect.area == 0) continue;
            var blast = (AreaBlast)PlayEffect(EffectType.AreaBlast, brewerEffect.position.ToVector2());
            blast.SetInfo(abilityEffect.area, GetBlastColorForEffect(abilityEffect.type));
        }

        if (!TryGetObject(brewerEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        foreach (var effect in effects)
        {
            ownerCharacter.AddClientEffect(effect.type, effect.duration);
        }
    }

    public void PlayBladeweaverAbilityEffect(BladeweaverAbilityWorldEffect bladeweaverEffect)
    {
        if (!TryGetObject(bladeweaverEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        ownerCharacter.AddClientEffect(StatusEffect.Dashing, AbilityFunctions.BladeWeaver.Dash_Duration);
    }

    public void PlayNomadAbilityEffect(NomadAbilityWorldEffect nomadEffect)
    {
        if (!TryGetObject(nomadEffect.ownerGameId, out var worldObject)) return;
        if (!(worldObject is Character ownerCharacter)) return;

        var bomb = (Bomb)PlayEffect(EffectType.Bomb, (Vector2)ownerCharacter.Position);
        bomb.SetInfo(Color.green, ownerCharacter.Position, nomadEffect.target.ToVector2(), AbilityFunctions.Nomad.Charm_Air_Time);
    }

    #endregion
}
