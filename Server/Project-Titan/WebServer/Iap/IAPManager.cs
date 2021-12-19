using Google;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Iap;
using TitanCore.Net;
using TitanCore.Net.Web;
using TitanDatabase;
using TitanDatabase.Models;
using Utils.NET.Logging;
using Utils.NET.Modules;
using Utils.NET.Net.RateLimiting;

namespace WebServer.Iap
{
    public class IapManager
    {
        private const string Apple_Production_Url = "https://buy.itunes.apple.com/verifyReceipt"; 

        private const string Apple_Sandbox_Url = "https://sandbox.itunes.apple.com/verifyReceipt";

        private const string Discord_Token = "--- DISCORD API TOKEN ---";

        private const string Discord_Application_Id = "--- DISCORD APP ID ---";

        private const string Android_Package_Name = "com.UnnamedStudios.TrialsofTitan";

        public static string Steam_Transaction_Url = "https://api.steampowered.com/ISteamMicroTxn/";

        public static string Steam_Sandbox_Transaction_Url = "https://api.steampowered.com/ISteamMicroTxnSandbox/";

        public static string Steam_Web_Api_Key = "--- STEAM WEB API KEY ---";

        public static string Steam_AppId = "949430";

        private HttpClient http;

        private HttpClient discordHttp;

        private Timer iapTimer;

        private AndroidPublisherService androidService;

        private PerIntervalLimit rateLimiter = new PerIntervalLimit(5, 5); // 5 requests per 5 seconds

        private bool sandboxAllowed = ModularProgram.manifest.Value("sandbox", false);

        private string SteamUrl => sandboxAllowed ? Steam_Sandbox_Transaction_Url : Steam_Transaction_Url;

        public IapManager()
        {
            http = new HttpClient();
            discordHttp = new HttpClient();
            discordHttp.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Discord_Token);
            discordHttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //iapTimer = new Timer(OnTimer, null, 0, 5000);

            var serviceAccountEmail = "webserver@trials-of-titan-android.iam.gserviceaccount.com";

