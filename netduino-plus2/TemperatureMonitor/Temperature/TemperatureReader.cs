using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using ThreelnDotOrg.NETMF.Hardware;

namespace TemperatureMonitor.Temperature
{
    public class TemperatureReader
    {
        private DS18B20 _sensor { get; set; }

        public delegate void TemperatureEvent(object sender, double temperature);
        public event TemperatureEvent OnRead;
        protected virtual void OnOnRead(double temperature)
        {
            var handler = OnRead;
            if (handler != null) handler(this, temperature);
        }

        public TemperatureReader()
        {
            var pin = new OutputPort(Pins.GPIO_PIN_D0, false);
            var bus = new OneWire(pin);
            var devs = OneWireBus.Scan(bus, OneWireBus.Family.DS18B20);
            _sensor = new DS18B20(bus, devs[0]);
        }

        public void Start()
        {
            var threadWorker = new Thread(StartReading);
            threadWorker.Start();
        }

        private void StartReading()
        {
            while (true)
            {
               OnOnRead(_sensor.ConvertAndReadTemperature());
               Thread.Sleep(100);
            }
        }
    }
}
