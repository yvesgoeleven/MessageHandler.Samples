using System;
using System.Collections;
using System.IO;
using System.Text;

namespace TemperatureMonitor
{
public class JsonParser
{
    public Hashtable Parse(Stream stream)
    {
        var hashtable = new Hashtable();
        using (var reader = new StreamReader(stream))
        {
            var text = reader.ReadToEnd();
            text = text.TrimStart('{');
            text = text.TrimEnd('}');
            var namevaluepairs = text.Split(',');
            foreach (var pair in namevaluepairs)
            {
                var i = pair.IndexOf(':');
                hashtable[pair.Substring(0, i).Trim('"')] = Replace(pair.Substring(i+1).Trim('"'), "\\u0026", "&");
            }
        }
        return hashtable;
    }

    public string Replace(string s, string x, string y)
    {
        var sb = new StringBuilder(s);
        sb.Replace(x, y);
        return sb.ToString();
    }
}
}
