using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowsAzure.Acs.Oauth2.Client.WinRT;
using Microsoft.Phone.Controls;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Send(App.OauthClient, new ToggleCommand { Gateway= ConfigurationSettings.QueueName , Device = "netduino"});
        }

        private async Task Send(SimpleOAuth2Client client, ToggleCommand message)
        {
            const string apiRoot =
                ConfigurationSettings.ApiRoot + "/" + 
                ConfigurationSettings.RoutingChannel + "/" +
                ConfigurationSettings.Environment + "/";
                
            var req = WebRequest.CreateHttp(apiRoot);
            await client.AppendAccessTokenToAsync(req);

            req.Method = "post";
            req.ContentType = "application/json";

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            req.ContentLength = bytes.Length;

            req.BeginGetRequestStream(ar =>
            {
                var request = (HttpWebRequest) ar.AsyncState;
                var s = request.EndGetRequestStream(ar);
                s.Write(bytes, 0, bytes.Length);
                s.Close();

                request.BeginGetResponse(result =>
                {
                    var r = (HttpWebRequest)result.AsyncState;
                    var response = (HttpWebResponse)r.EndGetResponse(result);
                    var streamResponse = response.GetResponseStream();
                    var streamRead = new StreamReader(streamResponse);
                    var responseString = streamRead.ReadToEnd();

                    Console.WriteLine(responseString);
                    
                }, request);
            },req);
        }
    }
}