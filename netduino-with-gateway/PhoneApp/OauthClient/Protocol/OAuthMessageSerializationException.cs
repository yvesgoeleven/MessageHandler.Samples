using System;

namespace WindowsAzure.Acs.Oauth2.Client.WinRT.Protocol
{
    public class OAuthMessageSerializationException
        : Exception
    {
        public OAuthMessageSerializationException()
            : base("")
        {
        }
        public OAuthMessageSerializationException(string message)
            : base(message)
        {
        }
        public OAuthMessageSerializationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}