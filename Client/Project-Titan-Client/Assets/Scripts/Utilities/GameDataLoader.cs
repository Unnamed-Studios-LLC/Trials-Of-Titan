#if UNITY_STANDALONE
using Steamworks;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Iap;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;

public class GameDataLoader : MonoBehaviour
{
#if UNITY_STANDALONE

    public static DiscordManager discordManager;

#endif

    /// <summary>
    /// All game sprites
    /// </summary>
    public SpriteAtlas[] spriteAtlases;

    /// <summary>
    /// All game meshes
    /// </summary>
    public Mesh[] meshes;

    public TextAsset[] xmls;

    private Dictionary<string, TextAsset> xmlMap = new Dictionary<string, TextAsset>();

    public Sprite[] uiSprites;

    public Texture2D meshTexture;

    private void Awake()
    {
        bool isOther = FindObjectsOfType<GameDataLoader>().Any(_ => _ != this);
        if (isOther)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        xmlMap = xmls.ToDictionary(_ => _.name);
        LoadGameData();
        MeshManager.Init(meshTexture, meshes);
        TextureManager.Init(spriteAtlases);
        TextureManager.SetUISprites(uiSprites);
        AnimationManager.Init();

#if UNITY_STANDALONE

        discordManager = new DiscordManager();
        discordManager.Init();

        if (Constants.Store_Type == StoreType.Steam)
        {
            try
            {
                SteamClient.Init(949430);
            }
            catch (Exception)
            {
                // Couldn't init for some reason (steam is closed etc)
            }
        }

#endif
    }

    private void LoadGameData()
    {
        //BetterStreamingAssets.Initialize();
        GameData.ClearObjects();

        Debug.Log("Loading game data...");
        GameData.LoadFiles(xmlMap.Keys, LoadResource);
        Debug.Log($"Loaded {GameData.objects.Count} objects");
    }

    private Stream LoadResource(string name)
    {
        var asset = xmlMap[name];
        return new MemoryStream(asset.bytes);
    }

    /*
#if UNITY_ANDROID

    private static string[] assets = new string[]
    {
        "/data/characters.xml",
        "/data/enemies.xml",
        "/data/items.xml",
        "/data/lootbags.xml",
        "/data/pets.xml",
        "/data/projectiles.xml",
        "/data/skins.xml",
        "/data/staticobjects.xml",
        "/data/tiles.xml",
    };


    private void LoadGameData()
    {
        //BetterStreamingAssets.Initialize();
        GameData.ClearObjects();

        Debug.Log("Loading game data...");
        GameData.LoadFiles(assets, ReadAndroid);
        Debug.Log($"Loaded {GameData.objects.Count} objects");
    }

    private Stream ReadAndroid(string path)
    {
        path = Application.streamingAssetsPath + path;
        var webRequest = UnityWebRequest.Get(path);
        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                break;
            }
        }

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(path);
            Debug.LogError(webRequest.error);
            return null;
        }
        else
        {
            var data = webRequest.downloadHandler.data;
            return new MemoryStream(data);
        }
    }
#else
    private void LoadGameData()
    {
        GameData.ClearObjects();

        Debug.Log("Loading game data...");
        var streamingPath = Application.streamingAssetsPath;
        GameData.LoadDirectory(Path.Combine(Application.streamingAssetsPath, "data"));
        Debug.Log($"Loaded {GameData.objects.Count} objects");
    }
#endif
*/

#if UNITY_STANDALONE

    private void Update()
    {
        discordManager.Update();

        if (Constants.Store_Type == StoreType.Steam)
        {
            SteamClient.RunCallbacks();
        }
    }

    private void OnApplicationQuit()
    {
        discordManager.OnApplicationQuit();

        if (Constants.Store_Type == StoreType.Steam)
        {
            SteamClient.Shutdown();
        }
    }

#endif
}
