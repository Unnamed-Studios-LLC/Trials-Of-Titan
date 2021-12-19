using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class TokenLogin : Model
    {
        public static async Task<GetResponse<TokenLogin>> Get(string token)
        {
            var request = new GetItemRequest(Database.Table_Token_Login, new Dictionary<string, AttributeValue>() { { "accessToken", new AttributeValue { S = token } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<TokenLogin>
                {
                    result = response.result,
                    item = null
                };

            var obj = new TokenLogin();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<TokenLogin>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(string token, ulong accountId)
        {
            var request = new DeleteItemRequest(Database.Table_Token_Login, new Dictionary<string, AttributeValue>() { { "accessToken", new AttributeValue { S = token } } });
            request.ConditionExpression = "accountId = :id";
            request.ExpressionAttributeValues[":id"] = new AttributeValue() { N = accountId.ToString() };
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Token_Login;

        public string accessToken;

        public ulong accountId;

        public override void Read(ItemReader r)
        {
            accessToken = r.String("accessToken");
            accountId = r.UInt64("accountId");
        }

        public override void Write(ItemWriter w)
        {
            w.Write("accessToken", accessToken);
            w.Write("accountId", accountId);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
