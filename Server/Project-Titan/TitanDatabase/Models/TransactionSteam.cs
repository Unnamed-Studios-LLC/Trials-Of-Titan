using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class TransactionSteam : Model
    {
        public static async Task<GetResponse<TransactionSteam>> Get(ulong id)
        {
            var request = new GetItemRequest(Database.Table_Steam_Transactions, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<TransactionSteam>
                {
                    result = response.result,
                    item = null
                };

            var obj = new TransactionSteam();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<TransactionSteam>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(ulong id)
        {
            var request = new DeleteItemRequest(Database.Table_Steam_Transactions, new Dictionary<string, AttributeValue>() { { "id", new AttributeValue { N = id.ToString() } } });
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Steam_Transactions;

        public ulong id;

        public string transactionId;

        public ulong accountId;

        public uint itemId;

        public int finalized = 0;

        //public StoreType storeType;

        public override void Read(ItemReader r)
        {
            id = r.UInt64("id");
            transactionId = r.String("transactionId", null);
            accountId = r.UInt64("accountId");
            itemId = r.UInt32("itemId");
            finalized = r.Int32("finalized", 0);
            //storeType = (StoreType)r.UInt8("storeType");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("id", id);
            if (transactionId != null)
                w.Write("transactionId", transactionId);
            w.Write("accountId", accountId);
            w.Write("itemId", itemId);
            w.Write("finalized", finalized);
            //w.Write("storeType", (byte)storeType);
        }

        public override bool IsDifferent()
        {
            return true;
        }

        public async Task<bool> Consume(string transactionId)
        {
            this.transactionId = transactionId;
            if (finalized != 0) return false;
            finalized = 1;
            var putResponse = await Put("finalized = :fin", new Dictionary<string, AttributeValue> { { ":fin", new AttributeValue { N = "0" } } });
            return putResponse.result == RequestResult.Success;
        }
    }
}
