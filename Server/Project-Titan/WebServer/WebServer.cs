using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Net;
using TitanCore.Net.Web;
using TitanDatabase;
using TitanDatabase.Broadcasting;
using TitanDatabase.Broadcasting.Packets;
using TitanDatabase.Leaderboards;
using TitanDatabase.Models;
using Utils.NET.Crypto;
using Utils.NET.IO;
using Utils.NET.Logging;
using Utils.NET.Net.RateLimiting;
using Utils.NET.Net.Web;
using Utils.NET.Utils;
using WebServer.Iap;
using WebServer.Leaderboard;
using WebServer.Servers;

namespace WebServer
{
    public class WebServer
    {
#if DEBUG
        private const string Prefix = "http://*:8443/";
#else
        private const string Prefix = "https://*.trialsoftitan.com/";
#endif

        private const string ServerName = "Web";

        /// <summary>
        /// Rsa decryption used to read secrets
        /// </summary>
        private static Rsa rsa = new Rsa(Database.Rsa_Private_Key, true);

        private WebListener listener;

        private PerIntervalLimit rateLimiter = new PerIntervalLimit(10, 30); // 10 requests per 30 seconds

        private PerIntervalLimit registerRateLimiter = new PerIntervalLimit(5, TimeSpan.FromDays(1).TotalSeconds); // 5 accounts per day

        private LeaderboardDescriber leaderboardDescriber;

        public BroadcastListener broadcastListener;

        private ServerList serverList = new ServerList();

        //private IapManager iapManager;

        public WebServer()
        {
            broadcastListener = new BroadcastListener();

            //iapManager = new IapManager();

            listener = new WebListener(Prefix);
            listener.AddHandler("privacy", HandlePrivacyPolicy);

            listener.AddHandler("v1/account/login", HandleLogin);
            listener.AddHandler("v1/account/register", HandleRegister);
            listener.AddHandler("v1/account/describe", HandleDescribe);
            listener.AddHandler("v1/account/purchaseslot", HandlePurchaseSlot);
            listener.AddHandler("v1/account/verify", HandleVerify);
            listener.AddHandler("v1/account/verified", HandleVerified);
            listener.AddHandler("v1/account/changename", HandleChangeName);
            listener.AddHandler("v1/account/forgot", HandleForgot);

            listener.AddHandler("v1/server/list", HandleServerList);
            listener.AddHandler("v1/server/update", HandleServerUpdate);

            listener.AddHandler("v1/leaderboard/describe", HandleLeaderboard);

            listener.AddHandler("v1/purchase/ios/verify", HandleiOSPurchaseVerify);
            listener.AddHandler("v1/purchase/android/verify", HandleAndroidPurchaseVerify);
            listener.AddHandler("v1/purchase/steam/start", HandleSteamPurchaseStart);
            listener.AddHandler("v1/purchase/steam/verify", HandleSteamPurchaseVerify);
            listener.AddHandler("v1/purchase/discord/verify", HandleDiscordPurchaseVerify);

            listener.AddHandler("favicon.ico", HandleFavIcon);
        }

        public void Start()
        {
            listener.Start();

            broadcastListener.Start();

            LoadLeaderboards();
        }

        private async void LoadLeaderboards()
        {
            leaderboardDescriber = await LeaderboardDescriber.Load();
        }

        public void Stop()
        {
            listener.Stop();

            broadcastListener.Stop();
        }

