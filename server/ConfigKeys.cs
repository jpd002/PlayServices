using System;

namespace PlayServices
{
    static class ConfigKeys
    {
        public const string g_env_ps_gh_apitoken = "ps_gh_apitoken";

        public const string g_env_ps_builds_aws_access_key = "ps_builds_aws_access_key";
        public const string g_env_ps_builds_aws_access_secret = "ps_builds_aws_access_secret";
        public const string g_env_ps_builds_dynamodb_table_name = "ps_builds_dynamodb_table_name";
        public const string g_env_ps_users_dynamodb_table_name = "ps_users_dynamodb_table_name";
    };
}
