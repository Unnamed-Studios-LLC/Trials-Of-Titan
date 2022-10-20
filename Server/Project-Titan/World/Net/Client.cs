using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TitanCore.Net.Packets;
using TitanCore.Net.Packets.Client;
using TitanCore.Net.Packets.Server;
using Utils.NET.Crypto;
using Utils.NET.IO;
using Utils.NET.Logging;
using Utils.NET.Net.Udp;
using Utils.NET.Net.Udp.Reliability;
using TitanDatabase;
using TitanDatabase.Models;
using World.Map.Objects.Entities;
using World.Net.Handling;
using static TitanDatabase.Models.Model;
using Utils.NET.Net.Tcp;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Models;
using TitanCore.Core;
using Utils.NET.Utils;
using TitanDatabase.Leaderboards;
using Utils.NET.Modules;
using TitanCore.Data.Items;
using TitanCore.Net;
using Utils.NET.Collections;

namespace World.Net
{
    public class Client : NetConnection<TnPacket> // : UdpClient<TnPacket>
    {
        public const int Client_Fixed_Delta = 16;

        /// <summary>
        /// The time interval, in milliseconds, between server to client pings
        /// </summary>
        private const int Ping_Interval_Milliseconds = 3000;

        /// <summary>
        /// The time interval, in milliseconds, before timing out the connection from not receiving pongs
        /// </summary>
        private const int Pong_Timeout_Milliseconds = 8000;

        /// <summary>
        /// The time interval to send lock heartbeats at
        /// </summary>
        private const double Heartbeat_Interval_Seconds = 30;

        /// <summary>
        /// The max amount of attempts to logout
        /// </summary>
        private const int Logout_Attempts = 2;

        #region Locks and Access Control

        private int loginAccess = 0;

        private int transferAccess = 0;

        #endregion

        #region Spam Checking

        public SpamChecker chatSpamRegulator = new SpamChecker(6, 4, new Range(3, 9), 3);

        public SpamChecker emoteSpamRegulator = new SpamChecker(8, 2, new Range(2, 6), 2);

        #endregion

        /// <summary>
        /// Rsa decryption used to read client secrets
        /// </summary>
        private static Rsa rsa = new Rsa(Database.Rsa_Private_Key, true);

        /// <summary>
        /// Timer used to ping the client's connection
        /// </summary>
        private Timer pingTimer;

        /// <summary>
        /// The time of the last received pong 
        /// </summary>
        public DateTime lastPong = DateTime.UtcNow;

        public ConcurrentQueue<DateTime> pings = new ConcurrentQueue<DateTime>();

        public double ping = 50;

        /// <summary>
        /// The next time to send a lock heartbeat
        /// </summary>
        private DateTime nextLockHeartbeat;

        /// <summary>
        /// The account logged into by the client
        /// </summary>
        public Account account;

        /// <summary>
        /// The client manager that own this client
        /// </summary>
        private ClientManager manager;

        /// <summary>
        /// The player that this client controls
        /// </summary>
        public Player player;

        private TimeChecker timeChecker;

        /// <summary>
        /// Queue for packets that need to be processed during a tick
        /// </summary>
        private ConcurrentQueue<TnPacket> tickPacketQueue = new ConcurrentQueue<TnPacket>();

        private string accessTokenEncrypt;

        #region Init

        public Client(Socket socket) : base(socket)
        {
            maxReceiveSize = 20_000;
        }

        public Client() : base()
        {
        }

        public void SetManager(ClientManager manager)
        {
            this.manager = manager;
            StartPingTimer();
        }

        #endregion

        #region Connection Methods

        protected override void OnDisconnect()
        {
            if (player != null)
                player.world?.LogoutPlayer(player, null);
            else
                Logout(null);
            StopPing();
        }

        protected override void HandlePacket(TnPacket packet)
        {
            if (account == null && packet.Type != TnPacketType.Hello && packet.Type != TnPacketType.Ping)
                return;

            if (timeChecker != null && packet is ITimePacket timePacket)
            {
                if (!timeChecker.ValidTimeAdvance(timePacket.GetTime(), ping))
                {
                    //SendAsync(new TnError("Time check invalid"));
                    Disconnect();
                    return;
                }
            }

            switch (packet.Type)
            {
                case TnPacketType.Ping:
                case TnPacketType.Pong:
                case TnPacketType.Hello:
                    ClientPacketHandler.HandlePacket(packet, this);
                    break;
                default:
                    tickPacketQueue.Enqueue(packet);
                    break;
            }
        }

