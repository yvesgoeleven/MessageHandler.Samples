using System;
using Microsoft.SPOT;
using TemperatureMonitor.MessageHandler;
using TemperatureMonitor.Temperature;

namespace TemperatureMonitor
{
public class Program
{
    public static void Main()
    {
        Time.Set();

        var client = new MessageHandlerClient();
        client.Start();
        
        var reader = new TemperatureReader();
        reader.OnRead += (sender, temperature) =>
        {
            //Debug.Print(temperature.ToString());

            var now = ToIso8601(DateTime.UtcNow);
            client.Send("{"+
                        " Amount:" + temperature.ToString() + ", " +
                        " Unit: \"Celcius\","+
                        " MeasurementType:\"Temperature\","+
                        " When:\"" + now + "\""+
                        "}");

        };
        reader.Start();
    }

    public static string ToIso8601(DateTime dateTime)
    {
        string result = dateTime.Year.ToString() + "-" +
                        TwoDigits(dateTime.Month) + "-" +
                        TwoDigits(dateTime.Day) + "T" +
                        TwoDigits(dateTime.Hour) + ":" +
                        TwoDigits(dateTime.Minute) + ":" +
                        TwoDigits(dateTime.Second) + "." +
                        ThreeDigits(dateTime.Millisecond) + "Z";

        return result;
    }

    private static string TwoDigits(int value)
    {
        if (value < 10)
            return "0" + value.ToString();

        return value.ToString();
    }

    private static string ThreeDigits(int value)
    {
        if (value < 10)
            return "00" + value.ToString();

        if (value < 100)
            return "0" + value.ToString();

        return value.ToString();
    }

}
}
