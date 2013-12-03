using System;
using System.IO.Ports;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsAzure.Acs.Oauth2.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Gateway
{
    class Program
    {
        public static SerialPort Port = null;

        static void Main(string[] args)
        {
            SubscribeWithAMQP();

            Console.WriteLine("Connected to amqp");

            var client = new SimpleOAuth2Client(
                new Uri(ConfigurationSettings.AuthorizationServer),
                new Uri(ConfigurationSettings.AuthenticationServer),
                ConfigurationSettings.ClientId,
                ConfigurationSettings.ClientSecret,
                ConfigurationSettings.Scope,
                new Uri(ConfigurationSettings.RedirectUri), 
                ClientMode.TwoLegged);

            client.Authorize();

            ConnectToSignalr(client);
            
            Console.WriteLine("Connected to the signalr endpoint");
            
            try
            {
                Port = new SerialPort("COM8", 9600, Parity.None, 8, StopBits.One);

                if (Port.IsOpen) Port.Close();
                Port.Open();

                Port.DataReceived += PortOnDataReceived;
                Port.ErrorReceived += PortOnErrorReceived;

                Console.WriteLine("Connected to serial port");
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect to serial port");
            }

            var x = string.Empty;

            while (x != "x")
            {
                Console.WriteLine("Hit x to cancel, any other key to turn the sensors on/off");
                x = Console.ReadLine();

                if (x != "x" && Port != null)
                {
                    Toggle(Port);
                }
            }
        }

        private static void Toggle(SerialPort port)
        {
            var buffer = Encoding.UTF8.GetBytes("onoff");
            port.Write(buffer, 0, buffer.Length);
        }

        private static void Toggle(ToggleCommand msg)
        {
            if (msg.Gateway == ConfigurationSettings.Queue && msg.Device == "netduino" && Port != null)
                Toggle(Port);
        }

        private static void PortOnErrorReceived(object sender, SerialErrorReceivedEventArgs serialErrorReceivedEventArgs)
        {
            Console.WriteLine("Error received on serial port");
        }

        private static void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var buffer = new byte[8];
            Port.Read(buffer, 0, buffer.Length);

            var i = BitConverter.ToDouble(buffer, 0);

            if (i > 0 && i < 0.0000000000000000001) return; // remove jitter

            Console.WriteLine("Data received: " + i.ToString("##.##"));

            Task.Run(() => PublishWithSignalr(new Measurement
            {
                Amount = i,
                Unit = "Celcius",
                MeasurementType = "Temperature",
                AggregationMode = "None"
            }));
        }

        private static void SubscribeWithAMQP()
        {
            var factory = MessagingFactory.CreateFromConnectionString(ConfigurationSettings.ServiceBusConnectionstring + ";TransportType=Amqp");
            var receiver = factory.CreateMessageReceiver(ConfigurationSettings.Queue);
            var messageListener = new MessageListener(receiver);
            messageListener.On += (sender, command) => Toggle(command);
            var listenerThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        BrokeredMessage message = receiver.Receive(new TimeSpan(0, 0, 10));
                        if (message != null)
                        {
                            var body = message.GetBody<string>();
                            Toggle(JsonConvert.DeserializeObject<ToggleCommand>(body));

                            message.Complete();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Caught exception: " + e.Message);
                        break;
                    }
                }
            });
            listenerThread.Start();
        }

        private static void ConnectToSignalr(SimpleOAuth2Client client)
        {
            var hubConnection = new HubConnection(ConfigurationSettings.SignalrEndpoint);

            hubConnection.Headers.Add(HttpRequestHeader.Authorization.ToString(), client.GetAccessToken());

            _channelHubProxy = hubConnection.CreateHubProxy("ChannelHub");

           // uncomment to listen with signalr (instead of amqp)
           // _channelHubProxy.On<ToggleCommand>("ReceiveMessage", Toggle);

           var t = hubConnection.Start(new LongPollingTransport());

           t.Wait();

           // uncomment to listen with signalr (instead of amqp)
           // _channelHubProxy.Invoke("Subscribe", ConfigurationSettings.RoutingChannel, ConfigurationSettings.Environment).Wait();
        }

        private static void PublishWithSignalr(dynamic msg)
        {
            if (msg.Amount > 1)
                _channelHubProxy.Invoke("Publish", ConfigurationSettings.TemperaturesChannel, ConfigurationSettings.Environment, msg); 
        }

       

        private static IHubProxy _channelHubProxy;
    }
}