        public void ProcessTickPackets()
        {
            while (tickPacketQueue.TryDequeue(out var packet))
            {
                if (!player.startTick && packet.Type != TnPacketType.StartTick) continue;
                ClientPacketHandler.HandlePacket(packet, this);
            }
        }

        #endregion

        #region Ping

        /// <summary>
        /// Starts the timer used to ping the client's connection
        /// </summary>
        private void StartPingTimer()
        {
            pingTimer = new Timer(DoPing, null, Ping_Interval_Milliseconds, Ping_Interval_Milliseconds);
        }

        /// <summary>
        /// Sends the ping packet to the client
        /// </summary>
        /// <param name="state"></param>
        private async void DoPing(object state)
        {
            pings.Enqueue(DateTime.UtcNow);
            SendAsync(new TnPing());

            if ((DateTime.UtcNow - lastPong).TotalMilliseconds > Pong_Timeout_Milliseconds)
            {
                Disconnect();
            }
            else if (DateTime.UtcNow > nextLockHeartbeat)
            {
                nextLockHeartbeat = DateTime.UtcNow.AddSeconds(Heartbeat_Interval_Seconds);
                if (account == null || loginAccess != 0) return;
                if (!await Database.HeartbeatLock(account, WorldModule.ServerName))
                    Disconnect();
            }
        }

        /// <summary>
        /// Stops the ping timer from executing
        /// </summary>
        private void StopPing()
        {
            pingTimer?.Dispose();
        }

        #endregion

        #region Account

