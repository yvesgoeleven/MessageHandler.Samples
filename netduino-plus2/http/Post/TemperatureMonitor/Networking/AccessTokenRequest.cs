using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.SPOT;

namespace TemperatureMonitor
{
public class AccessTokenRequest
{
    public void Execute(out string accessToken, out string refreshToken, out int expiresin)
    {
        using (var request = (HttpWebRequest)WebRequest.Create(Settings.RequestUri + "authorize"))
        {
            const string body = "client_id=" + Settings.ClientId
                                + "&client_secret=" + Settings.ClientSecret
                                + "&scope=" + Settings.Scope
                                + "&grant_type=client_credentials";

            var postData = new UTF8Encoding().GetBytes(body);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(postData, 0, postData.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = response.GetResponseStream();

                    var hashtable = new JsonParser().Parse(stream);

                    accessToken = hashtable["access_token"].ToString();
                    refreshToken = hashtable["refresh_token"].ToString();
                    expiresin = int.Parse(hashtable["expires_in"].ToString());
                }
                else
                {
                    accessToken = null;
                    expiresin = 0;
                    refreshToken = null;

                    Debug.Print(DateTime.Now + " Response code: " + response.StatusCode.ToString());

                    var stream = response.GetResponseStream();
                    var reader = new StreamReader(stream);
                    Debug.Print(reader.ReadToEnd());
                }
            }
        }
    }

        public void Execute(string refreshToken, out string accessToken, out int expiresin)
        {
            using (var request = (HttpWebRequest)WebRequest.Create(Settings.RequestUri + "authorize"))
            {
                var body = "client_id=" + Settings.ClientId
                        + "&client_secret=" + Settings.ClientSecret
                        + "&scope=" + Settings.Scope
                        + "&grant_type=refresh_token"
                        + "&refresh_token=" + refreshToken;


                var postData = new UTF8Encoding().GetBytes(body);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var stream = response.GetResponseStream();

                        var hashtable = new JsonParser().Parse(stream);

                        accessToken = hashtable["access_token"].ToString();
                        expiresin = int.Parse(hashtable["expires_in"].ToString());
                    }
                    else
                    {
                        accessToken = null;
                        expiresin = 0;

                        Debug.Print(DateTime.Now + " Response code: " + response.StatusCode.ToString());

                        var stream = response.GetResponseStream();
                        var reader = new StreamReader(stream);
                        Debug.Print(reader.ReadToEnd());
                    }
                }
            }
        }

        
    }

}
