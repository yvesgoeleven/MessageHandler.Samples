using System;
using System.Net;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using WindowsAzure.Acs.Oauth2.Client;
using Newtonsoft.Json.Linq;

namespace SampleApplication
{
    /// <summary>
    /// Just complete the login information and hit F5 to run this sample program
    /// </summary>
    class Program
    {
        // this information is required to get access to your channels
        // copy the missing values from the connection details page
        // and you're good to go!

        const string ClientId = "XXXXXXXXXXXX"; // copy this from the connection details page
        const string ClientSecret = "XXXXXXXXXXXX"; // copy this from the connection details page
        const string RedirectUri = "http://" + ClientId;
        const string Scope = "http://api.messagehandler.net/";
        const string AuthenticationServer = "https://messagehandler-acs-eu-west-prod.accesscontrol.windows.net/v2/OAuth2-13/";
        const string AuthorizationServer = "http://messagehandler-alpha.cloudapp.net:8080/authorize";

        // Signalr provides us a bi-directional communication option with our channels, 
        // this is the address where we can both send messages to as well as listen for
        // messages coming from signalr handlers in our channel

        private const string SignalrEndpoint = "http://messagehandler-alpha.cloudapp.net:8080/signalr/";

        // the channel we deployed and the environment where we deployed to

        private const string Channel = "Tutorial";
        private const string Environment = "Development";

        /// <summary>
        /// The main program logic
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Authorize();

            ListenForAlerts();

            SendMeasurements();

            Console.ReadLine();
        }

        /// <summary>
        /// Authenticates us against windows azure ACS and authorizes against the messagehandler gateway
        /// </summary>
        private static void Authorize()
        {
            _client = new SimpleOAuth2Client(new Uri(AuthorizationServer), 
                new Uri(AuthenticationServer), 
                ClientId, 
                ClientSecret, 
                Scope, 
                new Uri(RedirectUri), 
                ClientMode.TwoLegged);

            _client.Authorize();
        }

        /// <summary>
        /// Subscribes to the signalr endpoint on the messagehandler gateway 
        /// and listens for alerts emitted by the signalr handler in our channel
        /// </summary>
        private static void ListenForAlerts()
        {
            Console.WriteLine("Start listening for alerts");

            // prepare a connection to the endpoint
            var hubConnection = new HubConnection(SignalrEndpoint);

            // make sure we're allowed in by adding the authorization header
            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), _client.GetAccessToken());

            // setup a proxy on our end
            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");
            // and start listening for messages coming from our channel
            _channelHubProxy.On("ReceiveMessage", alert =>
            {
                dynamic msg = JObject.Parse(alert.Message.Value);
                Console.WriteLine("Received alert: {0}, measured temperature {1}", alert.Trigger, msg.Temperature);
            });

            // open the connection
            hubConnection.Start(new LongPollingTransport()).Wait();

            // tell the gateway which of our channels and environment we're interested in
            _channelHubProxy.Invoke("Subscribe", Channel, Environment).Wait();
        }

        /// <summary>
        /// Sends measurements to our channel
        /// </summary>
        private static void SendMeasurements()
        {
            Console.WriteLine("Sending measurements");

            // just send a bunch of random temperatures to our channel
            // those over 50 degrees will result in an alert coming in via the signalr endpoint
            // those over 90 degrees will result in an email sent to your smtp server
            for(var i = 0; i< 100; i++)
                _channelHubProxy.Invoke("Publish", Channel, Environment, new Measurement
                {
                    Type = "TemperatureMeasurement",
                    Temperature = new Random().Next(0, 100)
                }).Wait();
        }

        
        private static IHubProxy _channelHubProxy;
        private static SimpleOAuth2Client _client;
    }
}
