namespace PhoneApp1
{
    // Assumptions you set up the following before trying this example:
    // You have a valid message handler
    // You have setup a channel containing a routing handler to an servicesbus queue based on a property called Gateway

    public class ConfigurationSettings
    {
        public const string BaseUri = "http://api.messagehandler.net";
        public const string ClientId = "XXXXXXX";
        public const string ClientSecret = "XXXXXXXXXXXXXXXXX";
        public const string Scope = "http://api.messagehandler.net/";

        public const string Channel = "XXXXXXXXXX";
        public const string Environment = "XXXXXXXXXXx";

    }
}