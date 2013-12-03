using System;
using System.Net;
using WindowsAzure.Acs.Oauth2.Client;
using Gateway;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Receiver
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

            Console.WriteLine("Subscribed to the signalr endpoint");

            Console.ReadLine();
        }

        private static void SubscribeWithSignalr(SimpleOAuth2Client client)
        {
            Console.WriteLine("Subscribing...");

            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint);

            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), client.GetAccessToken());

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
