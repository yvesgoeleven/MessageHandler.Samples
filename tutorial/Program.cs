using System;
using System.Net;
using System.Text;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

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

        const string ClientId = "XXXXXXXXXX"; // copy this from the connection details page
        const string ClientSecret = "XXXXXXXXXXXXXXXXXXXXXXXXXXXX"; // copy this from the connection details page
        const string Scope = "http://api.messagehandler.net/";
        const string BaseUri = "http://api.messagehandler.net/";

        // the channel we deployed and the environment where we deployed to
        private const string Channel = "Tutorial";
        private const string Environment = "Free";

        /// <summary>
        /// The main program logic
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var header = GetAuthorizationHeader(BaseUri, ClientId, ClientSecret, Scope);

            ListenForAlerts(header);

            SendMeasurements();

            Console.ReadLine();
        }

        /// <summary>
        /// Authenticates us against windows azure ACS and authorizes against the messagehandler gateway
        /// </summary>
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

        /// <summary>
        /// Subscribes to the signalr endpoint on the messagehandler gateway 
        /// and listens for alerts emitted by the signalr handler in our channel
        /// </summary>
        private static void ListenForAlerts(string header)
        {
            Console.WriteLine("Start listening for alerts");

            // Signalr provides us a bi-directional communication option with our channels, 
            // this is the address where we can both send messages to as well as listen for
            // messages coming from signalr handlers in our channel
            string address, hub;
            
            GetSignalrEndpoint(header, out address, out hub);

            // prepare a connection to the endpoint
            var hubConnection = new HubConnection(address);

            // make sure we're allowed in by adding the authorization header
            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), header);

            // setup a proxy on our end
            _channelHubProxy = hubConnection.CreateHubProxy(hub);
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

        private static void GetSignalrEndpoint(string header, out string address, out string hub)
        {
            var client = new RestClient(BaseUri);

            var request = new RestRequest("endpoints", Method.POST);
            request.AddHeader(HttpRequestHeader.Authorization.ToString(), header);

            request.AddParameter("protocol", "signalr");
            request.AddParameter("channel", Channel);
            request.AddParameter("environment", Environment);

            var response = client.Execute(request);

            var endpoint = JsonConvert.DeserializeObject<dynamic>(response.Content);

            address = endpoint.address;
            hub = endpoint.hub;
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
            for (var i = 0; i < 100; i++)
                _channelHubProxy.Invoke("Publish", Channel, Environment, new Measurement
                {
                    Type = "TemperatureMeasurement",
                    Temperature = new Random().Next(0, 100)
                }).Wait();
        }


        private static IHubProxy _channelHubProxy;
    }
}
