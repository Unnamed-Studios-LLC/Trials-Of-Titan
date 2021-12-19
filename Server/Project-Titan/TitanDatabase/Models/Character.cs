using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using TitanCore.Net.Web;
using Utils.NET.IO;
using Utils.NET.Logging;

namespace TitanDatabase.Models
{
    public class Character : Model
    {
        public static async Task<GetResponse<Character>> Get(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Characters, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } }, true);
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<Character>
                {
                    result = response.result,
                    item = null
                };

            var obj = new Character();
            obj.Read(new ItemReader(response.item));

            var itemLoadResponse = await Database.LoadItems(obj.itemIds);
            switch (itemLoadResponse.result)
            {
                case LoadItemsResult.AwsError:
                    return new GetResponse<Character>
                    {
                        result = RequestResult.InternalServerError,
                        item = null
                    };
                case LoadItemsResult.Success:
                    obj.items = itemLoadResponse.items;
                    break;
            }

            return new GetResponse<Character>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public override string TableName => Database.Table_Characters;

        public ulong id;

        public ulong accountId;

        public ushort type;

        public uint experience;

        public uint level;

        public List<uint> stats;

        public List<uint> statsLocked;

        public List<ulong> itemIds;

        public bool dead;

        public ulong deathValue;

        public ushort killer;

        public DateTime creationDate;

        public ulong souls;

        public ushort pet;

        public ushort skin;

        public byte levelIncrement;

        public List<ServerItem> items;

        public Dictionary<CharacterStatisticType, CharacterStatistic> statistics = new Dictionary<CharacterStatisticType, CharacterStatistic>();

        public Character()
        {

        }

        public override void Read(ItemReader r)
        {
            id = r.UInt64("id");
            accountId = r.UInt64("accountId");
            type = r.UInt16("type");
            experience = r.UInt32("experience");
            level = r.UInt32("level");
            stats = r.UInt32List("stats");
            statsLocked = r.UInt32List("statsLocked");
            itemIds = r.UInt64List("itemIds");
            dead = r.Bool("dead");
            if (dead)
                deathValue = r.UInt64("deathValue");
            killer = r.UInt16("killer");
            creationDate = r.Date("creationDate", DateTime.UtcNow);
            souls = r.UInt64("souls");
            pet = r.UInt16("pet");
            skin = r.UInt16("skin");
            levelIncrement = r.UInt8("lvlInc");
            SetStatistics(r.UInt64List("charstats"));
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            w.Write("accountId", accountId);
            w.Write("type", type);
            w.Write("experience", experience);
            w.Write("level", level);
            w.Write("stats", stats);
            w.Write("statsLocked", statsLocked);
            w.Write("itemIds", itemIds);
            w.Write("dead", dead);
            if (dead)
                w.Write("deathValue", deathValue);
            w.Write("killer", killer);
            w.Write("creationDate", creationDate);
            w.Write("souls", souls);
            w.Write("pet", pet);
            w.Write("skin", skin);
            w.Write("lvlInc", levelIncrement);
            w.Write("charstats", ExportStatistics());
        }

        private void SetStatistics(List<ulong> binaries)
        {
            foreach (var binary in binaries)
            {
                var stat = new CharacterStatistic(binary);
                statistics[stat.type] = stat;
            }
        }

        private List<ulong> ExportStatistics()
        {
            var binaries = new List<ulong>();
            foreach (var stat in statistics.Values)
                binaries.Add(stat.ToBinary());
            return binaries;
        }

        public void CheckItemContainerIds()
        {
            for (int i = 0; i < items.Count; i++)
            {
                var serverItem = items[i];
                if (serverItem == null) continue;
                if (serverItem.containerId == id) continue;
                items[i] = null;
                itemIds[i] = 0;
            }
        }

        public async Task<WebCharacterInfo> GetWebInfo()
        {
            var serverItems = new Item[4];
            for (int i = 0; i < itemIds.Count && i < serverItems.Length; i++)
            {
                var itemId = itemIds[i];
                if (itemId == 0) continue;

                var response = await ServerItem.Get(itemId);
                switch (response.result)
                {
                    case ServerItemGetResult.Success:
                        serverItems[i] = response.item.itemData;
                        break;
                    case ServerItemGetResult.DoesntExist:
                        itemIds[i] = 0;
                        break;
                }
            }

            return new WebCharacterInfo()
            {
                id = id,
                type = type,
                skin = skin,
                equips = serverItems
            };
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
