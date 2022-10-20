using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Entities;
using Utils.NET.Logging;
using Utils.NET.Utils;
using TitanDatabase.Models;
using static TitanDatabase.Models.Model;
using TitanCore.Net.Web;
using TitanCore.Core;
using TitanDatabase.Email;
using TitanCore.Net;
using Amazon;
using TitanDatabase.Leaderboards;
using Utils.NET.Modules;

namespace TitanDatabase
{
    public class LoadCharactersResponse
    {
        public LoadCharactersResult result;

        public List<Character> characters;
    }

    public enum LoadCharactersResult
    {
        Success,
        AwsError
    }
    public class LoadItemsResponse
    {
        public LoadItemsResult result;

        public List<ServerItem> items;
    }

    public enum LoadItemsResult
    {
        Success,
        AwsError
    }

    public enum SaveItemsResult
    {
        Success,
        AwsError
    }

    public class CreateAccountResponse
    {
        public TokenLogin token;

        public CreateAccountResult result;

        public CreateAccountResponse(TokenLogin token, CreateAccountResult result)
        {
            this.token = token;
            this.result = result;
        }
    }

    public enum CreateAccountResult
    {
        Success,
        DuplicateName,
        DuplicateEmail,
        AwsServerError
    }

    public enum EmailVerificationResult
    {
        Success,
        LinkExpired,
        InternalServerError
    }

    public class CreateCharacterResponse
    {
        public Character character;

        public CreateCharacterResult result;

        public CreateCharacterResponse(Character character, CreateCharacterResult result)
        {
            this.character = character;
            this.result = result;
        }
    }

    public enum CreateCharacterResult
    {
        Success,
        MaxCharactersReached,
        InvalidCharacterType,
        AwsServerError,
        CharacterTypeLocked,
        CharacterNotPlayable
    }

    public class CreateItemResponse
    {
        public ServerItem item;

        public CreateItemResult result;

        public CreateItemResponse(ServerItem item, CreateItemResult result)
        {
            this.item = item;
            this.result = result;
        }
    }

    public enum CreateItemResult
    {
        Success,
        AwsServerError
    }

    public class LoginResponse
    {
        public Account account;

        public LoginResult result;

        public LoginResponse(Account account, LoginResult result)
        {
            this.account = account;
            this.result = result;
        }
    }

    public enum LoginResult
    {
        Success,
        EmailNotFound,
        TokenNotFound,
        HashConditionFailed,
        AwsServerError,
        AccountInUse,
        FailedToCreate
    }

    public class LogoutResponse
    {
        public Account account;

        public LogoutResult result;

        public LogoutResponse(Account account, LogoutResult result)
        {
            this.account = account;
            this.result = result;
        }
    }

    public enum LogoutResult
    {
        Success,
        FailedToRemoveLock,
        AwsServerError
    }

    public class FailedToCreateClientException : Exception
    {

    }

    public static class Database
    {
        public const string Rsa_Private_Key = "eyJEIjoib2tNVlpzL2phS3NmcFZ5b0FoNzhoSjVxd2ZHb2ExakxsQzF4NXFuTlI1b2JWWHd2SVE0cjUvVmVuc2txU0VzVG1md0RrL2xOcEtkeUh1MFNBSm4wYmpEb1VtUTVqZFh1Z0k5a1l4WUJ6dGJNYURlTU5nVzJBNjBqY0NLdGlXa3Y5dG0yQjE0Yy8rY2Rnb1dkTEVzeW5ZMTRIMXp4V2M1aTZWSlNhMVlJdGYwPSIsIkRQIjoiWXA5OVJmQ0hIdmlnSXcxV1NUWDViVVlZSHB3ZGw4RjNocXB5RWdHSnp4c1VwbUEvSUtwZmY4cmRNMG1MRHRDT09hR2dlaE81RlNFSWtvTFpGTVcyV1E9PSIsIkRRIjoieUVsVUl5em1tSXVzQVdpSXlMOHRBaDZtcmxGQktiM3g5bXBLVUUvZldqczFxVG1NUmp2bGF5dGhzU0U5TG1lLy9YOUZ0aVo1RjFtTmF6di8vSXAzb3c9PSIsIkV4cG9uZW50IjoiQVFBQiIsIkludmVyc2VRIjoiMDZiT05tNW9IaXFvLzRkTGlIMlVzdDhySGtqUnJ6ME5TSWpXdUo3QzZ5UE00ZnA2OFhPdmwyTnNmY1JSZFh5Q0xSOStwajdRam01bGY2VnVXcno2dFE9PSIsIk1vZHVsdXMiOiIxTVhRMWE2cFQza1FTOG4xUkppSDI2Q1UxTUY4YXNXNFE1NGR1Q0o3WmxwUmpZUjNrKzNrbTM2YmNBYUZWa29qTWRJNTZUWUFPYVBZb2crTG5PQTdqRG5sUFBZNkxaQmg3SVRmZVFhZHJnbld3aHJPbXJPWHJUQXYvQStCVnV3UlpEYVZ2cXlkQTBTQ2hQRzFZOEhuUjZiRU9nSVpodFcyaCt3dSswbzZacEU9IiwiUCI6Ijd6WEdySkRrdnllRTNlZm8zbGxqWEcybTFVRGlEem5jKzgvZ2IzUXhiTVozLzVkMFBjbW44aWJhemdBMk1vT2hESGVNdi9SYmZsVm1aVXVqNTVVVTh3PT0iLCJRIjoiNDdVQXNNcDdGV2I1cjI4cHM3WjNHcFBsbTIzczFsRkhCR2s0QkVYbVFyUzRkbGJ2TGxucEVsQk9OM3NWUDV5NmEzNjErSWYySmkxdlFSMk5IVWdIYXc9PSJ9";

