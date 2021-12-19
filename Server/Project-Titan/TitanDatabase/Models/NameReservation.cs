using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TitanDatabase.Models
{
    public class NameReservation : Model
    {
        public static async Task<GetResponse<NameReservation>> Get(string name)
        {
            var request = new GetItemRequest(Database.Table_Name_Reservation, new Dictionary<string, AttributeValue>() { { "playerName", new AttributeValue { S = name.ToLower() } } });
            var response = await GetItemAsync(request);

            if (response.result != RequestResult.Success)
                return new GetResponse<NameReservation>
                {
                    result = response.result,
                    item = null
                };

            var nameReservation = new NameReservation();
            nameReservation.Read(new ItemReader(response.item));
            return new GetResponse<NameReservation>
            {
                result = RequestResult.Success,
                item = nameReservation
            };
        }

        public static async Task<DeleteResponse> Delete(string name, ulong accountId)
        {
            var request = new DeleteItemRequest(Database.Table_Name_Reservation, new Dictionary<string, AttributeValue>() { { "playerName", new AttributeValue { S = name.ToLower() } } });
            request.ConditionExpression = "accountId = :id";
            request.ExpressionAttributeValues[":id"] = new AttributeValue() { N = accountId.ToString() };
            return await DeleteItemAsync(request);
        }

        public override string TableName => Database.Table_Name_Reservation;

        public string playerName;

        public ulong accountId;

        public string reservationToken;

        public DateTime creationDate;

        public override void Read(ItemReader r)
        {
            playerName = r.String("playerName");
            accountId = r.UInt64("accountId");
            reservationToken = r.String("resToken");
            creationDate = r.Date("creationDate", DateTime.UtcNow);
        }

        public override void Write(ItemWriter w)
        {
            w.Write("playerName", playerName);
            w.Write("accountId", accountId);
            w.Write("resToken", reservationToken);
            w.Write("creationDate", creationDate);
        }

        public override bool IsDifferent()
        {
            return true;
        }
    }
}
