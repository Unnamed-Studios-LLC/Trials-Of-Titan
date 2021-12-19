using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class EmailVerification : Model
    {
        public static async Task<GetResponse<EmailVerification>> Get(string token)
        {
            var request = new GetItemRequest(Database.Table_Verification, new Dictionary<string, AttributeValue>() { { "verifyToken", new AttributeValue { S = token } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<EmailVerification>
                {
                    result = response.result,
                    item = null
                };

            var obj = new EmailVerification();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<EmailVerification>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(string token, ulong accountId)
        {
            var request = new DeleteItemRequest(Database.Table_Verification, new Dictionary<string, AttributeValue>() { { "verifyToken", new AttributeValue { S = token } } });
            request.ConditionExpression = "accountId = :id";
            request.ExpressionAttributeValues[":id"] = new AttributeValue() { N = accountId.ToString() };
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Verification;

        public string verifyToken;

        public ulong accountId;

        public ulong ttl;

        public override void Read(ItemReader r)
        {
            verifyToken = r.String("verifyToken");
            accountId = r.UInt64("accountId");
            ttl = r.UInt64("ttl");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("verifyToken", verifyToken);
            w.Write("accountId", accountId);
            w.Write("ttl", ttl);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
