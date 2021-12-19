using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class TransactionAndroid : Model
    {
        public static async Task<GetResponse<TransactionAndroid>> Get(string id)
        {
            var request = new GetItemRequest(Database.Table_Android_Transactions, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { S = id.ToString() } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<TransactionAndroid>
                {
                    result = response.result,
                    item = null
                };

            var obj = new TransactionAndroid();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<TransactionAndroid>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(string id)
        {
            var request = new DeleteItemRequest(Database.Table_Android_Transactions, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { S = id.ToString() } } });
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Android_Transactions;

        public string id;

        public ulong accountId;

        //public StoreType storeType;

        public override void Read(ItemReader r)
        {
            id = r.String("id");
            accountId = r.UInt64("accountId");
            //storeType = (StoreType)r.UInt8("storeType");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            w.Write("accountId", accountId);
            //w.Write("storeType", (byte)storeType);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
