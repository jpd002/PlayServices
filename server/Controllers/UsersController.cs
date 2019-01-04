using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices.Server.Controllers
{
    class CreateResponse
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string GuestToken { get; set; } = string.Empty;
    };

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
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
            //Create guest user
            var guestToken = GenerateRandomCryptographicKey(32);

            var user = new User();
            user.Id = Guid.NewGuid();
            user.AuthMethods.Add(PlayServices.User.AUTHTYPE_GUEST, guestToken);

            //Save to DB
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = g_env_ps_users_table_name };
            await context.SaveAsync(user, cfg);

            //Return response
            var response = new CreateResponse();
            response.Id = user.Id;
            response.GuestToken = guestToken;
            return new JsonResult(response);
        }

        [HttpPost("{id}/sessions")]
        public async Task<ActionResult> CreateSession(Guid id, [FromForm] string authType, [FromForm] string authToken)
        {
            //Load from DB
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = g_env_ps_users_table_name };
            var user = await context.LoadAsync<User>(id, cfg);

            string userAuthToken = string.Empty;
            if(!user.AuthMethods.TryGetValue(authType, out userAuthToken))
            {
                throw new Exception("Invalid authentication type.");
            }

            if(userAuthToken != authToken)
            {
                return Unauthorized();
            }

            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("accessTokenKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(tokenHandler.WriteToken(token));
        }

        [Authorize("CanAccessSelfInfo")]
        [HttpPost("{id}/links")]
        public async Task<ActionResult> CreateLink(Guid id)
        {
            return Ok();
        }
    }
}