        private static AmazonDynamoDBClient client;
        public static AmazonDynamoDBClient Client => client;

        #region Init

        public static ManualResetEvent Initialize()
        {
            bool local = ModularProgram.manifest.Value<bool>("localDb", true);
            var resetEvent = new ManualResetEvent(false);
            if (client != null)
            {
                resetEvent.Set();
            }
            else if (CreateClient(local))
            {
                CreateTables(resetEvent);
            }
            else
            {
                throw new FailedToCreateClientException();
            }
            return resetEvent;
        }

        private static bool CreateClient(bool local)
        {
            if (local)
            {
                // First, check to see whether anyone is listening on the DynamoDB local port
                // (by default, this is port 8000, so if you are using a different port, modify this accordingly)
                bool localFound = false;
                try
                {
                    using (var tcp_client = new TcpClient())
                    {
                        var result = tcp_client.BeginConnect("localhost", 8000, null, null);
                        localFound = result.AsyncWaitHandle.WaitOne(3000); // Wait 3 seconds
                        tcp_client.EndConnect(result);
                    }
                }
                catch
                {
                    localFound = false;
                }

                if (!localFound)
                {
                    Log.Write("ERROR: DynamoDB Local does not appear to have been started... (checked port 8000)");
                    return false;
                }

                // If DynamoDB-Local does seem to be running, so create a client
                Log.Write("Setting up a DynamoDB-Local client (DynamoDB Local seems to be running)");

                AmazonDynamoDBConfig ddbConfig = new AmazonDynamoDBConfig();
                ddbConfig.ServiceURL = "http://localhost:8000";

                try { client = new AmazonDynamoDBClient(new BasicAWSCredentials("test", "test"), ddbConfig); } // TODO change credentials
                catch (Exception ex)
                {
                    Log.Write("FAILED to create a DynamoDBLocal client; " + ex.Message);
                    return false;
                }
            }
            else
            {
                try { client = new AmazonDynamoDBClient(AwsConstants.Key, AwsConstants.Secret, RegionEndpoint.USEast2); }
                catch (Exception ex)
                {
                    Log.Write("FAILED to create a DynamoDB client; " + ex.Message);
                    return false;
                }
            }

            Log.Write("DynamoDB client successfully setup");
            return true;
        }

        #endregion

        #region Tables

        public static string Table_Accounts = ModularProgram.manifest.Value("dbPrefix", "") + "titan_accounts";

        public static string Table_Token_Login = ModularProgram.manifest.Value("dbPrefix", "") + "titan_token_login";

        public static string Table_Email_Login = ModularProgram.manifest.Value("dbPrefix", "") + "titan_email_login";

        public static string Table_Name_Reservation = ModularProgram.manifest.Value("dbPrefix", "") + "titan_name_reservation";

        public static string Table_Account_Lock = ModularProgram.manifest.Value("dbPrefix", "") + "titan_account_lock";

        public static string Table_Characters = ModularProgram.manifest.Value("dbPrefix", "") + "titan_characters";

        public static string Table_Items = ModularProgram.manifest.Value("dbPrefix", "") + "titan_items";

        public static string Table_Verification = ModularProgram.manifest.Value("dbPrefix", "") + "titan_verification";

        public static string Table_Leaderboards = ModularProgram.manifest.Value("dbPrefix", "") + "titan_leaderboards";

        public static string Table_Ip_Ban = ModularProgram.manifest.Value("dbPrefix", "") + "titan_ip_bans";

        public static string Table_iOS_Transactions = ModularProgram.manifest.Value("dbPrefix", "") + "titan_ios_transactions";

        public static string Table_Android_Transactions = ModularProgram.manifest.Value("dbPrefix", "") + "titan_android_transactions";

        public static string Table_Steam_Transactions = ModularProgram.manifest.Value("dbPrefix", "") + "titan_steam_transactions";

