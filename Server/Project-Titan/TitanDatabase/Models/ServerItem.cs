using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Core;
using Utils.NET.Utils;

namespace TitanDatabase.Models
{
    public enum ServerItemGetResult
    {
        Success,
        DoesntExist,
        AwsError
    }

    public class ServerItemResponse
    {
        public ServerItemGetResult result;

        public ServerItem item;
    }

    public class ServerItem : Model
    {
        public static async Task<ServerItemResponse> Get(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Items, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } }, true);
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
            {
                return new ServerItemResponse()
                {
                    result = DecodeGetResult(response.result),
                    item = null
                };
            }

            var obj = new ServerItem();
            obj.Read(new ItemReader(response.item));
            obj.StoreValues(response.item);
            return new ServerItemResponse()
            {
                result = ServerItemGetResult.Success,
                item = obj
            };
        }

        private static ServerItemGetResult DecodeGetResult(RequestResult result)
        {
            switch (result)
            {
                case RequestResult.Success:
                    return ServerItemGetResult.Success;
                case RequestResult.ResourceNotFound:
                    return ServerItemGetResult.DoesntExist;
                default:
                    return ServerItemGetResult.AwsError;
            }
        }

        public static async Task<DeleteResponse> Delete(ulong id)
        {
            var request = new DeleteItemRequest(Database.Table_Items, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } });
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Items;

        public ulong id;

        public ulong containerId;

        public Item itemData;


        private ulong originalContainerId;

        private Item originalItem;

        public ServerItem()
        {

        }

        public override void Read(ItemReader r)
        {
            id = r.UInt64("id");
            containerId = originalContainerId = r.UInt64("cId");
            itemData = originalItem = Item.FromBinary(r.Binary("item").ToArray());
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            w.Write("cId", containerId);
            w.Write("item", new MemoryStream(itemData.ToBinary()));
            if (containerId == 0)
                w.Write("ttl", TimeUtils.ToEpochSeconds(DateTime.Now.AddDays(1)));
        }

        public override bool IsDifferent()
        {
            return originalItem != itemData || originalContainerId != containerId;
        }
    }
}
