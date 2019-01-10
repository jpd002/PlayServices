using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices.DataModel
{
    public class UserService
    {
        static readonly string g_env_ps_builds_aws_access_key = "ps_builds_aws_access_key";
        static readonly string g_env_ps_builds_aws_access_secret = "ps_builds_aws_access_secret";
        static readonly string g_env_ps_users_table_name = "play_users_test";

        private AmazonDynamoDBClient CreateDynamoDbClient()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable(g_env_ps_builds_aws_access_key);
            var awsSecretKey = Environment.GetEnvironmentVariable(g_env_ps_builds_aws_access_secret);
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            return new AmazonDynamoDBClient(creds, RegionEndpoint.USWest2);
        }

        public Task<User> GetUser(Guid id)
        {
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = g_env_ps_users_table_name };
            return context.LoadAsync<User>(id, cfg);
        }

        public Task SaveUser(User user)
        {
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = g_env_ps_users_table_name };
            return context.SaveAsync(user, cfg);
        }
    };
}
