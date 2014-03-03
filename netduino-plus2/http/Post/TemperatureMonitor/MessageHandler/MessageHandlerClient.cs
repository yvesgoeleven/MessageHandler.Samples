using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;

namespace TemperatureMonitor.MessageHandler
{
    public class MessageHandlerClient
    {
        public static bool NetworkIsAvailable = false;
        public static string AccessToken;
        public static string RefreshToken;
        public static string BearerToken;
        public static string Host;
        public static string Path;
        public static DateTime Expired;

        public void Start()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;

            CheckNetwork();

            if (NetworkIsAvailable)
            {
                if (BearerToken == null || Expired <= DateTime.Now.AddMinutes(1))
                {
                    TryAuthenticate();
                }

                if (Host == null && BearerToken != null)
                {
                    TryGetEndpoint();
                }
            }
        }

        public void Send(string message)
        {
            if (NetworkIsAvailable)
            {
                if (BearerToken == null || Expired <= DateTime.Now.AddMinutes(1))
                {
                    TryAuthenticate();
                }

                if (Host == null && BearerToken != null)
                {
                    TryGetEndpoint();
                }

                if (Host != null && BearerToken != null)
                {
                    TrySend(message);
                }
            }
        }

        private void TrySend(string message)
        {
            try
            {
                new SendRequest().Execute(message, BearerToken, Host, Path);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void TryGetEndpoint()
        {
            try
            {
                string host, path;
                new EndpointRequest().Execute(BearerToken, out host, out path);
                Host = host;
                Path = path;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            
        }

        private void TryAuthenticate()
        {
            try
            {
                string accessToken, refreshToken;
                int expiresIn;
                var request = new AccessTokenRequest();
                if (RefreshToken == null)
                {
                    request.Execute(out accessToken, out refreshToken, out expiresIn);
                    RefreshToken = refreshToken;
                }
                else
                {
                    request.Execute(RefreshToken, out accessToken, out expiresIn);
                }

                if (accessToken != null)
                {
                    AccessToken = accessToken;
                    BearerToken = "Bearer " + Base64.Encode(accessToken, false);
                    Expired = DateTime.Now.AddSeconds(expiresIn);
                }
                else
                {
                    AccessToken = null;
                    BearerToken = null;
                    RefreshToken = null;
                    Expired = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

       

        private void NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            NetworkIsAvailable = e.IsAvailable;
        }

        private void CheckNetwork()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces(); 
            foreach (NetworkInterface networkInterface in networkInterfaces) 
            { 
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet) 
                { 
                    NetworkIsAvailable = networkInterface.IPAddress != "0.0.0.0"; 
                } 
            } 
        }
    }
}