        private string DecryptString(string encrypted)
        {
            return Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encrypted)));
        }

        private bool AnyNull(params object[] objs)
        {
            return objs.Any(_ => _ == null);
        }

        private bool AnyNullOrEmpty(params string[] objs)
        {
            return objs.Any(_ => string.IsNullOrWhiteSpace(_));
        }

        private async Task<object> HandleLogin(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebLoginResponse(WebLoginResult.RateLimitExceeded, null);
            }

            var email = query["email"];
            var hash = query["hash"];

            if (AnyNull(email, hash))
            {
                return new WebLoginResponse(WebLoginResult.InvalidRequest, null);
            }

            email = DecryptString(email);
            hash = DecryptString(hash);

            return await Database.WebLogin(email, hash);
        }

        private async Task<object> HandleForgot(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebLoginResponse(WebLoginResult.RateLimitExceeded, null);
            }

            var email = query["email"];

            if (AnyNull(email))
            {
                return new WebLoginResponse(WebLoginResult.InvalidRequest, null);
            }

            email = DecryptString(email);

            return new WebLoginResponse(WebLoginResult.InternalServerError, null);
        }

        private async Task<object> HandleRegister(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address) || !registerRateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebRegisterResponse(WebRegisterResult.RateLimitExceeded, null);
            }

            var email = query["email"];
            var hash = query["hash"];

            if (AnyNull(email, hash))
            {
                return new WebRegisterResponse(WebRegisterResult.InvalidRequest, null);
            }

            email = DecryptString(email);
            hash = DecryptString(hash);

            var createResponse = await Database.CreateAccount(email, hash);
            if (createResponse.result != CreateAccountResult.Success)
            {
                switch (createResponse.result)
                {
                    case CreateAccountResult.DuplicateName:
                        return new WebRegisterResponse(WebRegisterResult.DuplicateName, null);
                    case CreateAccountResult.DuplicateEmail:
                        return new WebRegisterResponse(WebRegisterResult.DuplicateEmail, null);
                    default:
                        return new WebRegisterResponse(WebRegisterResult.InternalServerError, null);
                }
            }

            return new WebRegisterResponse(WebRegisterResult.Success, createResponse.token.accessToken);
        }

        private async Task<object> HandleDescribe(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebDescribeResponse(WebDescribeResult.RateLimitReached);
            }

            var token = query["token"];
            var version = query["version"];

            if (AnyNull(token, version))
            {
                return new WebDescribeResponse(WebDescribeResult.InvalidRequest);
            }

            if (!NetConstants.BuildCanPlay(version))
                return new WebDescribeResponse(WebDescribeResult.UpdateRequired, NetConstants.Required_Build_Version);

            token = DecryptString(token);

            var describe = await Database.WebDescribe(token);
            if (describe.result == WebDescribeResult.Success)
                describe.servers = serverList.infos;
            return describe;
        }

        private async Task<object> HandlePurchaseSlot(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebPurchaseSlotResponse(WebPurchaseSlotResult.RateLimitReached, 0);
            }

            var token = query["token"];

            if (AnyNull(token))
            {
                return new WebPurchaseSlotResponse(WebPurchaseSlotResult.InvalidRequest, 0);
            }

            token = DecryptString(token);

            var loginResponse = await Database.Login(token, ServerName);
            if (loginResponse.result != LoginResult.Success)
            {
                switch (loginResponse.result)
                {
                    case LoginResult.AccountInUse:
                        return new WebPurchaseSlotResponse(WebPurchaseSlotResult.AccountInUse, 0);
                    default:
                        return new WebPurchaseSlotResponse(WebPurchaseSlotResult.InternalServerError, 0);
                }
            }

            var account = loginResponse.account;
            try
            {
                var cost = NetConstants.GetCharacterSlotCost(account.maxCharacters);
                if (account.premiumCurrency < cost)
                {
                    return new WebPurchaseSlotResponse(WebPurchaseSlotResult.NotEnoughGold, 0);
                }

                account.premiumCurrency -= cost;
                account.maxCharacters++;

                return new WebPurchaseSlotResponse(WebPurchaseSlotResult.Success, account.premiumCurrency);
            }
            finally
            {
                await Database.Logout(account, ServerName);
            }
        }

        private TemplateString verifyHtml = new TemplateString(File.ReadAllText("Htmls/verify_email.html"), '*');

        private async Task<object> HandleVerify(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebDescribeResponse(WebDescribeResult.RateLimitReached);
            }

            var token = query["token"];

            if (AnyNull(token))
            {
                return new WebDescribeResponse(WebDescribeResult.InvalidRequest);
            }

            await SendHtml(context, verifyHtml.Build(
                new Dictionary<string, string>
                {
                    { "token", token }
                }
            ));

            return null;
        }

        private async Task<object> HandleVerified(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebDescribeResponse(WebDescribeResult.RateLimitReached);
            }

            var verificationKey = query["token"];
            var gToken = query["gToken"];

            if (AnyNull(verificationKey, gToken))
            {
                return new WebDescribeResponse(WebDescribeResult.InvalidRequest);
            }

            JObject value = new JObject();
            if (AnyNull(verificationKey, gToken))
            {
                value["error"] = true;
                value["value"] = "Invalid input";
                await SendJson(context, value);
                return null;
            }

            string uri = "https://www.google.com/recaptcha/api/siteverify";
            string parameters = $"secret=6LeZ0fcUAAAAAPMuY8NemgY_y6HsOozfyE8ADOUg&response={gToken}&remoteip={context.Request.RemoteEndPoint.Address.ToString()}";

            string result;
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                try
                {
                    result = await wc.UploadStringTaskAsync(uri, parameters);
                }
                catch (Exception)
                {
                    value["error"] = true;
                    value["value"] = "Error validating captcha";
                    await SendJson(context, value);
                    return null;
                }
            }

            var json = JObject.Parse(result);
            if (!json.GetValue("success").Value<bool>())
            {
                value["error"] = true;
                value["value"] = "Error validating captcha";
                await SendJson(context, value);
                return null;
            }

            var verifyResponse = await Database.VerifyEmail(verificationKey);

            if (verifyResponse != EmailVerificationResult.Success)
            {
                value["error"] = true;
                value["value"] = StringUtils.Labelize(verifyResponse.ToString());
                await SendJson(context, value);
                return null;
            }

            value["error"] = false;
            await SendJson(context, value);
            return null;
        }

        private static byte[] favIcon = File.ReadAllBytes("favicon.png");

        private async Task<object> HandleFavIcon(HttpListenerContext context, NameValueCollection query)
        {
            //context.Response.ContentType = "image/x-icon";
            return favIcon;
        }

        private async Task<object> HandleChangeName(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebNameChangeResponse(WebNameChangeResult.RateLimitExceeded, null);
            }

            var fromName = query["fromName"];
            var toName = query["toName"];
            var token = query["token"];
            var reservation = query["reservation"];

            if (AnyNullOrEmpty(fromName, toName, token))
            {
                return new WebNameChangeResponse(WebNameChangeResult.InvalidRequest, null);
            }

            token = DecryptString(token);
            if (!string.IsNullOrEmpty(reservation))
                reservation = DecryptString(reservation);

            var response = await Database.ChangeName(fromName, toName, reservation, token, ServerName);
            Log.Write(response.result);
            return response;
        }

        protected async Task SendHtml(HttpListenerContext context, string html)
        {
            context.Response.ContentType = "text/html";
            var bytes = Encoding.UTF8.GetBytes(html);
            try
            {
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }

        protected async Task SendJson(HttpListenerContext context, JObject json)
        {
            context.Response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(json.ToString(Formatting.None));
            try
            {
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
        }

        private async Task<object> HandleLeaderboard(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebLeaderboardResponse(WebLeaderboardResult.RateLimitReached);
            }

            var typeString = query["type"];

            if (AnyNull(typeString))
            {
                return new WebLeaderboardResponse(WebLeaderboardResult.InvalidRequest);
            }
            
            if (!Enum.TryParse(typeString, true, out LeaderboardType leaderboardType))
            {
                return new WebLeaderboardResponse(WebLeaderboardResult.InvalidRequest);
            }

            if (leaderboardDescriber == null)
                return new WebLeaderboardResponse(WebLeaderboardResult.InternalServerError);

            var infos = leaderboardDescriber.GetLeaderboard(leaderboardType);
            if (infos == null)
                return new WebLeaderboardResponse(WebLeaderboardResult.InternalServerError);

            return new WebLeaderboardResponse(WebLeaderboardResult.Success, infos);
        }

        private async Task<object> HandleServerList(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebServerListResponse(WebServerListResult.RateLimitExceeded);
            }

            var version = query["version"];
            
            if (version != null)
            {
                if (NetConstants.BuildAhead(version))
                {
                    return new WebServerListResponse(WebServerListResult.Success, new WebServerInfo[] {
                        new WebServerInfo("Testing", "71.195.254.8", "71.195.254.8", ServerStatus.Normal)
                    });
                }
            }

            return new WebServerListResponse(WebServerListResult.Success, serverList.infos);
        }

        private async Task<object> HandleServerUpdate(HttpListenerContext context, NameValueCollection query)
        {
            var authString = query["auth"];
            var name = query["name"];
            var host = query["host"];
            var pingHost = query["pingHost"];
            var statusString = query["status"];

            //Log.Write($"Received unverified server update: name: {name}, host: {host}, pingHost: {pingHost}, status: {statusString}");

            if (AnyNull(authString, name, host, pingHost, statusString)) return null;
            if (!authString.Equals(Tokens.Server_Auth_Token)) return null;

            if (!Enum.TryParse(statusString, true, out ServerStatus status)) return null;

            //Log.Write($"Received server update: name: {name}, host: {host}, pingHost: {pingHost}, status: {status}");

            serverList.PushUpdate(name, host, pingHost, status);

            return null;
        }

        private async Task<object> HandleiOSPurchaseVerify(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebVerifyResponse(false);
            }

            var receiptData = query["receipt"];
            var accountIdString = query["accountId"];

            if (AnyNull(receiptData, accountIdString)) return new WebVerifyResponse(false);
            if (!ulong.TryParse(accountIdString, out var accountId)) return new WebVerifyResponse(false);

            int currencyAmount = -1;// iapManager.VerifyApplePurchase(receiptData, accountId, false, context.Request.RemoteEndPoint.Address);
            if (currencyAmount < 0)
                return new WebVerifyResponse(false);

            if (currencyAmount == 0)
                return new WebVerifyResponse(true);

            return new WebVerifyResponse(await DispatchGold(accountId, currencyAmount));
        }

        private async Task<object> HandleAndroidPurchaseVerify(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebVerifyResponse(false);
            }

            var token = query["token"];
            var productId = query["productId"];
            var accountIdString = query["accountId"];

            if (AnyNullOrEmpty(token, productId, accountIdString)) return new WebVerifyResponse(false);
            if (!ulong.TryParse(accountIdString, out var accountId)) return new WebVerifyResponse(false);

            int currencyAmount = -1;// iapManager.VerifyAndroidPurchase(token, productId, accountId, context.Request.RemoteEndPoint.Address);
            if (currencyAmount < 0)
                return new WebVerifyResponse(false);

            if (currencyAmount == 0)
                return new WebVerifyResponse(true);

            return new WebVerifyResponse(await DispatchGold(accountId, currencyAmount));
        }

        private async Task<object> HandleSteamPurchaseStart(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebSteamInitTxnResponse(false);
            }

            var steamId = query["steamId"];
            var languageCode = query["lan"];
            var productId = query["productId"];
            var accountIdString = query["accountId"];

            if (AnyNullOrEmpty(steamId, languageCode, productId, accountIdString)) return new WebVerifyResponse(false);
            if (!ulong.TryParse(accountIdString, out var accountId)) return new WebVerifyResponse(false);

            //var response = await iapManager.StartSteamPurchase(steamId, languageCode, productId, accountId);
            return new WebSteamInitTxnResponse(false);
        }

        private async Task<object> HandleSteamPurchaseVerify(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebVerifyResponse(false);
            }

            var orderIdString = query["orderId"];
            var accountIdString = query["accountId"];

            if (AnyNullOrEmpty(orderIdString, accountIdString)) return new WebVerifyResponse(false);
            if (!ulong.TryParse(accountIdString, out var accountId) || !ulong.TryParse(orderIdString, out var orderId)) return new WebVerifyResponse(false);

            var currencyAmount = -1;// await iapManager.FinalizeSteamPurchase(orderId);

            if (currencyAmount < 0)
                return new WebVerifyResponse(false);

            return new WebVerifyResponse(await DispatchGold(accountId, currencyAmount));
        }

        private async Task<object> HandleDiscordPurchaseVerify(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebVerifyResponse(false);
            }

            var accountIdString = query["accountId"];
            var entitlementId = query["id"];

            if (AnyNull(entitlementId, accountIdString)) return new WebVerifyResponse(false);
            if (!ulong.TryParse(accountIdString, out var accountId)) return new WebVerifyResponse(false);

            int currencyAmount = -1;// await iapManager.VerifyDiscordPurchase(entitlementId, accountId, context.Request.RemoteEndPoint.Address);
            if (currencyAmount < 0)
                return new WebVerifyResponse(false);

            if (currencyAmount == 0)
                return new WebVerifyResponse(true);

            return new WebVerifyResponse(await DispatchGold(accountId, currencyAmount));
        }

        private async Task<bool> DispatchGold(ulong accountId, int currency)
        {
            int bc = 5;
            while (bc-- > 0)
            {
                if (await TryDispatchGold(accountId, currency))
                    return true;
            }
            return false;
        }

        private async Task<bool> TryDispatchGold(ulong accountId, int currency)
        {
            var accLockResponse = await AccountLock.Get(accountId);
            if (accLockResponse.result == Model.RequestResult.Success)
            {
                var server = accLockResponse.item.server;
                if (broadcastListener.TryGetConnection(server, out var connection))
                {
                    var giveGold = new BrGiveGold(accountId, currency);
                    var resetEvent = new ManualResetEvent(false);
                    BrGiveGoldResponse response = null;
                    connection.SendTokenAsync(giveGold, (responsePacket, net) =>
                    {
                        response = (BrGiveGoldResponse)responsePacket;
                        resetEvent.Set();
                    });

                    resetEvent.WaitOne(5000);

                    if (response != null && response.success)
                    {
                        return true;
                    }
                }
            }

            var loginResponse = await Database.Login(accountId, ServerName);
            if (loginResponse.result == LoginResult.Success)
            {
                loginResponse.account.premiumCurrency += currency;
                await Database.Logout(loginResponse.account, ServerName);
                return true;
            }

            return false;
        }

        private string privacyPolicyHtml = File.ReadAllText("Htmls/privacy_policy.html");

        private async Task<object> HandlePrivacyPolicy(HttpListenerContext context, NameValueCollection query)
        {
            if (!rateLimiter.CanRequest(context.Request.RemoteEndPoint.Address))
            {
                return new WebDescribeResponse(WebDescribeResult.RateLimitReached);
            }

            await SendHtml(context, privacyPolicyHtml);

            return null;
        }
    }
}
