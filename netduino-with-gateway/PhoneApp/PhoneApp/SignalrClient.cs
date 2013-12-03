using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WindowsAzure.Acs.Oauth2.Client.WinRT;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace PhoneApp1
{
    public class SignalrClient
    {
        public async Task ListenForTemperatureChanges(SimpleOAuth2Client client)
        {
            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint, 
                                   new Dictionary<string, string>
                                   {
                                       { HttpRequestHeader.Authorization.ToString(), await client.GetAccessToken() }
                                   });

            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");

            _channelHubProxy.Subscribe("ReceiveMessage").Received +=
               list => App.RootFrame.Dispatcher.BeginInvoke(() => UpdateTemperatureView(list[0]));

            await hubConnection.Start(new LongPollingTransport());

            await _channelHubProxy.Invoke("Subscribe", ConfigurationSettings.TemperaturesChannel, ConfigurationSettings.Environment);
        }

        private static void UpdateTemperatureView(dynamic msg)
        {
            ViewModel.Temperature.Current = msg.Amount;
        }

        public async Task Send(ToggleCommand toggleCommand)
        {
            await _channelHubProxy.Invoke("Publish", ConfigurationSettings.RoutingChannel, ConfigurationSettings.Environment, toggleCommand);
        }

    
        private static IHubProxy _channelHubProxy;


    }
}