using System;
using System.Net;
using System.Text;
using Gateway;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json;
using RestSharp;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var header = GetAuthorizationHeader(
                ConfigurationSettings.BaseUri,
                ConfigurationSettings.ClientId,
                ConfigurationSettings.ClientSecret,
                ConfigurationSettings.Scope
                );

            SubscribeWithSignalr(header);

            Console.WriteLine("Subscribed to the signalr endpoint");

            Console.ReadLine();
        }

        private static string GetAuthorizationHeader(string baseuri, string clientId, string clientSecret, string scope)
        {
            var client = new RestClient(baseuri);

            var request = new RestRequest("authorize", Method.POST);
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("scope", scope);
            request.AddParameter("grant_type", "client_credentials");

            var response = client.Execute(request);

            string token = JsonConvert.DeserializeObject<dynamic>(response.Content).access_token;

            return "Bearer " + Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

        private static void SubscribeWithSignalr(string header)
        {
            Console.WriteLine("Subscribing...");

            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint);

            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), header);

            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");

            _channelHubProxy.On("ReceiveMessage", msg => WriteLine(msg));

            var t = hubConnection.Start(new LongPollingTransport());

            t.Wait();

            _channelHubProxy.Invoke("Subscribe", ConfigurationSettings.TemperaturesChannel, ConfigurationSettings.Environment).Wait();
        }

        private static void WriteLine(dynamic msg)
        {
            Console.WriteLine("Data received: {0}, {1}", msg.Amount.ToString("##.###"), msg.AggregationMode);
        }

    

        private static IHubProxy _channelHubProxy;
    }
}
