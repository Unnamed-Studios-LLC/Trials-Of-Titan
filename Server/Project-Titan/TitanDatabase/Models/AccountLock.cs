using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class AccountLock : Model
    {
        public static async Task<GetResponse<AccountLock>> Get(ulong accountId)
        {
            var request = new GetItemRequest(Database.Table_Account_Lock, new Dictionary<string, AttributeValue>() { { "accountId", new AttributeValue { N = accountId.ToString() } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<AccountLock>
                {
                    result = response.result,
                    item = null
                };

            var obj = new AccountLock();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<AccountLock>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(ulong accountId, string server)
        {
            var request = new DeleteItemRequest(Database.Table_Account_Lock, new Dictionary<string, AttributeValue>() { { "accountId", new AttributeValue { N = accountId.ToString() } } });
            request.ConditionExpression = "server = :ser";
            request.ExpressionAttributeValues[":ser"] = new AttributeValue() { S = server };
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Account_Lock;

        public ulong accountId;

        public string server;

        public DateTime creationDate;

        public override void Read(ItemReader r)
        {
            accountId = r.UInt64("accountId");
            server = r.String("server");
            creationDate = r.Date("creationDate", DateTime.UtcNow);
        }

        public override void Write(ItemWriter w)
        {
            w.Write("accountId", accountId);
            w.Write("server", server);
            w.Write("creationDate", creationDate);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
