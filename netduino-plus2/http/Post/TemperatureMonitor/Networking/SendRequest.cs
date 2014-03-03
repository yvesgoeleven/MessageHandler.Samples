using System;
using System.IO;
using Microsoft.SPOT;
using System.Net;
using System.Text;

namespace TemperatureMonitor
{
    public class SendRequest
    {
        public void Execute(string body, string bearerToken, string host, string path)
        {
            using (var request = (HttpWebRequest)WebRequest.Create(host + path))
            {
                request.Headers.Add("Authorization", bearerToken);

                var postData = new UTF8Encoding().GetBytes(body);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = postData.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postData, 0, postData.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var stream = response.GetResponseStream();
                    Debug.Print(DateTime.Now + " Response code: " + response.StatusCode.ToString());

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var reader = new StreamReader(stream);
                        Debug.Print("Result: " + reader.ReadToEnd());
                    }
                }
            }
        }
    }
}