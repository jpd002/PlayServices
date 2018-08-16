using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.S3;

namespace PlayServices.CompatibilityValidator
{
    class Program
    {
        static CompatibilitySummary GenerateCompatibilitySummary(IEnumerable<GameCompatibility> gameCompats)
        {
            var stateCount = new Dictionary<string, int>();
            foreach(var gameCompat in gameCompats)
            {
                int currentCount = 0;
                stateCount.TryGetValue(gameCompat.State, out currentCount);
                stateCount[gameCompat.State] = currentCount + 1;
            }

            var summary = new CompatibilitySummary();
            foreach(var entry in stateCount)
            {
                var summaryItem = new CompatibilitySummaryItem();
                summaryItem.State = entry.Key;
                summaryItem.Count = entry.Value;
                summary.Items.Add(summaryItem);
            }
            summary.UpdateTime = DateTime.UtcNow;
            return summary;
        }

        static Task<Amazon.S3.Model.PutObjectResponse> UploadToS3(CompatibilitySummary summary)
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("ps_aws_access_key");
            var awsSecretKey = Environment.GetEnvironmentVariable("ps_aws_secret_key");
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var client = new AmazonS3Client(creds, Amazon.RegionEndpoint.USWest2);
            var summaryBody = JsonConvert.SerializeObject(summary);
            var putRequest = new Amazon.S3.Model.PutObjectRequest
            {
                BucketName = "playcompatibility",
                Key = "compat_summary.json",
                ContentBody = summaryBody
            };
            return client.PutObjectAsync(putRequest);
        }

        static void Main(string[] args)
        {
            try
            {
                var gameCompats = CompatibilityDataExtractor.GetGameCompatibilities();
                var summary = GenerateCompatibilitySummary(gameCompats);
                var task = UploadToS3(summary);
                task.Wait();
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Got an exception: {0}.", ex.Message);
                Environment.ExitCode = -1;
            }
        }
    }
}
