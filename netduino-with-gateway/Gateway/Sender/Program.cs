using System;
using System.Net;
using WindowsAzure.Acs.Oauth2.Client;
using Gateway;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SimpleOAuth2Client(
                new Uri(ConfigurationSettings.AuthorizationServer),
                new Uri(ConfigurationSettings.AuthenticationServer),
                ConfigurationSettings.ClientId,
                ConfigurationSettings.ClientSecret,
                ConfigurationSettings.Scope,
                new Uri(ConfigurationSettings.RedirectUri), ClientMode.TwoLegged);

            client.Authorize();

            SubscribeWithSignalr(client);

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


        private static void SubscribeWithSignalr(SimpleOAuth2Client client)
        {
            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint);

            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), client.GetAccessToken());

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
