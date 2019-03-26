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

        public const string g_env_ps_patreon_client_id = "ps_patreon_client_id";
        public const string g_env_ps_patreon_client_secret = "ps_patreon_client_secret";
        public const string g_env_ps_patreon_redirect_uri = "ps_patreon_redirect_uri";

        public const string g_env_accessTokenKey = "ps_accessTokenKey";
    };
}
