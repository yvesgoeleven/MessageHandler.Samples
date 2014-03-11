using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace PhoneApp1
{
    public class SignalrClient
    {
        public async Task ListenForTemperatureChanges()
        {
            var header = await _authorization.GetAuthorizationHeader();
            var endpoint = await _endpointRequest.GetEndpoint(header, "signalr");

            var hubConnection = new HubConnection(endpoint, 
                                   new Dictionary<string, string>
                                   {
                                       { HttpRequestHeader.Authorization.ToString(), header }
                                   });

            hubConnection.EnsureReconnecting();
            hubConnection.Reconnecting += async () => 
            {
                header = await _authorization.GetAuthorizationHeader();
            };

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
        private readonly EndpointRequest _endpointRequest = new EndpointRequest();
    }
}