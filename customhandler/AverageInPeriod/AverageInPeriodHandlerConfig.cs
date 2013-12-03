using System.Configuration;

namespace AverageInPeriod
{
    public class AverageInPeriodHandlerConfig : ConfigurationSection
    {
        public double DurationInSeconds { get; set; }
        public string MeasurementType { get; set; }
    }
}