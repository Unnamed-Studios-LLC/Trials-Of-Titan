using UnityEngine;
using System.Collections;
using TitanCore.Data;
using System.Collections.Generic;
using TitanCore.Core;

public class ObjectManager : MonoBehaviour
{
    /// <summary>
    /// The game manager
    /// </summary>
    public GameManager gameManager;

    /// <summary>
    /// The parent transform for chat bubbles
    /// </summary>
    public RectTransform chatBubbleParent;

    /// <summary>
    /// The parent transform for alerts
    /// </summary>
    public Transform alertParent;

    /// <summary>
    /// Prefab of all world objects
    /// </summary>
    public GameObject[] objectPrefabs;

    /// <summary>
    /// Prefab for a ObjectShadow
    /// </summary>
    public GameObject shadowPrefab;

    /// <summary>
    /// Object pool for shadows
    /// </summary>
    private ObjectPool<ObjectShadow> shadowPool;

    /// <summary>
    /// Prefab for a game projectile
    /// </summary>
    public GameObject projectilePrefab;

    /// <summary>
    /// Object pool for projectiles
    /// </summary>
    private ObjectPool<Projectile> projectilePool;

    /// <summary>
    /// Prefab for a health bar
    /// </summary>
    public GameObject healthBarPrefab;

    /// <summary>
    /// Object pool for health bars
    /// </summary>
    private ObjectPool<WorldHealthBar> healthBarPool;

    /// <summary>
    /// Prefab for ground label objects
    /// </summary>
    public GameObject groundLabelPrefab;

    /// <summary>
    /// Object pool for ground labels
    /// </summary>
    private ObjectPool<GroundLabel> groundLabelPool;

    /// <summary>
    /// Prefab for the in-game chat bubbles
    /// </summary>
    public GameObject chatBubblePrefab;

    /// <summary>
    /// Prefab for in-game alerts
    /// </summary>
    public GameObject alertPrefab;

    /// <summary>
    /// Object pool for alerts
    /// </summary>
    private ObjectPool<Alert> alertPool;

    /// <summary>
    /// Prefab for status effect sprite
    /// </summary>
    public GameObject statusEffectSpritePrefab;

    /// <summary>
    /// Object pool for status effect sprites
    /// </summary>
    private ObjectPool<SpriteRenderer> statusEffectSpritePool;

    /// <summary>
    /// All world object prefabs keyed to their represented GameObjectType
    /// </summary>
    private Dictionary<GameObjectType, ObjectPool<WorldObject>> prefabs = new Dictionary<GameObjectType, ObjectPool<WorldObject>>();

    private void Awake()
    {
        SortPrefabs();

        CreatePools();
    }

    /// <summary>
    /// Sorts the array of prefabs into the prefab dictionary
    /// </summary>
    private void SortPrefabs()
    {
        foreach (var prefab in objectPrefabs)
        {
            var worldObject = prefab.GetComponent<WorldObject>();
            prefabs.Add(worldObject.ObjectType, new ObjectPool<WorldObject>(0, prefab));
        }
    }

    private void CreatePools()
    {
        shadowPool = new ObjectPool<ObjectShadow>(0, shadowPrefab);
        projectilePool = new ObjectPool<Projectile>(0, projectilePrefab);
        alertPool = new ObjectPool<Alert>(0, alertPrefab);
        groundLabelPool = new ObjectPool<GroundLabel>(0, groundLabelPrefab);
        statusEffectSpritePool = new ObjectPool<SpriteRenderer>(0, statusEffectSpritePrefab);
        healthBarPool = new ObjectPool<WorldHealthBar>(0, healthBarPrefab);
    }

    /// <summary>
    /// Creates a player object from a given object info
    /// </summary>
    public Player GetPlayer(GameObjectInfo info)
    {
        var player = (Player)GetObject(GameObjectType.Player);
        player.LoadObjectInfo(info);
        player.Enable();
        return player;
    }

    /// <summary>
    /// Returns a world object representing the given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool TryGetObject(ushort type, out WorldObject worldObject)
    {
        if (!GameData.objects.TryGetValue(type, out var info))
        {
            worldObject = null;
            return false;
        }
        return TryGetObject(info, out worldObject);
    }

    /// <summary>
    /// Returns a world object representing the given info type
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public bool TryGetObject(GameObjectInfo info, out WorldObject worldObject)
    {
        if (!prefabs.ContainsKey(info.Type))
        {
            worldObject = null;
            return false;
        }

        worldObject = GetObject(info.Type);
        worldObject.LoadObjectInfo(info);
        worldObject.Enable();
        return true;
    }