            var certificate = new X509Certificate2("--- ANDROID P12 ANDROID CERTIFICATE PATH ---", "--- ANDROID P12 CERTIFICATE PASSWORD ---", X509KeyStorageFlags.Exportable);

            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = new[]
                {
                    AndroidPublisherService.ScopeConstants.Androidpublisher
                }
            }.FromCertificate(certificate));

            androidService = new AndroidPublisherService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Trials of Titan Server"
            });
        }

        private void OnTimer(object state)
        {

        }

        private IapProduct GetProduct(string productId)
        {
            return IapProduct.idToProducts[productId];
        }

        public async Task<int> VerifyApplePurchase(string receiptData, ulong accountId, bool sandbox, IPAddress address)
        {
            if (!sandbox && address != null && !rateLimiter.CanRequest(address)) return -1;

            if (sandbox && !sandboxAllowed)
            {
                return -1;
            }

            var json = new JObject();
            json.Add("receipt-data", receiptData);

            var requestContent = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var response = await http.PostAsync(sandbox ? Apple_Sandbox_Url : Apple_Production_Url, requestContent);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseString);

            var status = responseJson.Value<int>("status");

            switch (status)
            {
                case 0:
                    return await ConsumeApplePurchase(responseJson.Value<JToken>("receipt"), accountId);
                case 21007:
                    return await VerifyApplePurchase(receiptData, accountId, true, address);
                default:
                    if (responseJson.Value<bool>("is-retryable"))
                    {
                        return await VerifyApplePurchase(receiptData, accountId, sandbox, address);
                    }
                    return -1;
            }
        }

        private async Task<int> ConsumeApplePurchase(JToken receipt, ulong accountId)
        {
            int givenCurrency = 0;
            foreach (var value in receipt.Value<JArray>("in_app"))
            {
                var productId = value.Value<string>("product_id");
                var transactionId = value.Value<string>("transaction_id");
                var quantity = value.Value<int>("quantity");

                var transaction = new TransactioniOS()
                {
                    id = transactionId,
                    accountId = accountId,
                    //storeType = StoreType.Apple
                };

                var putResponse = await transaction.Put("attribute_not_exists(id)");
                if (putResponse.result == Model.RequestResult.Success)
                    givenCurrency += GetProduct(productId).currencyReward * quantity;
            }
            return givenCurrency;
        }

        public async Task<int> VerifyDiscordPurchase(string entitlementId, ulong accountId, IPAddress address)
        {
            if (address != null && !rateLimiter.CanRequest(address)) return -1;

            var response = await discordHttp.GetAsync($"https://discord.com/api/v6/applications/{Discord_Application_Id}/entitlements/{entitlementId}");

            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JObject.Parse(responseString);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Log.Write("Failed to contact Discord API, status: " + response.StatusCode);
                return -1;
            }

            var skuIdString = responseJson.Value<string>("sku_id");
            entitlementId = responseJson.Value<string>("id");
            var type = responseJson.Value<int>("type");

            if (type == 4 && !sandboxAllowed)
            {
                Log.Write("Sandbox purchases are not allowed!");
                return -1;
            }

            var skuId = long.Parse(skuIdString);
            var product = IapProduct.discordIdToProducts[skuId];

            var transaction = new TransactioniOS()
            {
                id = entitlementId,
                accountId = accountId,
                //storeType = StoreType.Discord
            };

            var putResponse = await transaction.Put("attribute_not_exists(id)");
            if (putResponse.result == Model.RequestResult.Success)
            {
                if (type == 4)
                    await DeleteTestDiscordEntitlement(entitlementId);
                else
                    await ConsumeDiscordEntitlement(entitlementId);
                return product.currencyReward;
            }
            else
            {
                Log.Write($"Discord entitlement already completed");
                if (type == 4)
                    await DeleteTestDiscordEntitlement(entitlementId);
                else
                    await ConsumeDiscordEntitlement(entitlementId);
                return -1;
            }
        }
        

        private async Task ConsumeDiscordEntitlement(string entitlementId)
        {
            var response = await discordHttp.GetAsync($"https://discord.com/api/v6/applications/{Discord_Application_Id}/entitlements/{entitlementId}/consume");
            //Log.Write($"Consume response: {response.StatusCode}");
        }

        private async Task DeleteTestDiscordEntitlement(string entitlementId)
        {
            var response = await discordHttp.DeleteAsync($"https://discord.com/api/v6/applications/{Discord_Application_Id}/entitlements/{entitlementId}");
            Log.Write($"Delete response: {response.StatusCode}");
        }

        public async Task<int> VerifyAndroidPurchase(string token, string productId, ulong accountId, IPAddress address)
        {
            if (address != null && !rateLimiter.CanRequest(address))
            {
                Log.Error($"Android verify failed rate limiter");
                return -1;
            }

            productId = productId.ToLower();
            if (!IapProduct.androidIdToProducts.TryGetValue(productId, out var product))
            {
                Log.Error($"Failed to retrieve product: {productId}");
                return -1;
            }

            ProductPurchase purchase;
            try
            {
                purchase = await androidService.Purchases.Products.Get(Android_Package_Name, productId, token).ExecuteAsync();
            }
            catch (GoogleApiException apiException)
            {
                Log.Error(apiException);
                return -1;
            }

            if (purchase.PurchaseState.HasValue && purchase.PurchaseState.Value == 1)
            {
                Log.Error($"Product purchase state: {purchase.PurchaseState.Value}");
                return -1;
            }

            if (purchase.PurchaseType.HasValue && !sandboxAllowed)
            {
                Log.Error($"Product has purchaseType: {purchase.PurchaseType.Value}");
                return -1;
            }

            var transaction = new TransactionAndroid()
            {
                id = token,
                accountId = accountId,
                //storeType = StoreType.Android
            };

            var putResponse = await transaction.Put("attribute_not_exists(id)");
            if (putResponse.result == Model.RequestResult.Success)
            {
                return product.currencyReward;
            }
            else
            {
                Log.Error($"Failed to create transaction");
                return -1;
            }
        }

        public async Task<WebSteamInitTxnResponse> StartSteamPurchase(string steamId, string languageCode, string productId, ulong accountId)
        {
            if (!IapProduct.idToProducts.TryGetValue(productId, out var product))
            {
                Log.Error("Failed to get product from id");
                return new WebSteamInitTxnResponse(false);
            }

            var orderId = await Database.CreateSteamTransaction(product.steamId, accountId);

            var url = SteamUrl + "InitTxn/v3/";
            var values = new Dictionary<string, string>
            {
                { "key", Steam_Web_Api_Key },
                { "appid", Steam_AppId },
                { "orderid", orderId.ToString() },
                { "steamid", steamId },
                { "itemcount", "1" },
                { "language", "en" },
                { "currency", "USD" },
                { "itemid[0]", product.steamId.ToString() },
                { "qty[0]", "1" },
                { "amount[0]", product.steamPrice.ToString() },
                { "description[0]", $"{product.currencyReward} of in-game {NetConstants.Premium_Currency_Name}s" },
                { "format", "json" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await http.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseString);
            if (!json.TryGetValue("response", out var responseJson))
            {
                Log.Error("Failed to get correct response from steam.\n" + responseString);
                await TransactionSteam.Delete(orderId);
                return new WebSteamInitTxnResponse(false);
            }

            var result = responseJson["result"];
            if (result.Value<string>() != "OK")
            {
                Log.Error("Failed to get correct response from steam.\n" + responseString);
                await TransactionSteam.Delete(orderId);
                return new WebSteamInitTxnResponse(false);
            }

            return new WebSteamInitTxnResponse(true, orderId);
        }

        public async Task<int> FinalizeSteamPurchase(ulong orderId)
        {
            var transactionResponse = await TransactionSteam.Get(orderId);
            if (transactionResponse.result != Model.RequestResult.Success)
            {
                Log.Error("Failed to load steam transaction from order id.");
                return -1;
            }

            var transaction = transactionResponse.item;
            var product = IapProduct.steamIdToProducts[transaction.itemId];

            var url = SteamUrl + "FinalizeTxn/v2/";
            var values = new Dictionary<string, string>
            {
                { "key", Steam_Web_Api_Key },
                { "appid", Steam_AppId },
                { "orderid", orderId.ToString() },
                { "format", "json" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await http.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            var json = JObject.Parse(responseString);
            if (!json.TryGetValue("response", out var responseJson))
            {
                await TransactionSteam.Delete(orderId);
                Log.Error("Failed to get correct response from steam.\n" + responseString);
                return -1;
            }

            var result = responseJson["result"];
            if (result.Value<string>() != "OK")
            {
                var errorCode = responseJson["error"].Value<int>("errorcode");
                if (errorCode != 6) // 6 means already finalized
                {
                    await TransactionSteam.Delete(orderId);
                    Log.Error("Failed to get correct response from steam.\n" + responseString);
                    return -1;
                }
            }

            var transactionId = responseJson["params"].Value<string>("transid");

            if (!await transaction.Consume(transactionId))
            {
                Log.Error("Failed to consume transaction.\n" + responseString);
                return -1;
            }

            return product.currencyReward;
        }
    }
}
