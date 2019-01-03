using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices.Server.Controllers
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string GuestToken { get; set; } = string.Empty;
    };

    class User
    {
        [DynamoDBHashKey("id")]
        public Guid Id { get; set; } = Guid.Empty;

        public string GuestToken { get; set; } = string.Empty;

        public List<string> AuthMethods { get; set; } = new List<string>();
    };

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        static readonly string g_env_ps_builds_aws_access_key = "ps_builds_aws_access_key";
        static readonly string g_env_ps_builds_aws_access_secret = "ps_builds_aws_access_secret";

        private AmazonDynamoDBClient CreateDynamoDbClient()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable(g_env_ps_builds_aws_access_key);
            var awsSecretKey = Environment.GetEnvironmentVariable(g_env_ps_builds_aws_access_secret);
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            return new AmazonDynamoDBClient(creds, RegionEndpoint.USWest2);
        }

        public static string GenerateRandomCryptographicKey(int keyLength)
        {
            var generator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[keyLength];
            generator.GetNonZeroBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        [HttpPost]
        public async Task<JsonResult> Create()
        {
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = "play_users_test" };
            var user = new User();
            user.Id = Guid.NewGuid();
            user.GuestToken = GenerateRandomCryptographicKey(32);
            user.AuthMethods.Add("blah");
            user.AuthMethods.Add("blih");
            await context.SaveAsync(user, cfg);

            var response = new CreateUserResponse();
            response.Id = user.Id;
            response.GuestToken = user.GuestToken;
            return new JsonResult(response);
        }
    }
}
