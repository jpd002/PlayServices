using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3;

namespace PlayServices.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompatibilityController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_key);
            var awsSecretKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_secret);
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var client = new AmazonS3Client(creds, Amazon.RegionEndpoint.USWest2);
            var getRequest = new Amazon.S3.Model.GetObjectRequest
            {
                BucketName = "playcompatibility",
                Key = "compat_summary.json"
            };
            string responseBody;
            using (var response = await client.GetObjectAsync(getRequest))
            {
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    responseBody = reader.ReadToEnd();
                }
            }
            return Ok(responseBody);
        }
    }
}
