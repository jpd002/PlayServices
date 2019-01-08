using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices
{
    public class User
    {
        public const string AUTHTYPE_GUEST = "guest";
        public const string AUTHTYPE_PATREON = "patreon";

        [DynamoDBHashKey("id")]
        public Guid Id { get; set; } = Guid.Empty;

        public Dictionary<string, string> AuthMethods { get; set; } = new Dictionary<string, string>();
    };
}
