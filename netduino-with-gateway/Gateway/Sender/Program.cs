using System;
using System.Net;
using System.Text;
using Gateway;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json;
using RestSharp;

namespace Sender
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

            string x = null;
            while (x != "x")
            {
                PublishWithSignalr(new
                {
                    Amount = 10.0,
                    Unit = "Celcius",
                    MeasurementType = "Temperature",
                    AggregationMode = "None"
                });
                x = Console.ReadLine();
            }
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
            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint);

            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), header);

            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");

            var t = hubConnection.Start(new LongPollingTransport());

            t.Wait();
        }


        private static void PublishWithSignalr(dynamic msg)
        {
            Console.WriteLine("Sending");

            _channelHubProxy.Invoke("Publish", ConfigurationSettings.TemperaturesChannel, ConfigurationSettings.Environment, msg); //.Wait();
        }

        private static IHubProxy _channelHubProxy;
    }
}
