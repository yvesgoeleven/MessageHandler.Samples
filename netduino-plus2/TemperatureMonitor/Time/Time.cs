using Microsoft.SPOT;

namespace TemperatureMonitor
{
    public static class Time
    {
        public static bool Set()
        {
            return Ntp.UpdateTimeFromNtpServer("pool.ntp.org", +1);  // Central European Time
        }
    }
}