    /// <summary>
    /// Creates an object from a given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private WorldObject GetObject(GameObjectType type)
    {
        var worldObject = prefabs[type].Get();//Instantiate(prefabs[type]).GetComponent<WorldObject>();
        worldObject.transform.SetParent(gameManager.world.transform);
        worldObject.world = gameManager.world;
        return worldObject;
    }

    /// <summary>
    /// Returns a world object to the manager to be recycled
    /// </summary>
    /// <param name="worldObject"></param>
    public void ReturnObject(WorldObject worldObject)
    {
        worldObject.Disable();
        prefabs[worldObject.ObjectType].Return(worldObject);
        //Destroy(worldObject.gameObject);
    }

    /// <summary>
    /// Gets a new shadow for a given transform to follow
    /// </summary>
    /// <param name="toFollow"></param>
    /// <returns></returns>
    public ObjectShadow GetShadow(Transform toFollow)
    {
        //var shadow = Instantiate(shadowPrefab).GetComponent<ObjectShadow>();
        var shadow = shadowPool.Get();
        shadow.Enable();
        shadow.SetToFollow(toFollow);
        shadow.transform.SetParent(gameManager.world.transform);
        return shadow;
    }

    /// <summary>
    /// Returns a shadow object
    /// </summary>
    /// <param name="shadow"></param>
    public void ReturnShadow(ObjectShadow shadow)
    {
        shadow.Disable();
        shadowPool.Return(shadow);
        //Destroy(shadow.gameObject);
    }

    /// <summary>
    /// Gets a projectile
    /// </summary>
    /// <returns></returns>
    public Projectile GetProjectile()
    {
        var proj = projectilePool.Get();//Instantiate(projectilePrefab).GetComponent<Projectile>();
        proj.transform.SetParent(gameManager.world.transform);
        proj.world = gameManager.world;
        gameManager.world.AddProjectile(proj);
        return proj;
    }

    /// <summary>
    /// Returns a projectile
    /// </summary>
    public void ReturnProjectile(Projectile projectile)
    {
        gameManager.world.RemoveProjectile(projectile);
        projectilePool.Return(projectile);
        //Destroy(projectile.gameObject);
    }

    /// <summary>
    /// Gets a ground label
    /// </summary>
    /// <returns></returns>
    public GroundLabel GetGroundLabel(WorldObject toFollow, string text)
    {
        var label = groundLabelPool.Get();
        label.Init(toFollow, text);
        return label;
    }

    /// <summary>
    /// Returns a ground label
    /// </summary>
    public void ReturnGroundLabel(GroundLabel label)
    {
        groundLabelPool.Return(label);
    }

    public ChatBox GetChatBubble(WorldObject worldObject)
    {
        var bubble = Instantiate(chatBubblePrefab).GetComponent<ChatBox>();
        bubble.SetGameView(chatBubbleParent);
        bubble.transform.SetParent(chatBubbleParent);
        bubble.owner = worldObject;
        return bubble;
    }

    public void ReturnChatBubble(ChatBox bubble)
    {
        Destroy(bubble.gameObject);
    }

    public WorldHealthBar GetHealthBar(Character character)
    {
        var bar = healthBarPool.Get();
        bar.transform.SetParent(gameManager.world.transform);
        bar.owner = character;
        return bar;
    }

    public void ReturnHealthBar(WorldHealthBar bar)
    {
        healthBarPool.Return(bar);
    }

    public Alert GetAlert(WorldObject worldObject, string text, Color color, bool statusEffect)
    {
        var alert = alertPool.Get();//Instantiate(alertPrefab).GetComponent<Alert>();
        alert.transform.SetParent(alertParent);
        alert.Init(gameManager.world, worldObject, text, color, this, statusEffect);
        return alert;
    }

    public void ReturnAlert(Alert alert)
    {
        alertPool.Return(alert);
        //Destroy(alert.gameObject);
    }

    public SpriteRenderer GetStatusEffectSprite(StatusEffects effects, StatusEffect effect)
    {
        var effectSprite = statusEffectSpritePool.Get();
        effectSprite.transform.SetParent(effects.transform);
        effectSprite.sprite = TextureManager.GetStatusEffect(effect);
        effectSprite.transform.localScale = Vector3.one;
        effectSprite.transform.localEulerAngles = Vector3.zero;
        return effectSprite;
    }

    public void ReturnStatusEffectSprite(SpriteRenderer sprite)
    {
        sprite.transform.SetParent(null);
        statusEffectSpritePool.Return(sprite);
    }
}
