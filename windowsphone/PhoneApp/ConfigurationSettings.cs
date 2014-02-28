namespace PhoneApp1
{
    // Assumptions you set up the following before trying this example:
    // You have a valid message handler
    // You have setup a channel containing a routing handler to an servicesbus queue based on a property called Gateway

    public class ConfigurationSettings
    {
        public const string BaseUri = "http://api.messagehandler.net";
        public const string ClientId = "";
        public const string ClientSecret = "";
        public const string Scope = "http://api.messagehandler.net/";

        public const string SignalrEndpoint = "http://api.messagehandler.net/signalr/";

        public const string Channel = "";
        public const string Environment = "";

    }
}