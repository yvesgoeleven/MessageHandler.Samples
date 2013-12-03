using System;
using System.Linq;
using System.Reactive.Linq;
using MessageHandler;
using AverageInPeriod.Messages;

namespace AverageInPeriod
{
    public class AverageInPeriodHandler:
        IStandingQuery<Measurement>,
        IAction<Measurement>
    {
        private readonly IChannel _channel;
        private readonly AverageInPeriodHandlerConfig _config;

        public AverageInPeriodHandler(IConfigurationSource configurationSource, IChannel channel)
        {
            _channel = channel;

            _config = configurationSource.GetConfiguration<AverageInPeriodHandlerConfig>() ?? new AverageInPeriodHandlerConfig{ DurationInSeconds = 1, MeasurementType = "Temperature"};
        }

        public IObservable<Measurement> Compose(IObservable<Measurement> measurements)
        {
            return from e in measurements
                    where e.MeasurementType == _config.MeasurementType 
                          && (string.IsNullOrEmpty(e.AggregationMode) || e.AggregationMode == "None")
                    group e by "nothing" into c
                   from v in c.Buffer(TimeSpan.FromSeconds(_config.DurationInSeconds), TimeSpan.FromSeconds(_config.DurationInSeconds))
                    where v.Count > 0
                   select new Measurement { MeasurementType = v.First().MeasurementType, AggregationMode = "Average", Amount = v.Average(t => t.Amount), Unit = v.First().Unit };
        }

        public void Action(Measurement msg)
        {
            _channel.Push(msg);
        }
    }
}