        private static async void CreateTables(ManualResetEvent resetEvent)
        {
            var tableCreators = new Dictionary<string, Func<Task<bool>>>()
            {
                { Table_Accounts, CreateAccountsTable },
                { Table_Token_Login, CreateTokenLoginTable },
                { Table_Email_Login, CreateEmailLoginTable },
                { Table_Name_Reservation, CreateNameReservationTable },
                { Table_Account_Lock, CreateAccountLockTable },
                { Table_Characters, CreateCharactersTable },
                { Table_Items, CreateItemsTable },
                { Table_Verification, CreateVerificationTable },
                { Table_Leaderboards, CreateLeaderboardsTable },
                { Table_iOS_Transactions, CreateiOSTransactionsTable },
                { Table_Android_Transactions, CreateAndroidTransactionsTable },
                { Table_Steam_Transactions, CreateSteamTransactionsTable },
            };

            var missing = await GetMissingTables(tableCreators.Keys.ToArray());
            foreach (var missingTable in missing)
            {
                if (!tableCreators.TryGetValue(missingTable, out var creator)) continue;
                await creator();
            }

            resetEvent.Set();
        }

        private static async Task<List<string>> GetMissingTables(string[] tables)
        {
            var missing = new List<string>();

            ListTablesResponse listTablesResponse = null;
            try
            {
                listTablesResponse = await client.ListTablesAsync();
            }
            catch (InternalServerErrorException internalEx)
            {
                Log.Write($"Failed to retrieve table list: {internalEx.Message}");
            }

            foreach (var table in tables)
            {
                if (listTablesResponse.TableNames.Contains(table)) continue;
                missing.Add(table);
            }

            return missing;
        }

        private static async Task<bool> CreateTable(CreateTableRequest request, bool ttl)
        {
            Log.Write("Creating table: " + request.TableName);
            CreateTableResponse response;
            try
            {
                response = await client.CreateTableAsync(request);
            }
            catch (InternalServerErrorException internalEx)
            {
                Log.Write($"Failed to create table {request.TableName}: {internalEx.Message}");
                return false;
            }
            catch (LimitExceededException limitEx)
            {
                Log.Write($"Failed to create table {request.TableName}: {limitEx.Message}");
                return false;
            }
            catch (ResourceInUseException inUseEx)
            {
                Log.Write($"Failed to create table {request.TableName}: {inUseEx.Message}");
                return false;
            }

            if (ttl)
            {
                var ttlRequest = new UpdateTimeToLiveRequest()
                {
                    TableName = request.TableName,
                    TimeToLiveSpecification = new TimeToLiveSpecification()
                    {
                        AttributeName = "ttl",
                        Enabled = true
                    }
                };
                await client.UpdateTimeToLiveAsync(ttlRequest);
            }

            return true;
        }

