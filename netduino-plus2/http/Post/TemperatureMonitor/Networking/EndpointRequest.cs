using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.SPOT;

namespace TemperatureMonitor
{
public class EndpointRequest
{
    public void Execute(string bearerToken, out string host, out string path)
    {
        using (var request = (HttpWebRequest)WebRequest.Create(Settings.RequestUri + "endpoints"))
        {
            request.Headers.Add("Authorization", bearerToken);

            const string body = "protocol=http&channel=demo-temperatures&environment=Development";

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

                    host = hashtable["host"].ToString();
                    path = hashtable["path"].ToString();
                }
                else
                {
                    host = path = string.Empty;

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
