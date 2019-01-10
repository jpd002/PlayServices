using System;
using Amazon.DynamoDBv2.DataModel;

namespace PlayServices.DataModel
{
    public class User
    {
        public class GuestAuthMethod
        {
            public string Token { get; set; } = string.Empty;
        };

        public class PatreonAuthMethod
        {
            public int UserId { get; set; }
        };

        [DynamoDBHashKey("id")]
        public Guid Id { get; set; } = Guid.Empty;

        public GuestAuthMethod GuestAuth { get; set; }
        public PatreonAuthMethod PatreonAuth { get; set; }
    };
}
