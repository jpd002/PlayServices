using Amazon.S3;
using Amazon.S3.Model;
using PlayServices.DataModel;
using PlayServices.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayServices.Services
{
    public class GameDataService : IGameDataService
    {
        struct GameDataKey
        {
            public Guid UserId { get; set; }
            public string GameId { get; set; }
            public uint Index { get; set; }
        };

        const string _bucketName = "playservices-gamedata";
        const string _iconsBucketName = "playservices-gamedata-icons";
        readonly AmazonS3Client _s3Client = CreateS3Client();

        static string MakeKeyPrefix(Guid userId, string gameId)
        {
            return string.Format("{0}/{1}/", userId, gameId);
        }

        static string MakeKeyString(GameDataKey key)
        {
            uint reverseIndex = uint.MaxValue - key.Index;
            return string.Format("{0}/{1}/{2:D16}", key.UserId, key.GameId, reverseIndex);
        }

        static GameDataKey SplitKeyString(string keyString)
        {
            var parts = keyString.Split('/');
            if(parts.Length != 3)
            {
                throw new System.Exception("Invalid key format.");
            }
            uint reverseIndex = uint.Parse(parts[2]);
            var key = new GameDataKey()
            {
                UserId = Guid.Parse(parts[0]),
                GameId = parts[1],
                Index = uint.MaxValue - reverseIndex
            };
            return key;
        }

        static AmazonS3Client CreateS3Client()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_key);
            var awsSecretKey = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_builds_aws_access_secret);
            var creds = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var client = new AmazonS3Client(creds, Amazon.RegionEndpoint.USEast2);
            return client;
        }

        string GetDataIconUrl(Guid userId, string gameId)
        {
            var iconKey = string.Format("{0}/{1}.glb", userId.ToString(), gameId);
            var preSignedUrlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = _iconsBucketName,
                Key = iconKey,
                Expires = DateTime.Now.AddMinutes(15)
            };
            return _s3Client.GetPreSignedURL(preSignedUrlRequest);
        }

        public async Task<GameDataInfo> GetDataInfo(Guid userId, string gameId)
        {
            var prefix = MakeKeyPrefix(userId, gameId);
            var request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                Prefix = prefix,
                MaxKeys = 1,
            };
            var response = await _s3Client.ListObjectsAsync(request);
            if(response.S3Objects.Count == 0) return null;
            var s3Object = response.S3Objects[0];
            var key = SplitKeyString(s3Object.Key);
            var result = new GameDataInfo
            {
                GameId = gameId,
                CurrentIndex = key.Index,
                LastModifiedDate = s3Object.LastModified,
                IconUrl = GetDataIconUrl(userId, gameId)
            };
            return result;
        }

        public async Task<IList<GameDataInfo>> GetAvailableData(Guid userId)
        {
            const string delimiter = "/";
            var request = new ListObjectsRequest
            {
                BucketName = _bucketName,
                Prefix = userId.ToString() + delimiter,
                Delimiter = delimiter,
            };
            var response = await _s3Client.ListObjectsAsync(request);
            var result = new List<GameDataInfo>();
            foreach(var prefix in response.CommonPrefixes)
            {
                var parts = prefix.Split(delimiter);
                var dataInfo = await GetDataInfo(userId, parts[1]);
                result.Add(dataInfo);
            }
            return result;
        }
        
        public string GetDataFetchUrl(Guid userId, string gameId, uint index)
        {
            var dataKeyString = MakeKeyString(new GameDataKey { UserId = userId, GameId = gameId, Index = index});
            var preSignedUrlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = _bucketName,
                Key = dataKeyString,
                Expires = DateTime.Now.AddMinutes(15)
            };
            return _s3Client.GetPreSignedURL(preSignedUrlRequest);
        }

        public async Task<string> GetNextDataCreateUrl(Guid userId, string gameId)
        {
            var dataInfo = await GetDataInfo(userId, gameId);
            var nextIndex = dataInfo?.CurrentIndex + 1 ?? 0;
            var dataKeyString = MakeKeyString(new GameDataKey { UserId = userId, GameId = gameId, Index = nextIndex});
            var preSignedUrlRequest = new GetPreSignedUrlRequest()
            {
                BucketName = _bucketName,
                Key = dataKeyString,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(5)
            };
            return _s3Client.GetPreSignedURL(preSignedUrlRequest);
        }
    };
}
