using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Data.Entities;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace TitanDatabase.Models
{
    public class Account : Model
    {
        private const int Default_Vault_Space = 8;

        public static async Task<GetResponse<Account>> Get(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Accounts, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } }, true);
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<Account>
                {
                    result = response.result,
                    item = null
                };

            var account = new Account();
            account.Read(new ItemReader(response.item));

            var itemLoadResponse = await Database.LoadItems(account.vaultIds);
            switch (itemLoadResponse.result)
            {
                case LoadItemsResult.AwsError:
                    return new GetResponse<Account>
                    {
                        result = RequestResult.InternalServerError,
                        item = null
                    };
                case LoadItemsResult.Success:
                    account.vaultItems = itemLoadResponse.items;
                    break;
            }

            return new GetResponse<Account>
            {
                result = RequestResult.Success,
                item = account
            };
        }

        public static async Task<DeleteResponse> Delete(ulong id)
        {
            var request = new DeleteItemRequest(Database.Table_Accounts, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } });
            return await DeleteItemAsync(request);
        }

        private static SHA256 sha = SHA256.Create();

        public static string CreateHash(string input, DateTime creationDate)
        {
            var salted = input + creationDate.Ticks.ToString();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salted));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public List<Item> GetDefaultVault(List<Item> current)
        {
            var list = new List<Item>();
            for (int i = 0; i < Default_Vault_Space; i++)
            {
                if (current != null && i < current.Count)
                    list.Add(current[i]);
                list.Add(Item.Blank);
            }
            return list;
        }

        public override string TableName => Database.Table_Accounts;

        public ulong id;

        public string playerName;

        public long premiumCurrency = 0;

        public long deathCurrency = 0;

        public int maxCharacters = 1;

        public string email;

        public Rank rank = Rank.Player;

        public string verificationToken;

        public bool verifiedEmail = false;

        public DateTime lastSeen = DateTime.UtcNow;

        public DateTime bannedUntil = DateTime.UtcNow;

        public DateTime mutedUntil = DateTime.UtcNow;

        public DateTime creationDate = DateTime.UtcNow;

        public List<ulong> lockedPlayers = new List<ulong>();

        public List<ulong> characters = new List<ulong>();

        public List<ulong> deaths = new List<ulong>();

        public List<ulong> vaultIds = new List<ulong>();

        public HashSet<uint> unlockedItems = new HashSet<uint>();

        public List<ServerItem> vaultItems;

        public Dictionary<ClassType, ClassQuest> classQuests = new Dictionary<ClassType, ClassQuest>();

        public int givenRewards = 0;

        public Account()
        {

        }

        public override void Read(ItemReader r)
        {
            id = r.UInt64("id");
            playerName = r.String("playerName", "");
            premiumCurrency = r.Int64("premiumCurrency");
            deathCurrency = r.Int64("deathCurrency");
            maxCharacters = r.Int32("maxCharacters");
            email = r.String("email");
            rank = (Rank)r.UInt8("rank", 0);
            verificationToken = r.String("verificationToken");
            verifiedEmail = r.Bool("verifiedEmail");
            lastSeen = r.Date("lastSeen", DateTime.UtcNow);
            bannedUntil = r.Date("bannedUntil", DateTime.UtcNow);
            mutedUntil = r.Date("mutedUntil", DateTime.UtcNow);
            creationDate = r.Date("creationDate", DateTime.UtcNow);
            lockedPlayers = r.UInt64List("lockedPlayers");
            characters = r.UInt64List("characters");
            deaths = r.UInt64List("deaths");
            vaultIds = r.UInt64List("vaultIds");
            unlockedItems = new HashSet<uint>(r.UInt32List("items"));
            SetClassQuests(r.UInt32List("classQuests"));
            givenRewards = r.Int32("givenRewards");

            ExpandVault(vaultIds);
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            w.Write("playerName", playerName);
            w.Write("premiumCurrency", premiumCurrency);
            w.Write("deathCurrency", deathCurrency);
            w.Write("maxCharacters", maxCharacters);
            w.Write("email", email);
            w.Write("rank", (byte)rank);
            w.Write("verificationToken", verificationToken);
            w.Write("verifiedEmail", verifiedEmail);
            w.Write("lastSeen", lastSeen);
            w.Write("bannedUntil", bannedUntil);
            w.Write("mutedUntil", mutedUntil);
            w.Write("creationDate", creationDate);
            w.Write("lockedPlayers", lockedPlayers);
            w.Write("characters", characters);
            w.Write("deaths", deaths);
            w.Write("vaultIds", vaultIds);
            w.Write("items", unlockedItems.ToList());
            w.Write("classQuests", ExportClassQuestBinaries());
            w.Write("givenRewards", givenRewards);
        }

        private void SetClassQuests(List<uint> binaries)
        {
            foreach (var binary in binaries)
            {
                var quest = new ClassQuest(binary);
                classQuests.Add((ClassType)quest.classId, quest);
            }
        }

        private List<uint> ExportClassQuestBinaries()
        {
            var binaries = new List<uint>();
            foreach (var classQuest in classQuests.Values)
                binaries.Add(classQuest.ToBinary());
            return binaries;
        }

        public void CheckItemContainerIds()
        {
            for (int i = 0; i < vaultItems.Count; i++)
            {
                var serverItem = vaultItems[i];
                if (serverItem == null) continue;
                if (serverItem.containerId == id) continue;
                vaultItems[i] = null;
                vaultIds[i] = 0;
            }
        }

        private void ExpandVault(List<ulong> list)
        {
            for (int i = list.Count; i < Default_Vault_Space; i++)
                list.Add(0);
        }

        public bool CanCreateCharacter(CharacterInfo info)
        {
            foreach (var requirement in info.requirements)
            {
                bool found = false;
                foreach (var quest in classQuests.Values)
                {
                    if (quest.classId != (ushort)requirement.classType) continue;
                    if (quest.GetCompletedCount() < requirement.questRequirement) return false;
                    found = true;
                    break;
                }

                if (!found)
                    return false;
            }
            return true;
        }

        public ClassQuest GetClassQuest(ClassType type)
        {
            if (!classQuests.TryGetValue(type, out var quest))
                return new ClassQuest((ushort)type, 0);
            return quest;
        }

        public void CompleteClassQuest(ClassType type, int index)
        {
            var quest = GetClassQuest(type);
            quest.CompleteQuest(index);
            classQuests[type] = quest;
        }

        public int GetClassQuestCompletedCount()
        {
            int count = 0;
            foreach (var quest in classQuests.Values)
                for (int i = 0; i < 4; i++)
                    if (quest.HasCompletedQuest(i))
                        count++;
            return count;
        }

        public void AccountReward1()
        {
            maxCharacters++;
        }

        public void AccountReward2()
        {
            for (int i = 0; i < 8; i++)
            {
                vaultIds.Add(0);
                vaultItems.Add(null);
            }
        }

        public void AccountReward3()
        {
            maxCharacters++;

            for (int i = 0; i < 8; i++)
            {
                vaultIds.Add(0);
                vaultItems.Add(null);
            }
        }

        public void UnlockItem(uint item)
        {
            unlockedItems.Add(item);
        }

        public bool HasUnlockedItem(uint item)
        {
            return unlockedItems.Contains(item);
        }

        public void CharacterDied(ulong id)
        {
            characters.Remove(id);
            deaths.Insert(0, id);
            if (deaths.Count > 20)
                deaths.RemoveAt(deaths.Count - 1);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
