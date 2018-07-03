using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildsController : ControllerBase
    {
        static readonly string g_userName = "jpd002";
        static readonly string g_repositoryName = "Play-";
        static readonly string g_commitId = "0";

        class BuildInfo
        {
            [DynamoDBHashKey("commit")]
            public string Commit { get; set; }

            public PlayServices.Build Build { get; set; } = new PlayServices.Build();
        };

        private AmazonDynamoDBClient CreateDynamoDbClient()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("ps_builds_aws_access_key");
            var awsSecretKey = Environment.GetEnvironmentVariable("ps_builds_aws_access_secret");
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            return new AmazonDynamoDBClient(creds, RegionEndpoint.USWest2);
        }

        private async Task<PlayServices.Build> GetTopCommitInfo()
        {
            var ghToken = Environment.GetEnvironmentVariable("ps_gh_apitoken");
            var client = new GitHubClient(new ProductHeaderValue("PlayServices"));
            client.Credentials = new Credentials(ghToken);
            var masterCommit = await client.Repository.Commit.Get(g_userName, g_repositoryName, "heads/master");
            var masterStatus = await client.Repository.Status.GetCombined(g_userName, g_repositoryName, "heads/master");
            bool hasBuild = masterStatus.State == CommitState.Success;
            var build = new PlayServices.Build();
            build.CommitMessage = masterCommit.Commit.Message;
            build.CommitDate = masterCommit.Commit.Committer.Date.UtcDateTime;
            build.CommitHash = masterCommit.Sha.Substring(0, 8);
            build.HasBuild = masterStatus.State == CommitState.Success;
            build.Timestamp = DateTime.UtcNow;
            return build;
        }

        private async Task<PlayServices.Build> GetBuildInfo()
        {
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = "play_buildinfo_test" };
            var buildInfo = await context.LoadAsync<BuildInfo>(g_commitId, cfg);
            if(buildInfo == null) return null;
            //Fixup datetimes due to bug in conversion from DynamoDB
            buildInfo.Build.Timestamp = buildInfo.Build.Timestamp.ToUniversalTime();
            buildInfo.Build.CommitDate = buildInfo.Build.CommitDate.ToUniversalTime();
            return buildInfo.Build;
        }

        private async Task SetBuildInfo(PlayServices.Build buildInfo)
        {
            var client = CreateDynamoDbClient();
            var context = new DynamoDBContext(client);
            var cfg = new DynamoDBOperationConfig() { OverrideTableName = "play_buildinfo_test" };
            var bi = new BuildInfo();
            bi.Commit = g_commitId;
            bi.Build = buildInfo;
            await context.SaveAsync(bi, cfg);
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var buildInfo = await GetBuildInfo();
            if((DateTime.UtcNow - buildInfo.Timestamp).TotalSeconds > 60)
            {
                buildInfo = await GetTopCommitInfo();
                await SetBuildInfo(buildInfo);
            }
            return Ok(buildInfo);
        }
    }
}
