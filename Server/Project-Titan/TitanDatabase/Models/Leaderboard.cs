using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TitanDatabase.Leaderboards;
using Utils.NET.Utils;

namespace TitanDatabase.Models
{
    public class Leaderboard : Model
    {
        private const float Leaderboard_Refresh_Min = 5;

        public const float Max_Leaderboard_Count = 20;

        public static async Task<GetResponse<Leaderboard>> GetOrCreate(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Leaderboards, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } }, true);
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
            {
                switch (response.result)
                {
                    case RequestResult.ResourceNotFound:
                        var createResponse = await Create(id);
                        return createResponse;
                    default:
                        return new GetResponse<Leaderboard>
                        {
                            result = response.result,
                            item = null
                        };
                }
            }

            var obj = new Leaderboard();
            obj.Read(new ItemReader(response.item));

            var loadResponse = await Database.LoadLeaderboardCharacters(obj);
            if (loadResponse.result != LoadCharactersResult.Success)
            {
                return new GetResponse<Leaderboard>
                {
                    result = RequestResult.InternalServerError,
                    item = null
                };
            }

            obj.characters = loadResponse.characters;

            return new GetResponse<Leaderboard>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        private static async Task<GetResponse<Leaderboard>> Create(ulong id)
        {
            var leaderboard = Default(id);

            var putResponse = await leaderboard.Put("attribute_not_exists(id)");
            return new GetResponse<Leaderboard>()
            {
                result = putResponse.result,
                item = leaderboard
            };
        }

        public static Leaderboard Default(ulong id) => new Leaderboard()
        {
            id = id,
            versionId = 1,
            characterIds = new List<ulong>(),
            values = new List<ulong>(),
            timestamps = new List<ulong>(),
            characters = new List<Character>()
        };

        public override string TableName => Database.Table_Leaderboards;

        public ulong id;

        public uint versionId;

        public List<ulong> characterIds;

        public List<ulong> values;

        public List<ulong> timestamps;

        public List<Character> characters;

        public DateTime lastUpdated = DateTime.Now;

        public bool needsReload = false;

        public Leaderboard()
        {

        }

        public override void Read(ItemReader r)
        {
            id = r.UInt64("id");
            versionId = r.UInt32("versionId");
            characterIds = r.UInt64List("characterIds");
            values = r.UInt64List("values");
            timestamps = r.UInt64List("timestamps");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            w.Write("versionId", versionId);
            w.Write("characterIds", characterIds);
            w.Write("values", values);
            w.Write("timestamps", timestamps);
        }

        public bool Outdated()
        {
            return needsReload || (DateTime.Now - lastUpdated).TotalMinutes > Leaderboard_Refresh_Min;
        }

        public async Task<bool> Insert(int index, Character character, ulong value)
        {
            characterIds.Insert(index, character.id);
            values.Insert(index, value);
            characters.Insert(index, character);
            timestamps.Insert(index, TimeUtils.ToEpochSeconds(DateTime.UtcNow));

            Truncate();

            if (!await PushUpdate())
            {
                needsReload = true;
                return false;
            }

            return true;
        }

        private void Truncate()
        {
            while (characterIds.Count > Max_Leaderboard_Count)
            {
                RemoveAt(characterIds.Count - 1);
            }
        }

        public void Remove(ulong characterId)
        {
            for (int i = 0; i < characterIds.Count; i++)
            {
                if (characterIds[i] == characterId)
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        public void RemoveAt(int index)
        {
            characterIds.RemoveAt(index);
            values.RemoveAt(index);
            timestamps.RemoveAt(index);
            if (characters != null)
                characters.RemoveAt(index);
        }

        public async Task<bool> RemoveFromLeaderboard(ulong characterId)
        {
            for (int i = 0; i < characterIds.Count; i++)
            {
                if (characterIds[i] == characterId)
                {
                    characterIds.RemoveAt(i);
                    values.RemoveAt(i);
                    characters.RemoveAt(i);
                    break;
                }
            }

            if (!await PushUpdate())
            {
                needsReload = true;
                return false;
            }

            return true;
        }

        private async Task<bool> PushUpdate()
        {
            var request = new PutItemRequest(TableName, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } });
            request.ConditionExpression = "versionId = :ver";
            request.ExpressionAttributeValues[":ver"] = new AttributeValue() { N = versionId.ToString() };
            versionId++;
            var putResponse = await Put(request);
            if (putResponse.result != RequestResult.Success)
            {
                return false;
            }
            return true;
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
