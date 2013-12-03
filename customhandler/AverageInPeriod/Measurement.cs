using System;
using MessageHandler.Contracts;

namespace AverageInPeriod.Messages
{
    public class Measurement : IEvent
    {
        public double Amount { get; set; }
        public string Unit { get; set; }
        public string MeasurementType { get; set; }
        public string AggregationMode { get; set; }

        public string What { get; set; }
        public string Who { get; set; }
        public DateTime When { get; set; }
        public Location Where { get; set; }
    }
}