        private static async Task<bool> CreateAccountsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Accounts,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateTokenLoginTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Token_Login,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("accessToken", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("accessToken", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateEmailLoginTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Email_Login,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("email", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("email", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateNameReservationTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Name_Reservation,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("playerName", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("playerName", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateAccountLockTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Account_Lock,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("accountId", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("accountId", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateCharactersTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Characters,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateItemsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Items,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateVerificationTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Verification,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("verifyToken", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("verifyToken", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateLeaderboardsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Leaderboards,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateIpBansTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Ip_Ban,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("ip", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("ip", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateiOSTransactionsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_iOS_Transactions,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateAndroidTransactionsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Android_Transactions,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        private static async Task<bool> CreateSteamTransactionsTable()
        {
            var request = new CreateTableRequest()
            {
                TableName = Table_Steam_Transactions,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("id", ScalarAttributeType.N)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("id", KeyType.HASH)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            return await CreateTable(request, false);
        }

        #endregion

        #region Creation

        private static ulong GenerateUInt64Id()
        {
            ulong id;
            do
            {
                id = BitConverter.ToUInt64(Rand.Bytes(8), 0);
            } while (id == 0);
            return id;
        }

        private static string GenerateAccessToken()
        {
            return Rand.Base64(64);
        }

        private static string GenerateVerificationToken()
        {
            return Rand.String(40, StringUtils.alphaNumericCharacters);
        }

        private static async Task CleanupAccountCreation(ulong id, string token, string email)
        {
            DeleteResponse response;
            if (email != null)
            {
                response = await EmailLogin.Delete(email.ToLower(), id);
                Log.Write("Email cleanup result: " + response.result);
            }
            if (token != null)
            {
                response = await TokenLogin.Delete(token, id);
                Log.Write("Token cleanup result: " + response.result);
            }
            if (id != 0)
            {
                response = await Account.Delete(id);
                Log.Write("Account cleanup result: " + response.result);
            }
        }

        public static async Task<bool> IsNameAvailable(string name)
        {
            var nameAvailableCheck = await NameReservation.Get(name);
            if (nameAvailableCheck.result == RequestResult.Success)
            {
                // name taken
                return false;
            }
            else if (nameAvailableCheck.result != RequestResult.ResourceNotFound)
            {
                // server error, don't continue with name just in case
                return false;
            }

            return true;
        }

        public static async Task ReserveName(string name, ulong accountId)
        {

        }

        public static async Task<CreateAccountResponse> CreateAccount(string email, string hash)
        {
            ulong id = 0;
            var creationDate = DateTime.UtcNow;

            var account = new Account // create account
            {
                id = 0,
                playerName = "*",
                email = email,
                creationDate = creationDate,
                premiumCurrency = 0,
#if DEBUG
                rank = Rank.Admin,
#endif
                bannedUntil = DateTime.UtcNow.AddDays(-1),
                verifiedEmail = true
            };


            var emailVerification = new EmailVerification() // create email verification
            {
                accountId = id,
                ttl = TimeUtils.ToEpochSeconds(DateTime.UtcNow.AddDays(1))
            };
            bool tokenFailed = false;
            do
            {
                emailVerification.verifyToken = GenerateVerificationToken();
                var verificationResponse = await emailVerification.Put("attribute_not_exists(verifyToken)");
                if (verificationResponse.result != RequestResult.Success)
                {
                    switch (verificationResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            tokenFailed = true; // regenerate token if the condition failed (i.e. the token is already in use)
                            break;
                        default:
                            await CleanupAccountCreation(id, null, null);
                            return new CreateAccountResponse(null, CreateAccountResult.AwsServerError);
                    }
                }
            }
            while (tokenFailed);
            account.verificationToken = emailVerification.verifyToken;

            bool idFailed = false;
            do
            {
                id = GenerateUInt64Id();
                if (id == 0)
                {
                    idFailed = true;
                    continue;
                }

                account.id = id;
                var accountResponse = await account.Put("attribute_not_exists(id)");
                if (accountResponse.result != RequestResult.Success)
                {
                    switch (accountResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            idFailed = true;
                            break;
                        default:
                            return new CreateAccountResponse(null, CreateAccountResult.AwsServerError);
                    }
                }
            }
            while (idFailed);

            emailVerification.accountId = account.id;
            await emailVerification.Put();

            /*
            var nameReservation = new NameReservation() // create name reservation
            {
                playerName = name.ToLower(),
                accountId = id,
                creationDate = creationDate
            };
            var nameResponse = await nameReservation.Put("attribute_not_exists(playerName)");
            if (nameResponse.result != RequestResult.Success)
            {
                switch (nameResponse.result)
                {
                    case RequestResult.ConditionalCheckFailed:
                        await CleanupAccountCreation(id, null, null, null);
                        return new CreateAccountResponse(null, CreateAccountResult.DuplicateName);
                    default:
                        await CleanupAccountCreation(id, null, null, name);
                        return new CreateAccountResponse(null, CreateAccountResult.AwsServerError);
                }
            }
            */

            var tokenLogin = new TokenLogin() // create token login
            {
                accountId = id
            };
            tokenFailed = false;
            do
            {
                tokenLogin.accessToken = GenerateAccessToken();
                var tokenResponse = await tokenLogin.Put("attribute_not_exists(accessToken)");
                if (tokenResponse.result != RequestResult.Success)
                {
                    switch (tokenResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            tokenFailed = true; // regenerate token if the condition failed (i.e. the token is already in use)
                            break;
                        default:
                            await CleanupAccountCreation(id, tokenLogin.accessToken, email);
                            return new CreateAccountResponse(null, CreateAccountResult.AwsServerError);
                    }
                }
            }
            while (tokenFailed);

            var emailLogin = new EmailLogin() // create email login
            {
                email = email.ToLower(),
                hash = Account.CreateHash(hash, creationDate),
                accountId = id,
                accessToken = tokenLogin.accessToken,
                creationDate = creationDate
            };
            var emailResponse = await emailLogin.Put("attribute_not_exists(email)");
            if (emailResponse.result != RequestResult.Success)
            {
                switch (emailResponse.result)
                {
                    case RequestResult.ConditionalCheckFailed:
                        await CleanupAccountCreation(id, null, null);
                        return new CreateAccountResponse(null, CreateAccountResult.DuplicateEmail);
                    default:
                        await CleanupAccountCreation(id, null, email);
                        return new CreateAccountResponse(null, CreateAccountResult.AwsServerError);
                }
            }

            Log.Write("Created account: " + email);
            await Emailer.SendVerificationEmail(email, account.verificationToken);

            return new CreateAccountResponse(tokenLogin, CreateAccountResult.Success);
        }

        private static List<uint> CreateCharacterStats(CharacterInfo info)
        {
            var stats = new List<uint>();
            for (int i = 0; i < 5; i++)
            {
                var stat = info.stats[(StatType)i];
                stats.Add((uint)stat.baseValue);
            }
            return stats;
        }

        private static List<ulong> CreateCharacterItems()
        {
            var items = new List<ulong>();
            for (int i = 0; i < 12; i++)
                items.Add(0);
            return items;
        }

        public static async Task<CreateCharacterResponse> CreateCharacter(Account account, ushort type)
        {
            if (account.characters.Count >= account.maxCharacters)
                return new CreateCharacterResponse(null, CreateCharacterResult.MaxCharactersReached);

            if (!GameData.objects.TryGetValue(type, out var info) || !(info is CharacterInfo charInfo))
                return new CreateCharacterResponse(null, CreateCharacterResult.InvalidCharacterType);

            if (!account.CanCreateCharacter(charInfo))
                return new CreateCharacterResponse(null, CreateCharacterResult.CharacterTypeLocked);

            if (charInfo.notPlayable)
                return new CreateCharacterResponse(null, CreateCharacterResult.CharacterNotPlayable);

            var character = new Character()
            {
                id = 0,
                accountId = account.id,
                type = type,
                experience = 0,
                level = 1,
                stats = CreateCharacterStats(charInfo),
                statsLocked = new List<uint>(),
                itemIds = CreateCharacterItems(),
                dead = false,
                killer = 0,
                creationDate = DateTime.UtcNow
            };

            bool idFailed = false;
            do
            {
                ulong id = GenerateUInt64Id();
                if (id == 0)
                {
                    idFailed = true;
                    continue;
                }

                character.id = id;
                var characterResponse = await character.Put("attribute_not_exists(id)");
                if (characterResponse.result != RequestResult.Success)
                {
                    switch (characterResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            idFailed = true;
                            break;
                        default:
                            return new CreateCharacterResponse(null, CreateCharacterResult.AwsServerError);
                    }
                }
            }
            while (idFailed);

            character.items = new List<ServerItem>();
            for (int i = 0; i < character.itemIds.Count; i++)
            {
                if (i >= charInfo.defaultItems.Length)
                {
                    character.items.Add(null);
                    continue;
                }

                var defaultItem = charInfo.defaultItems[i];
                if (defaultItem == 0)
                {
                    character.items.Add(null);
                    continue;
                }

                var createResponse = await CreateItem(new Item(defaultItem), character.id);
                if (createResponse.result != CreateItemResult.Success)
                {

                    character.items.Add(null);
                    continue;
                }

                character.items.Add(createResponse.item);
            }

            account.characters.Add(character.id);
            return new CreateCharacterResponse(character, CreateCharacterResult.Success);
        }

        public static async Task<CreateItemResponse> CreateItem(Item data, ulong containerId)
        {
            var serverItem = new ServerItem()
            {
                itemData = data,
                containerId = containerId
            };

            bool idFailed = false;
            do
            {
                ulong id = GenerateUInt64Id();
                if (id == 0)
                {
                    idFailed = true;
                    continue;
                }

                serverItem.id = id;
                var itemResponse = await serverItem.Put("attribute_not_exists(id)");
                if (itemResponse.result != RequestResult.Success)
                {
                    switch (itemResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            idFailed = true;
                            break;
                        default:
                            return new CreateItemResponse(null, CreateItemResult.AwsServerError);
                    }
                }
                else
                    idFailed = false;
            }
            while (idFailed);

            return new CreateItemResponse(serverItem, CreateItemResult.Success);
        }

        public static async Task<ulong> CreateSteamTransaction(uint itemId, ulong accountId)
        {
            var steamTransaction = new TransactionSteam()
            {
                id = 0,
                accountId = accountId,
                itemId = itemId,
                finalized = 0
            };

            bool idFailed = false;
            do
            {
                ulong id = GenerateUInt64Id();
                if (id == 0)
                {
                    idFailed = true;
                    continue;
                }

                steamTransaction.id = id;
                var itemResponse = await steamTransaction.Put("attribute_not_exists(id)");
                if (itemResponse.result != RequestResult.Success)
                {
                    switch (itemResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            idFailed = true;
                            break;
                        default:
                            return 0;
                    }
                }
                else
                    idFailed = false;
            }
            while (idFailed);

            return steamTransaction.id;
        }

        #endregion

        #region Login / Logout

        public static async Task<LoginResponse> Login(string accessToken, string currentServer)
        {
            var tokenResponse = await TokenLogin.Get(accessToken); // retrieve token login info
            if (tokenResponse.result != RequestResult.Success)
            {
                switch (tokenResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        return new LoginResponse(null, LoginResult.TokenNotFound);
                    default:
                        return new LoginResponse(null, LoginResult.AwsServerError);
                }
            }

            return await LoginToServer(tokenResponse.item.accountId, currentServer);
        }

        public static async Task<LoginResponse> Login(string email, string hash, string currentServer)
        {
            var emailResponse = await EmailLogin.Get(email.ToLower()); // retrieve email login info
            if (emailResponse.result != RequestResult.Success)
            {
                switch (emailResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        return new LoginResponse(null, LoginResult.TokenNotFound);
                    default:
                        return new LoginResponse(null, LoginResult.AwsServerError);
                }
            }

            if (!hash.Equals(emailResponse.item.hash, StringComparison.Ordinal)) // check hash
            {
                return new LoginResponse(null, LoginResult.HashConditionFailed);
            }

            return await LoginToServer(emailResponse.item.accountId, currentServer);
        }

        public static async Task<LoginResponse> Login(ulong accountId, string currentServer)
        {
            return await LoginToServer(accountId, currentServer);
        }

        private static async Task<LoginResponse> LoginToServer(ulong accountId, string currentServer)
        {
            var accountLock = new AccountLock()
            {
                accountId = accountId,
                server = currentServer,
                creationDate = DateTime.UtcNow
            };
#if DEBUG
            var lockResponse = await accountLock.Put("attribute_not_exists(accountId) OR creationDate < :date", new Dictionary<string, AttributeValue> { { ":date", new AttributeValue { N = DateTime.UtcNow.AddMinutes(10).Ticks.ToString() } } });
#else
            var lockResponse = await accountLock.Put("attribute_not_exists(accountId) OR creationDate < :date", new Dictionary<string, AttributeValue> { { ":date", new AttributeValue { N = DateTime.UtcNow.AddMinutes(-10).Ticks.ToString() } } });
#endif
            if (lockResponse.result != RequestResult.Success)
            {
                switch (lockResponse.result)
                {
                    case RequestResult.ConditionalCheckFailed:
                        return new LoginResponse(null, LoginResult.AccountInUse);
                    default:
                        return new LoginResponse(null, LoginResult.AwsServerError);
                }
            }

            var accountResponse = await Account.Get(accountId); // retrieve account
            if (accountResponse.result != RequestResult.Success)
            {
                return new LoginResponse(null, LoginResult.AwsServerError);
            }

            accountResponse.item.CheckItemContainerIds();

            return new LoginResponse(accountResponse.item, LoginResult.Success);
        }

        public static async Task<LogoutResponse> Logout(Account account, string server)
        {
            await SaveItems(account.vaultItems);

            var putResponse = await account.Put();
            if (putResponse.result != RequestResult.Success)
            {
                return new LogoutResponse(account, LogoutResult.AwsServerError);
            }

            var lockRemovalResponse = await AccountLock.Delete(account.id, server);
            if (lockRemovalResponse.result != RequestResult.Success)
            {
                return new LogoutResponse(account, LogoutResult.FailedToRemoveLock);
            }

            return new LogoutResponse(account, LogoutResult.Success);
        }

        public static async Task<bool> HeartbeatLock(Account account, string server)
        {
            var accountLock = new AccountLock()
            {
                accountId = account.id,
                server = server,
                creationDate = DateTime.UtcNow
            };
            var response = await accountLock.Put("server = :ser", new Dictionary<string, AttributeValue> { { ":ser", new AttributeValue { S = server } } });
            return response.result == RequestResult.Success;
        }

#endregion

        #region Web Server

        public static async Task<PutResponse> GiveGold(string accessToken, int amount, string server)
        {
            var loginResponse = await Login(accessToken, server);
            if (loginResponse.result != LoginResult.Success)
            {
                switch (loginResponse.result)
                {
                    default:
                        return new PutResponse(RequestResult.InternalServerError);
                }
            }

            var account = loginResponse.account;
            account.premiumCurrency += amount;
            var logout = await Logout(account, server);
            return new PutResponse(RequestResult.Success);
        }

        public static async Task<WebLoginResponse> WebLogin(string email, string hash)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(hash))
                return new WebLoginResponse(WebLoginResult.InvalidRequest, null);

            var emailResponse = await EmailLogin.Get(email.ToLower()); // retrieve email login info
            if (emailResponse.result != RequestResult.Success)
            {
                switch (emailResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        return new WebLoginResponse(WebLoginResult.InvalidEmail, null);
                    default:
                        return new WebLoginResponse(WebLoginResult.InternalServerError, null);
                }
            }

            if (!Account.CreateHash(hash, emailResponse.item.creationDate).Equals(emailResponse.item.hash, StringComparison.Ordinal)) // check hash
            {
                return new WebLoginResponse(WebLoginResult.InvalidHash, null);
            }

            return new WebLoginResponse(WebLoginResult.Success, emailResponse.item.accessToken);
        }

        public static async Task<WebDescribeResponse> WebDescribe(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return new WebDescribeResponse(WebDescribeResult.InvalidRequest);

            var tokenResponse = await TokenLogin.Get(accessToken); // retrieve token login info
            if (tokenResponse.result != RequestResult.Success)
            {
                switch (tokenResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        return new WebDescribeResponse(WebDescribeResult.InvalidToken);
                    default:
                        return new WebDescribeResponse(WebDescribeResult.InternalServerError);
                }
            }

            var accountResponse = await Account.Get(tokenResponse.item.accountId); // retrieve account
            if (accountResponse.result != RequestResult.Success)
            {
                return new WebDescribeResponse(WebDescribeResult.InternalServerError);
            }
            var account = accountResponse.item;

            if (!account.verifiedEmail)
            {
                return new WebDescribeResponse(WebDescribeResult.UnverifiedEmail);
            }

            if (account.playerName == "*")
            {
                return new WebDescribeResponse(WebDescribeResult.NameRequired);
            }

            var characters = new List<Character>();
            foreach (var charId in account.characters)
            {
                var characterResponse = await Character.Get(charId);
                if (characterResponse.result != RequestResult.Success)
                {
                    switch (characterResponse.result)
                    {
                        default:

                            break;
                    }
                    continue;
                }
                characters.Add(characterResponse.item);
            }

            var webCharacters = new WebCharacterInfo[characters.Count];
            for (int i = 0; i < webCharacters.Length; i++)
                webCharacters[i] = await characters[i].GetWebInfo();

            return new WebDescribeResponse(
                WebDescribeResult.Success, 
                account.id,
                account.premiumCurrency, 
                account.playerName, 
                account.email, 
                account.maxCharacters, 
                account.classQuests.Values.ToArray(), 
                webCharacters, 
                account.unlockedItems.ToArray());
        }

        public static async Task<EmailVerificationResult> VerifyEmail(string verificationToken)
        {
            var verifyResponse = await EmailVerification.Get(verificationToken);
            if (verifyResponse.result != RequestResult.Success)
            {
                switch (verifyResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        return EmailVerificationResult.LinkExpired;
                    default:
                        return EmailVerificationResult.InternalServerError;
                }
            }

            var accountResponse = await Account.Get(verifyResponse.item.accountId); // retrieve account
            if (accountResponse.result != RequestResult.Success)
            {
                return EmailVerificationResult.InternalServerError;
            }

            var account = accountResponse.item;
            if (string.IsNullOrEmpty(account.verificationToken) || !account.verificationToken.Equals(verificationToken, StringComparison.Ordinal))
            {
                return EmailVerificationResult.LinkExpired;
            }

            if (account.verifiedEmail) return EmailVerificationResult.Success;

            account.verifiedEmail = true;
            var saveResponse = await account.Put();
            if (saveResponse.result != RequestResult.Success)
            {
                return EmailVerificationResult.InternalServerError;
            }

            return EmailVerificationResult.Success;
        }

        public static async Task<WebNameChangeResponse> ChangeName(string fromName, string toName, string reservationToken, string accessToken, string server)
        {
            if (!NetConstants.IsValidUsername(toName))
            {
                return new WebNameChangeResponse(WebNameChangeResult.InvalidRequest, null);
            }

            if (fromName != "*")
                return new WebNameChangeResponse(WebNameChangeResult.InvalidRequest, null);

            var loginResponse = await Login(accessToken, server);
            if (loginResponse.result != LoginResult.Success)
            {
                switch (loginResponse.result)
                {
                    default:
                        return new WebNameChangeResponse(WebNameChangeResult.InvalidRequest, null);
                }
            }

            var account = loginResponse.account;
            LogoutResponse logout = null;
            try
            {
                if (!account.playerName.Equals(fromName, StringComparison.Ordinal))
                    return new WebNameChangeResponse(WebNameChangeResult.InvalidRequest, null);

                var existingReservationResponse = await NameReservation.Get(toName);
                var nameReservation = existingReservationResponse.item;
                switch (existingReservationResponse.result)
                {
                    case RequestResult.ResourceNotFound:
                        nameReservation = new NameReservation();
                        break;
                    case RequestResult.Success:

                        if (string.IsNullOrWhiteSpace(nameReservation.reservationToken))
                            return new WebNameChangeResponse(WebNameChangeResult.NameTaken, null);

                        if (!nameReservation.reservationToken.Equals(reservationToken, StringComparison.Ordinal))
                            return new WebNameChangeResponse(WebNameChangeResult.NameTaken, null);

                        var deleteResponse = await NameReservation.Delete(nameReservation.playerName, nameReservation.accountId);
                        if (deleteResponse.result != RequestResult.Success)
                            return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);

                        break;
                    default:
                        return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);
                }

                nameReservation.reservationToken = null;
                nameReservation.playerName = toName.ToLower();
                nameReservation.creationDate = DateTime.UtcNow;
                nameReservation.accountId = account.id;

                var nameResponse = await nameReservation.Put("attribute_not_exists(playerName)");
                if (nameResponse.result != RequestResult.Success)
                {
                    switch (nameResponse.result)
                    {
                        case RequestResult.ConditionalCheckFailed:
                            return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);
                        default:
                            return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);
                    }
                }

                account.playerName = toName;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                logout = await Logout(account, server);
            }

            if (logout.result != LogoutResult.Success)
                return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);

            if (!account.playerName.Equals(toName, StringComparison.Ordinal))
                return new WebNameChangeResponse(WebNameChangeResult.InternalServerError, null);

            return new WebNameChangeResponse(WebNameChangeResult.Success, toName);
        }

        #endregion

        #region Server Items Load/Save

        public static async Task<LoadItemsResponse> LoadItems(List<ulong> itemIds)
        {
            var response = new LoadItemsResponse();
            var serverItems = new List<ServerItem>();
            for (int i = 0; i < itemIds.Count; i++)
            {
                var itemId = itemIds[i];
                if (itemId == 0)
                    serverItems.Add(null);
                else
                {
                    int retryCount = 0;
                    do
                    {
                        var loadResponse = await ServerItem.Get(itemId);
                        switch (loadResponse.result)
                        {
                            case ServerItemGetResult.Success:
                                serverItems.Add(loadResponse.item);
                                break;
                            case ServerItemGetResult.DoesntExist:
                                serverItems.Add(null);
                                break;
                            default:
                                if (++retryCount >= 2)
                                {
                                    response.result = LoadItemsResult.AwsError;
                                    return response;
                                }
                                break;
                        }
                    } while (retryCount > 0);
                }
            }

            response.result = LoadItemsResult.Success;
            response.items = serverItems;
            return response;
        }

        public static async Task SaveItems(List<ServerItem> serverItems)
        {
            foreach (var item in serverItems)
            {
                if (item == null) continue;
                int retryCount = 0;
                do
                {
                    var response = await item.Put("attribute_exists(id)");
                    switch (response.result)
                    {
                        case RequestResult.Success:

                            break;
                        default:
                            if (++retryCount >= 2)
                                retryCount = 0;
                            break;
                    }
                } while (retryCount > 0);
            }
        }

        #endregion

        #region Leaderboards

        public static async Task<LoadCharactersResponse> LoadLeaderboardCharacters(Leaderboard leaderboard)
        {
            var response = new LoadCharactersResponse();
            var characters = new List<Character>();
            var now = TimeUtils.ToEpochSeconds(DateTime.UtcNow);
            for (int i = 0; i < leaderboard.characterIds.Count; i++)
            {
                var timestamp = leaderboard.timestamps[i];
                var characterId = leaderboard.characterIds[i];

                if (characterId == 0 || (now - timestamp) > GetMaxSeconds((LeaderboardType)leaderboard.id)) // null character or expired
                {
                    leaderboard.RemoveAt(i);
                    i--;
                }
                else
                {
                    int retryCount = 0;
                    do
                    {
                        var loadResponse = await Character.Get(characterId);
                        switch (loadResponse.result)
                        {
                            case RequestResult.Success:
                                if (((LeaderboardType)leaderboard.id) == LeaderboardType.Living && loadResponse.item.dead)
                                {
                                    leaderboard.RemoveAt(i);
                                    i--;
                                }
                                else
                                    characters.Add(loadResponse.item);
                                break;
                            case RequestResult.ResourceNotFound:
                                leaderboard.RemoveAt(i);
                                i--;
                                break;
                            default:
                                if (++retryCount >= 2)
                                {
                                    response.result = LoadCharactersResult.AwsError;
                                    return response;
                                }
                                break;
                        }
                    } while (retryCount > 0);
                }
            }

            response.result = LoadCharactersResult.Success;
            response.characters = characters;
            return response;
        }

        private static ulong GetMaxSeconds(LeaderboardType type)
        {
            switch (type)
            {
                case LeaderboardType.Monthly:
                    return GetMaxSeconds(LeaderboardType.Weekly) * 4;
                case LeaderboardType.Weekly:
                    return 60 * 60 * 24 * 7;
                default:
                    return ulong.MaxValue;
            }
        }

        public static async Task<List<WebLeaderboardInfo>> DescribeLeaderboard(Leaderboard leaderboard)
        {
            var accounts = new Dictionary<ulong, Account>();
            var infos = new List<WebLeaderboardInfo>();

            for (int i = 0; i < leaderboard.characters.Count; i++)
            {
                var character = leaderboard.characters[i];
                if (!accounts.TryGetValue(character.accountId, out var account))
                {
                    var accountLoadResponse = await Account.Get(character.accountId);
                    if (accountLoadResponse.result != RequestResult.Success)
                        continue;
                    account = accountLoadResponse.item;
                    accounts.Add(account.id, account);
                }

                var equips = new Item[4];
                for (int j = 0; j < equips.Length; j++)
                {
                    if (j >= character.items.Count) break;
                    var serverItem = character.items[j];
                    if (serverItem != null)
                        equips[j] = serverItem.itemData;
                }

                var info = new WebLeaderboardInfo(account.playerName,
                    character.type, 
                    character.skin,
                    equips, 
                    leaderboard.values[i], 
                    ((LeaderboardType)leaderboard.id) == LeaderboardType.Living ? new CharacterStatistic[0] : character.statistics.Values.ToArray());
                infos.Add(info);
            }

            return infos;
        }
    }

    #endregion
}
