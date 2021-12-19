using UnityEngine;
using System.Collections;
using UnityEngine.U2D;
using TitanCore.Data;
using System.IO;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;
using Utils.NET.Net.Udp;
using TitanCore.Net.Packets;
using Utils.NET.Net.Tcp;
using UnityEngine.SceneManagement;
using TitanCore.Net.Web;
using TitanCore.Net;

public class GameManager : MonoBehaviour
{
    private enum WorldLoadState
    {
        FindingServer,
        AwaitingConnection,
        Connected,
        AwaitingMapInfo,
        ReceivedMapInfo,
        Loaded,
        Disconnected
    }

    public static ulong characterToLoad = 0;

    public static ushort characterToCreate = 0;

    public bool mapEditor = false;

    /// <summary>
    /// The client connected to the server
    /// </summary>
    public Client client;

    /// <summary>
    /// The game's ui
    /// </summary>
    public GameUI ui;

    /// <summary>
    /// The game world
    /// </summary>
    public World world;

    /// <summary>
    /// Manager of object creation and re-use
    /// </summary>
    public ObjectManager objectManager;

    /// <summary>
    /// The current loading state of the world
    /// </summary>
    private WorldLoadState loadState = WorldLoadState.FindingServer;

    /// <summary>
    /// The map info received from the server
    /// </summary>
    private TnMapInfo mapInfo;

    private TnError error;

    public bool ClientReady => loadState != WorldLoadState.AwaitingConnection && loadState != WorldLoadState.Disconnected && loadState != WorldLoadState.FindingServer;

    private string worldName = "Nexus";

    private uint worldId = 1;

    private ulong worldKey = 0;

    private ServerPinger pinger;

    private WebServerInfo selectedServer;

    private Option vsync;

    private Option targetFramerate;

    public GameObject shopMenu;

    private void Awake()
    {
        vsync = Options.Get(OptionType.Vsync);
        vsync.AddBoolCallback(OnVsync);
        OnVsync(vsync.GetBool());

        targetFramerate = Options.Get(OptionType.TargetFramerate);
        targetFramerate.AddIntCallback(OnTargetFramerate);
        OnTargetFramerate(targetFramerate.GetInt());
    }

    private void OnVsync(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
    }

    private void OnTargetFramerate(int value)
    {
        var framerate = (TargetFramerate)value;
        switch (framerate)
        {
            case TargetFramerate.Thirty:
                Application.targetFrameRate = 30;
                break;
            case TargetFramerate.Sixty:
                Application.targetFrameRate = 60;
                break;
            case TargetFramerate.OneTwenty:
                Application.targetFrameRate = 120;
                break;
            case TargetFramerate.OneFortyFour:
                Application.targetFrameRate = 144;
                break;
            default:
                Application.targetFrameRate = -1;
                break;
        }
    }

    private void Start()
    {
        if (mapEditor) return;

        worldId = 1;
        worldKey = 0;

        var selectedServer = Account.GetSelectedServer();
        if (selectedServer != null)
        {
            StartWithServer(selectedServer);
        }
        else
        {
            pinger = new ServerPinger();
        }
    }

    private void StartWithServer(WebServerInfo server)
    {
        world.GameChat("Build " + NetConstants.Build_Version, ChatType.ClientInfo);

        worldName = server.name;
        selectedServer = server;
        CreateClient(server.host);
    }

    private void CreateClient(string host)
    {
        loadState = WorldLoadState.AwaitingConnection;

        client = new Client(host, world);
        client.SetDisconnectCallback(OnDisconnect);
        client.AddHandler<TnMapInfo>(OnMapInfo);
        client.AddHandler<TnReconnect>(OnReconnect);
        client.AddHandler<TnCreateResponse>(OnCreateResponse);
        client.AddHandler<TnError>(OnError);
        client.Connect(OnClientConnect);

        world.SetupClient();
    }

    private void OnDisconnect(NetConnection<TnPacket> connection)
    {
        if (client != connection) return;
        Debug.Log("Disconnected");
        loadState = WorldLoadState.Disconnected;
    }

    private void OnCreateResponse(TnCreateResponse createResponse)
    {
        characterToLoad = createResponse.characterId;
    }

    private void OnClientConnect(bool success)
    {
        if (!success)
        {

            return;
        }

        loadState = WorldLoadState.Connected;
    }

    private void CheckLoadState()
    {
        switch (loadState)
        {
            case WorldLoadState.FindingServer:
                if (mapEditor) return;
                pinger.Update();
                if (!pinger.isDone) return;
                StartWithServer(pinger.bestServer);
                break;
            case WorldLoadState.Connected:
                SendHello();
                loadState = WorldLoadState.AwaitingMapInfo;
                break;
            case WorldLoadState.ReceivedMapInfo:
                world.NewMap(mapInfo);
                loadState = WorldLoadState.Loaded;

#if UNITY_STANDALONE
                GameDataLoader.discordManager.UpdateState($"{selectedServer.name}, {mapInfo.worldName}");
#endif

                world.GameChat("Connected!", ChatType.ClientInfo);
                break;
            case WorldLoadState.Disconnected:
                SceneManager.LoadScene("MenuScene");
                break;
        }
    }

    public void SendHello()
    {
        var accessToken = Account.loggedInAccessToken;
        if (string.IsNullOrWhiteSpace(accessToken))
            accessToken = Account.savedAccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.LogError("No access token available!");

            return;
        }
        var rsa = Client.RsaEncrypt(accessToken);

        var hello = new TnHello(rsa, characterToLoad, characterToCreate, worldId, worldKey);
        characterToCreate = 0;
        client.SendAsync(hello);

        world.GameChat("Connecting to " + worldName, ChatType.ClientInfo);
    }

    private void OnMapInfo(TnMapInfo mapInfo)
    {
        this.mapInfo = mapInfo;
        loadState = WorldLoadState.ReceivedMapInfo;
        Debug.Log("Received MapInfo");
    }

    private void OnReconnect(TnReconnect reconnect)
    {
        Disconnect();

        worldName = reconnect.name;
        worldKey = reconnect.key;
        worldId = reconnect.worldId;

        loadState = WorldLoadState.AwaitingConnection;
        CreateClient(reconnect.host);
    }

    private void OnError(TnError error)
    {
        Disconnect();

        this.error = error;
    }

    private void Disconnect()
    {
        if (mapEditor) return;

        client.SetDisconnectCallback(null);
        client.Disconnect();
        client.ClearHandlers();
    }

    private void Update()
    {
        CheckLoadState();

        if (error != null)
        {
            ApplicationAlert.Show("Uh oh.", error.message, _ =>
            {
                SceneManager.LoadScene("MenuScene");
            }, "Menu");
            error = null;
        }
    }

    public void OnApplicationQuit()
    {
        client?.Disconnect();
    }

    private void OnApplicationPause(bool pause)
    {
        if (mapEditor) return;
        if (!pause)
        {
            if (shopMenu != null && shopMenu.activeSelf) return;

            Disconnect();

            SceneManager.LoadScene("MenuScene");
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        
    }

    private void OnDestroy()
    {
        vsync.RemoveBoolCallback(OnVsync);
        targetFramerate.RemoveIntCallback(OnTargetFramerate);

        if (client == null) return;

        client.SetDisconnectCallback(null);
        client.Disconnect();
        client.ClearHandlers();
    }
}
