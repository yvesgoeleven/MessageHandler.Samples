using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace PhoneApp1
{
    public class EndpointRequest
    {
        public async Task<dynamic> GetEndpoint(string bearerToken, string protocol)
        {
            var client = new RestClient(ConfigurationSettings.BaseUri);

            var request = new RestRequest("endpoints", Method.POST);
            request.AddHeader(HttpRequestHeader.Authorization.ToString(), bearerToken);
            request.AddParameter("protocol", protocol);
            request.AddParameter("channel", ConfigurationSettings.Channel);
            request.AddParameter("environment", ConfigurationSettings.Environment);

            var response = await client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var parsed = JObject.Parse(response.Content);
                return parsed["address"].Value<string>();
            }

            return null;
        }
    }
}