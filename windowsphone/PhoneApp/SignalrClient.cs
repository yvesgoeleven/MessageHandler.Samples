using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace PhoneApp1
{
    public class SignalrClient
    {
        public async Task ListenForTemperatureChanges()
        {
            var header = await _authorization.GetAuthorizationHeader();

            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint, 
                                   new Dictionary<string, string>
                                   {
                                       { HttpRequestHeader.Authorization.ToString(), header }
                                   });

            hubConnection.EnsureReconnecting();

            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");

            _channelHubProxy.Subscribe("ReceiveMessage").Received +=
               list => App.RootFrame.Dispatcher.BeginInvoke(() => UpdateTemperatureView(list[0]));

            await hubConnection.Start(new LongPollingTransport());

            await _channelHubProxy.Invoke("Subscribe", ConfigurationSettings.Channel, ConfigurationSettings.Environment);
        }

        private static void UpdateTemperatureView(dynamic msg)
        {
            ViewModel.Temperature.Current = msg["Amount"].Value;
        }
    
        private static IHubProxy _channelHubProxy;
        private readonly Authorization _authorization = new Authorization();
    }
}