using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class IpBan : Model
    {
        public static async Task<GetResponse<IpBan>> Get(ulong accountId)
        {
            var request = new GetItemRequest(Database.Table_Ip_Ban, new Dictionary<string, AttributeValue>() { { "ip", new AttributeValue { S = accountId.ToString() } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<IpBan>
                {
                    result = response.result,
                    item = null
                };

            var obj = new IpBan();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<IpBan>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(ulong accountId, string server)
        {
            var request = new DeleteItemRequest(Database.Table_Ip_Ban, new Dictionary<string, AttributeValue>() { { "ip", new AttributeValue { S = accountId.ToString() } } });
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Ip_Ban;

        public string ip;

        public ulong ttl;

        public override void Read(ItemReader r)
        {
            ip = r.String("ip", "");
            ttl = r.UInt64("ttl");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("ip", ip);
            w.Write("ttl", ttl);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
