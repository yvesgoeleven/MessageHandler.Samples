using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT.Hardware;
using NetduinoGo;
using SecretLabs.NETMF.Hardware.NetduinoGo;

namespace Netduino
{
    // netduino go configuration required to run this sample:
    // - RgbLed module on Socket2
    // - Schieldbase module on Socket 7
    // - TMP36 temperature sensor wired to A0 on shieldbase
    // - Uart To USB cable wired to D0/D1 on shieldbase and COM1 port

    public class Program
    {
        private static bool _on;
        private static SerialPort _serialPort;
        private static RgbLed _led;
        private static ShieldBase _shieldbase;

        public static void Main()
        {
            _led = new RgbLed();
            _led.SetColor(255, 165, 0); // orange, we're booting

            try
            {
                _shieldbase = new ShieldBase(GoSockets.Socket7);

                _serialPort = new SerialPort(_shieldbase.SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One);
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Open();
                _serialPort.DataReceived += DataReceivedHandler;

                var threadWorker = new Thread(DoWork);
                threadWorker.Start();


                Thread.Sleep(Timeout.Infinite);
            }
            catch
            {
                _led.SetColor(255, 0, 0); // red, we died
            }
        }

        private static void DoWork()
        {
            try
            {
                var onboardButton = new InterruptPort(Pins.Button, true, Port.ResistorMode.PullDown, Port.InterruptMode.InterruptEdgeHigh);

                onboardButton.OnInterrupt += (data1, data2, time) =>
                {
                    _on = !_on;
                    _led.SetBrightness(_on ? 100 : 0);
                    _led.SetColor(0, 255, 0); // on!
                };

                var a = new AnalogInput(_shieldbase.AnalogChannels.ANALOG_0);

                _led.SetColor(0, 0, 255); // good to go!

                while (true)
                {
                    if (_on)
                    {
                        var temperature = ReadTemperature(a);

                        SendValueToGateway(_serialPort, temperature);
                    }

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                _led.SetColor(255, 0, 0); // red, we died
            }
         
        }

        private static double ReadTemperature(AnalogInput a)
        {
            var sensorReading = a.Read();
            var sensorMilliVolts = (sensorReading*3.3);
            var temperatureC = (sensorMilliVolts - 0.5)*100.0;
            return temperatureC;
        }

        private static void SendValueToGateway(Stream serialPort, double value)
        {
            var buffer = BitConverter.GetBytes(value);
            serialPort.Write(buffer, 0, buffer.Length);
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            _on = !_on;
            _led.SetColor(0, 255, 0); // on
            _led.SetBrightness(_on ? 100 : 0);
        }

    }
}
