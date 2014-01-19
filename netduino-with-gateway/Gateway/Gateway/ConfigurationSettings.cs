namespace Gateway
{
    public class ConfigurationSettings
    {
        // authorization settings
        public const string ClientId = "";
        public const string ClientSecret = "";
        public const string Scope = "";
        public const string BaseUri = "";

        // servicebus (amqp)
        public const string ServiceBusConnectionstring = "";
        public const string Queue = "";

        // signalr
        public const string SignalrEndpoint = "";

        // messagehandler channels
        public const string TemperaturesChannel = "";
        public const string RoutingChannel = "";
        public const string Environment = "";
    }
}