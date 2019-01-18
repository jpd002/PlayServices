using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlayServices
{
    public class PatreonApi
    {
        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; } = string.Empty;

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("scope")]
            public string Scope { get; set; } = string.Empty;

            [JsonProperty("token_type")]
            public string TokenType { get; set; } = string.Empty;
        };

        public class User
        {
            [JsonProperty("id")]
            public int Id { get; set; }
        };

        public class IdentityResponse
        {
            [JsonProperty("data")]
            public User Data { get; set; } = new User();
        };

        public string AccessToken { get; set; } = string.Empty;

        public async Task<TokenResponse> GetTokenAuthorizationCode(string code, string clientId, string clientSecret, string redirectUri)
        {
            var contentDict = new Dictionary<string, string>();
            contentDict.Add("code", code);
            contentDict.Add("grant_type", "authorization_code");
            contentDict.Add("client_id", clientId);
            contentDict.Add("client_secret", clientSecret);
            contentDict.Add("redirect_uri", redirectUri);
            var content = new FormUrlEncodedContent(contentDict);
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, "https://www.patreon.com/api/oauth2/token") { Content = content };
            var res = await client.SendAsync(req);
            if(!res.IsSuccessStatusCode)
            {
                throw new System.Exception("Failed to get token info.");
            }
            var resultJson = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(resultJson);
        }

        public async Task<IdentityResponse> GetIdentity()
        {
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Get, "https://www.patreon.com/api/oauth2/v2/identity?include=memberships");
            req.Headers.Add("Authorization", string.Format("Bearer {0}", AccessToken));
            var res = await client.SendAsync(req);
            if(!res.IsSuccessStatusCode)
            {
                throw new System.Exception("Failed to get identity info.");
            }
            var resultJson = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IdentityResponse>(resultJson);
        }
    };
}
