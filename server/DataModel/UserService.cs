using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PlayServices.DataModel
{
    public class UserService
    {
        AmazonDynamoDBClient _client;

        public UserService()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_key);
            var awsSecretKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_secret);
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            _client = new AmazonDynamoDBClient(creds, RegionEndpoint.USWest2);
        }

        #region Serialization Boilerplate
        
        private Dictionary<string, AttributeValue> ConvertUserToDynamoDbItem(User user)
        {
            var item = new Dictionary<string, AttributeValue>();
            item.Add("id", new AttributeValue() { S = user.Id.ToString() });
            item.Add("patreonId", new  AttributeValue() { N = user.PatreonId.ToString() });
            return item;
        }

        private User ConvertUserFromDynamoDbItem(Dictionary<string, AttributeValue> item)
        {
            var user = new User();
            AttributeValue value;
            if(item.TryGetValue("id", out value)) user.Id = Guid.Parse(value.S);
            if(item.TryGetValue("patreonId", out value)) user.PatreonId = uint.Parse(value.N);
            return user;
        }

        #endregion

        public Task<User> GetUser(Guid id)
        {
            return null;
        }

        public async Task<User> GetUserFromPatreonId(uint patreonId)
        {
            var request = new QueryRequest();
            request.TableName = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_users_dynamodb_table_name);
            request.IndexName = "patreonId-index";
            request.KeyConditionExpression = "patreonId = :patreonId";
            request.ExpressionAttributeValues.Add(":patreonId", new AttributeValue() { N = patreonId.ToString() });
            var response = await _client.QueryAsync(request);

            if(response.Count == 0) return null;

            var user = ConvertUserFromDynamoDbItem(response.Items[0]);
            return user;
        }

        public async Task SaveUser(User user)
        {
            var request = new PutItemRequest();
            request.TableName = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_users_dynamodb_table_name);
            request.Item = ConvertUserToDynamoDbItem(user);
            request.ConditionExpression = "attribute_not_exists(id)";
            await _client.PutItemAsync(request);
        }
    };
}