        private string DecryptString(string encrypted)
        {
            var data = Convert.FromBase64String(encrypted);
            var decryptedData = rsa.Decrypt(data);
            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Logs into the account
        /// </summary>
        /// <param name="accessToken"></param>
        public async Task<bool> Login(TnHello hello)
        {
            if (Interlocked.CompareExchange(ref loginAccess, 1, 0) != 0) return false;

            try
            {
                if (account != null) return false;

                accessTokenEncrypt = hello.accessToken;
                var accessToken = DecryptString(hello.accessToken);
                var loginResponse = await Database.Login(accessToken, WorldModule.ServerName);
                if (loginResponse.result != LoginResult.Success)
                {
                    Log.Write("Failed to login: " + loginResponse.result);
                    switch (loginResponse.result)
                    {
                        case LoginResult.AccountInUse:
                            SendAsync(new TnError("Account in use"));
                            break;
                        case LoginResult.TokenNotFound:
                            Disconnect();
                            break;
                        case LoginResult.AwsServerError:
                            Disconnect();
                            break;
                    }
                    return false;
                }

                account = loginResponse.account;

                if (account.bannedUntil > DateTime.UtcNow)
                {
                    var timeDif = account.bannedUntil - DateTime.UtcNow;
                    string timeString;
                    if (timeDif.TotalDays > 1)
                        timeString = (int)timeDif.TotalDays + " days";
                    else if (timeDif.TotalHours > 1)
                        timeString = (int)timeDif.TotalHours + " hours";
                    else
                        timeString = (int)timeDif.TotalMinutes + " minutes";

                    await AccountLock.Delete(account.id, WorldModule.ServerName);

                    SendAsync(new TnError($"Your account has been banned. The ban lifts in " + timeString));
                    account = null;
                    return false;
                }

                /*
                if (!account.playerName.Equals("Juix"))
                {
                    await AccountLock.Delete(account.id, WorldModule.ServerName);
                    SendAsync(new TnError("Testing is currently locked to Juix-only"));
                    account = null;
                    return false;
                }
                */

                if (hello.createType == 0 && hello.characterId == 0)
                {
                    if (account.characters.Count > 0)
                        hello.characterId = account.characters[0];
                    else
                    {
                        Disconnect();
                        return false;
                    }
                }

                Character character = null;
                if (hello.characterId > 0)
                {
                    var characterResponse = await Character.Get(hello.characterId);
                    if (characterResponse.result != RequestResult.Success)
                    {
                        switch (characterResponse.result)
                        {
                            case RequestResult.ResourceNotFound:
                                Disconnect();
                                return false;
                            default:
                                Disconnect();
                                return false;
                        }
                    }
                    character = characterResponse.item;
                    character.CheckItemContainerIds();

                    if (character.accountId != account.id || character.dead)
                    {
                        Disconnect();
                        return false;
                    }
                }
                else if (hello.createType > 0)
                {
                    var createResponse = await Database.CreateCharacter(account, hello.createType);
                    if (createResponse.result != CreateCharacterResult.Success)
                    {
                        Disconnect();
                        return false;
                    }

                    character = createResponse.character;

                    SendAsync(new TnCreateResponse(character.id));
                }

                Log.Write("Successfully logged in: " + account.playerName);
                nextLockHeartbeat = DateTime.UtcNow.AddSeconds(Heartbeat_Interval_Seconds);

                if (!manager.module.worldManager.TryGetWorld(hello.worldId, out var world))
                {
                    Disconnect();
                    return false;
                }

                ushort characterType = hello.createType;
                if (!world.TryEnter(hello.worldAccessToken, character, this, out player))
                {
                    Disconnect();
                    return false;
                }

                tickPacketQueue = new ConcurrentQueue<TnPacket>();
            }
            finally
            {
                loginAccess = 0;

                if (Disconnected)
                    OnDisconnect();
            }
            return true;
        }

        /// <summary>
        /// Logs the player out of their account and saves all player data
        /// </summary>
        public async void Logout(Action<bool> callback)
        {
            if (Interlocked.CompareExchange(ref loginAccess, 1, 0) != 0)
            {
                callback?.Invoke(false);
                return;
            }

            try
            {
                await DoLogout();
            }
            finally
            {
                loginAccess = 0;
                callback?.Invoke(true);
            }
        }

        /// <summary>
        /// Logs the player out of their account and saves all player data
        /// </summary>
        public async Task<bool> Logout()
        {
            if (Interlocked.CompareExchange(ref loginAccess, 1, 0) != 0) return false;
            try
            {
                await DoLogout();
            }
            finally
            {
                loginAccess = 0;
            }
            return true;
        }

        private async Task DoLogout(int attempt = 1)
        {
            if (account == null) return;

            if (player != null)
            {
                player.RemoveFromWorld();
                player.SaveCharacter();

                await Database.SaveItems(player.character.items);

                if (!player.character.dead)
                    LeaderboardManager.PushLiving(player.character);

                var saveResponse = await player.character.Put();
                if (saveResponse.result != RequestResult.Success)
                {
                    Log.Write("Failed to save character: " + saveResponse.result);
                    if (attempt < Logout_Attempts)
                    {
                        await DoLogout(attempt + 1);
                    }
                    return;
                }

                player = null;
            }

            var logoutResponse = await Database.Logout(account, WorldModule.ServerName);
            if (logoutResponse.result != LogoutResult.Success)
            {
                Log.Write("Failed to logout: " + logoutResponse.result);
                if (attempt < Logout_Attempts)
                {
                    await DoLogout(attempt + 1);
                }
                return;
            }

            Log.Write("Successfully logged out: " + account.playerName);

            account = null;
        }

        public void Nexus()
        {
            if (player.world is Worlds.Nexus || account == null) return;

            if (manager.module.instanceManager == null && !ModularProgram.manifest.local)
                BeginTransferPlayer("", 1);
            else
                BeginTransferPlayer(null, 1);
        }

        public void BeginTransferPlayer(string remoteServer, uint worldId)
        {
            if (Interlocked.CompareExchange(ref transferAccess, 1, 0) != 0) return;
            if (remoteServer == null)
            {
                if (!manager.module.worldManager.TryGetWorld(worldId, out var otherWorld))
                {
                    transferAccess = 0;
                    player.AddChat(ChatData.Error("Unable to enter portal."));
                    return;
                }

                if (!otherWorld.TryGenerateKey(account.id, out var key))
                {
                    transferAccess = 0;
                    player.AddChat(ChatData.Error("World is full"));
                    return;
                }

                SendAsync(new TnWorldChange());
                var hello = new TnHello(accessTokenEncrypt, player.character.id, player.info.id, worldId, key);
                player.world.LogoutPlayer(player, async (success) =>
                {
                    if (!success)
                    {
                        transferAccess = 0;
                        return;
                    }
                    await Login(hello);
                    transferAccess = 0;
                });
            }
            else if (manager.module.instanceManager != null)
            {
                manager.module.instanceManager.GetTransferKey(remoteServer, account.id, worldId, (ip, key) =>
                {
                    if (key == 0)
                    {
                        transferAccess = 0;
                        return;
                    }

                    SendAsync(new TnWorldChange());
                    player.world.LogoutPlayer(player, (success) =>
                    {
                        if (!success)
                        {
                            transferAccess = 0;
                            return;
                        }
                        SendAsync(new TnReconnect("Overworld", ip, key, worldId));
                        transferAccess = 0;
                    });
                });
            }
            else
            {
                //var hello = new TnHello(accessTokenEncrypt, player.character.id, player.info.id, worldId, 0);
                player.world.LogoutPlayer(player, (success) =>
                {
                    if (!success)
                    {
                        transferAccess = 0;
                        return;
                    }
                    manager.module.SendToNexus(this);
                    transferAccess = 0;
                });
            }
        }

        #endregion

        public void StartTick(uint time)
        {
            if (player == null || player.startTick)
            {
                Disconnect();
                return;
            }
            player.startTick = true;
            player.startTickTime = time;

            timeChecker = new TimeChecker();
            timeChecker.StartTime(time);
        }

        public bool TryTakeCurrency(ObjectStatType currency, long amount)
        {
            if (account == null) return false;
            switch (currency)
            {
                case ObjectStatType.DeathCurrency:
                    if (account.deathCurrency < amount)
                        return false;
                    account.deathCurrency -= amount;
                    return true;
                case ObjectStatType.PremiumCurrency:
                    if (account.premiumCurrency < amount)
                        return false;
                    account.premiumCurrency -= amount;
                    return true;
                default:
                    return false;
            }
        }

        public void GiveCurrency(ObjectStatType currency, long amount)
        {
            if (account == null) return;
            switch (currency)
            {
                case ObjectStatType.DeathCurrency:
                    account.deathCurrency += amount;
                    return;
                case ObjectStatType.PremiumCurrency:
                    account.premiumCurrency += amount;
                    return;
                default:
                    return;
            }
        }

        public void CompleteClassQuest(ClassType classType, int index)
        {
            account.CompleteClassQuest(classType, index);
            var completedCount = account.GetClassQuestCompletedCount();
            if (account.givenRewards < 1 && completedCount >= NetConstants.Account_Reward_Goal_1)
            {
                account.givenRewards = 1;
                account.AccountReward1();
                player.AddChat(ChatData.Info("You've completed an account goal. An extra character slot has been unlocked!"));
            }
            if (account.givenRewards < 2 && completedCount >= NetConstants.Account_Reward_Goal_2)
            {
                account.givenRewards = 2;
                account.AccountReward2();
                player.AddChat(ChatData.Info("You've completed an account goal. An extra 8 vaults slots have been unlocked!"));

                var vault = player.GetVault();
                if (vault != null)
                {
                    vault.ReloadItems();
                }
            }
            if (account.givenRewards < 3 && completedCount >= NetConstants.Account_Reward_Goal_3)
            {
                account.givenRewards = 3;
                account.AccountReward3();
                player.AddChat(ChatData.Info("You've completed an account goal. An extra character slots and 8 vaults slots have been unlocked!"));
            }
        }

        public void UnlockEmote(EmoteUnlockerInfo info)
        {
            if (account.HasUnlockedItem(info.id)) return;
            account.UnlockItem(info.id);
            SendAsync(new TnEmoteUnlocked(info.id));
            player?.AddChat(ChatData.Info($"You've unlocked the '{StringUtils.Labelize(info.emoteType.ToString())}' emote!"));
        }

        public void UnlockSkin(SkinUnlockerInfo info)
        {
            if (account.HasUnlockedItem(info.id)) return;
            account.UnlockItem(info.id);
            SendAsync(new TnSkinUnlocked(info.id));
            player?.AddChat(ChatData.Info($"You've unlocked the '{StringUtils.Labelize(info.name)}'!"));
        }
    }
}