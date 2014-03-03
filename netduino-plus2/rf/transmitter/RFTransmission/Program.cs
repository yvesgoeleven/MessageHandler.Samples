using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RFTransmission
{
    public class Program
    {
        private static bool _state = false;

        public static void Main()
        {
            var port = new OutputPort(Pins.GPIO_PIN_D1, false);
           
            var transmitter = new DIO.RfTransmitter(port, 123, 260, 3);

            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            button.OnInterrupt += (data1, data2, time) =>
            {
                if (data2 == 1) // only react when pressed, not when released
                {
                    _state = !_state;
                    transmitter.Send(2, _state);
                }
            };
            
            Thread.Sleep(Timeout.Infinite);
        }

    }
}
