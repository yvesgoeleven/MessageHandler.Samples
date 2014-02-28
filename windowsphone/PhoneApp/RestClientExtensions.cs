using System.Threading.Tasks;
using RestSharp;

namespace PhoneApp1
{
    public static class RestClientExtensions
    {
        public static Task<IRestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            var tcs = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, tcs.SetResult);
            return tcs.Task;
        }
    }
}