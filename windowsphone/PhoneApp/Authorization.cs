using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace PhoneApp1
{
    public class Authorization
    {
        private string access_token;
        private string refresh_token;
        private string bearer_token;
        private DateTime expired;

        public async Task<string> GetAuthorizationHeader()
        {
            if (bearer_token == null || expired <= DateTime.Now.AddMinutes(1))
            {
                access_token = await GetAccessToken();
                if (access_token != null)
                {
                    bearer_token = "Bearer " + Convert.ToBase64String(Encoding.UTF8.GetBytes((string) access_token));
                }
                else
                {
                    bearer_token = null;
                }
            }
            return bearer_token;
        }

        public async Task<string> GetAccessToken()
        {
            if (access_token == null)
            {
                await ExecuteAccessTokenRequest();
            }
            else if (expired <= DateTime.Now.AddMinutes(1))
            {
                await ExecuteRefreshTokenRequest();
            }
            return access_token;
        }

        private async Task ExecuteAccessTokenRequest()
        {
            var client = new RestClient(ConfigurationSettings.BaseUri);

            var request = new RestRequest("authorize", Method.POST);
            request.AddParameter("client_id", ConfigurationSettings.ClientId);
            request.AddParameter("client_secret", ConfigurationSettings.ClientSecret);
            request.AddParameter("scope", ConfigurationSettings.Scope);
            request.AddParameter("grant_type", "client_credentials");

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var parsed = JObject.Parse(response.Content);
                access_token = parsed["access_token"].Value<string>();
                refresh_token = parsed["refresh_token"].Value<string>();
                expired = DateTime.Now.AddSeconds(parsed["expires_in"].Value<int>());
            }
            else
            {
                access_token = null;
                refresh_token = null;
                expired = DateTime.MinValue;
            }
        }

        private async Task ExecuteRefreshTokenRequest()
        {
            var client = new RestClient(ConfigurationSettings.BaseUri);

            var request = new RestRequest("authorize", Method.POST);
            request.AddParameter("client_id", ConfigurationSettings.ClientId);
            request.AddParameter("client_secret", ConfigurationSettings.ClientSecret);
            request.AddParameter("scope", ConfigurationSettings.Scope);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", refresh_token);

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var parsed = JObject.Parse(response.Content);
                access_token = parsed["access_token"].Value<string>();
                expired = DateTime.Now.AddSeconds(parsed["expires_in"].Value<int>());
            }
            else
            {
                access_token = null;
                refresh_token = null;
                expired = DateTime.MinValue;
            }
        }
    }
}