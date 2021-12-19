using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class EmailLogin : Model
    {
        public static async Task<GetResponse<EmailLogin>> Get(string email)
        {
            var request = new GetItemRequest(Database.Table_Email_Login, new Dictionary<string, AttributeValue>() { { "email", new AttributeValue { S = email } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<EmailLogin>
                {
                    result = response.result,
                    item = null
                };

            var obj = new EmailLogin();
            obj.Read(new ItemReader(response.item));
            return new GetResponse<EmailLogin>
            {
                result = RequestResult.Success,
                item = obj
            };
        }

        public static async Task<DeleteResponse> Delete(string email, ulong accountId)
        {
            var request = new DeleteItemRequest(Database.Table_Email_Login, new Dictionary<string, AttributeValue>() { { "email", new AttributeValue { S = email } } });
            request.ConditionExpression = "accountId = :id";
            request.ExpressionAttributeValues[":id"] = new AttributeValue() { N = accountId.ToString() };
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Email_Login;

        public string email;

        public string hash;

        public ulong accountId;

        public string accessToken;

        public DateTime creationDate;

        public override void Read(ItemReader r)
        {
            email = r.String("email");
            hash = r.String("hash");
            accountId = r.UInt64("accountId");
            accessToken = r.String("accessToken");
            creationDate = r.Date("creationDate", DateTime.Now);
        }

        public override void Write(ItemWriter w)
        {
            w.Write("email", email);
            w.Write("hash", hash);
            w.Write("accountId", accountId);
            w.Write("accessToken", accessToken);
            w.Write("creationDate", creationDate);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
