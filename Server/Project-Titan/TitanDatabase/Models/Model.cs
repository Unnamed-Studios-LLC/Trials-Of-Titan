using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utils.NET.Logging;

namespace TitanDatabase.Models
{
    public abstract class Model
    {
        public enum RequestResult
        {
            Success,
            InternalServerError,
            ProvisionedThroughputExceeded,
            RequestLimitExceeded,
            ResourceNotFound,
            ConditionalCheckFailed,
            ItemCollectionSizeLimitExceeded,
            TransactionConflict
        }

        public class GetResponse<T>
        {
            public RequestResult result;

            public T item;
        }

        public class DeleteResponse
        {
            public RequestResult result;
        }

        public class PutResponse
        {
            public RequestResult result;

            public PutResponse() { }

            public PutResponse(RequestResult result)
            {
                this.result = result;
            }
        }

        protected static async Task<GetResponse<Dictionary<string, AttributeValue>>> GetItemAsync(GetItemRequest request)
        {
            GetItemResponse response = null;
            try
            {
                response = await Database.Client.GetItemAsync(request);
            }
            catch (InternalServerErrorException)
            {
                return new GetResponse<Dictionary<string, AttributeValue>>
                {
                    result = RequestResult.InternalServerError,
                    item = null
                };

            }
            catch (ProvisionedThroughputExceededException)
            {
                return new GetResponse<Dictionary<string, AttributeValue>>
                {
                    result = RequestResult.ProvisionedThroughputExceeded,
                    item = null
                };
            }
            catch (RequestLimitExceededException)
            {
                return new GetResponse<Dictionary<string, AttributeValue>>
                {
                    result = RequestResult.RequestLimitExceeded,
                    item = null
                };
            }
            catch (ResourceNotFoundException)
            {
                return new GetResponse<Dictionary<string, AttributeValue>>
                {
                    result = RequestResult.ResourceNotFound,
                    item = null
                };
            }

            if (response.Item.Keys.Count == 0)
                return new GetResponse<Dictionary<string, AttributeValue>>
                {
                    result = RequestResult.ResourceNotFound,
                    item = null
                };

            return new GetResponse<Dictionary<string, AttributeValue>>
            {
                result = RequestResult.Success,
                item = response.Item
            };
        }

        protected static async Task<DeleteResponse> DeleteItemAsync(DeleteItemRequest request)
        {
            DeleteItemResponse response;
            try
            {
                response = await Database.Client.DeleteItemAsync(request);
            }
            catch (ConditionalCheckFailedException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.ConditionalCheckFailed
                };
            }
            catch (InternalServerErrorException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.InternalServerError
                };
            }
            catch (ItemCollectionSizeLimitExceededException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.ItemCollectionSizeLimitExceeded
                };
            }
            catch (ProvisionedThroughputExceededException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.ProvisionedThroughputExceeded
                };
            }
            catch (RequestLimitExceededException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.RequestLimitExceeded
                };
            }
            catch (ResourceNotFoundException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.ResourceNotFound
                };
            }
            catch (TransactionConflictException)
            {
                return new DeleteResponse
                {
                    result = RequestResult.TransactionConflict
                };
            }

            return new DeleteResponse
            {
                result = RequestResult.Success
            };
        }

        protected static async Task<PutResponse> PutItemAsync(PutItemRequest request)
        {
            PutItemResponse response;
            try
            {
                response = await Database.Client.PutItemAsync(request);
            }
            catch (ConditionalCheckFailedException)
            {
                return new PutResponse
                {
                    result = RequestResult.ConditionalCheckFailed
                };
            }
            catch (InternalServerErrorException)
            {
                return new PutResponse
                {
                    result = RequestResult.InternalServerError
                };
            }
            catch (ItemCollectionSizeLimitExceededException)
            {
                return new PutResponse
                {
                    result = RequestResult.ItemCollectionSizeLimitExceeded
                };
            }
            catch (ProvisionedThroughputExceededException)
            {
                return new PutResponse
                {
                    result = RequestResult.ProvisionedThroughputExceeded
                };
            }
            catch (RequestLimitExceededException)
            {
                return new PutResponse
                {
                    result = RequestResult.RequestLimitExceeded
                };
            }
            catch (ResourceNotFoundException)
            {
                return new PutResponse
                {
                    result = RequestResult.ResourceNotFound
                };
            }
            catch (TransactionConflictException)
            {
                return new PutResponse
                {
                    result = RequestResult.TransactionConflict
                };
            }

            return new PutResponse
            {
                result = RequestResult.Success
            };
        }

        private Dictionary<string, AttributeValue> values;

        public void StoreValues(Dictionary<string, AttributeValue> values)
        {
            this.values = values;
        }

        public abstract bool IsDifferent();

        public Task<PutResponse> Put()
        {
            return Put(new PutItemRequest());
        }

        public Task<PutResponse> Put(string condition)
        {
            var request = new PutItemRequest();
            request.ConditionExpression = condition;
            return Put(request);
        }

        public Task<PutResponse> Put(string condition, Dictionary<string, AttributeValue> expressionValues)
        {
            var request = new PutItemRequest();
            request.ConditionExpression = condition;
            request.ExpressionAttributeValues = expressionValues;
            return Put(request);
        }

        public async Task<PutResponse> Put(PutItemRequest request)
        {
            request.TableName = TableName;

            var w = new ItemWriter();
            Write(w);
            request.Item = w.GetValues();

            if (!IsDifferent())
            {
                return new PutResponse(RequestResult.Success);
            }

            return await PutItemAsync(request);
        }

        public abstract string TableName { get; }

        public abstract void Read(ItemReader r);

        public abstract void Write(ItemWriter w);
    }
}
