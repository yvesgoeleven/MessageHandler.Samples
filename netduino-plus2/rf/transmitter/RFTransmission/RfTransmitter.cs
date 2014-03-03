using Microsoft.SPOT.Hardware;

namespace RFTransmission.DIO
{
    public class RfTransmitter
    {
        private readonly OutputPort _port;

        private readonly uint _delayInMicroseconds;
        private readonly uint _address;
        private readonly int _repeats;

        public RfTransmitter(OutputPort port, uint address, uint delayInMicroseconds, int repeats)
        {
            _port = port;
            _delayInMicroseconds = delayInMicroseconds;
            _address = address;
            _repeats = (1 << repeats) - 1;
        }

        public void Send(byte unit, bool on)
        {
            for (var i = _repeats; i >= 0; i--)
            {
                SendStartPulse();

                SendAddress(); 

                SendGroupBit();

                SendOnOff(on);

                SendUnit(unit);

                SendStopPulse();
            }
        }



        public void SendStartPulse() //1T high, 10.44T
        {
            _port.Write(true);
            DelayMicroseconds(_delayInMicroseconds);
            _port.Write(false);
            DelayMicroseconds(_delayInMicroseconds * 10 + (_delayInMicroseconds >> 1));
        }

        public void SendAddress() // 26 bits
        {
            for (var i = 25; i >= 0; i--)
            {
                SendBit(((_address >> i) & 1) != 0);
            }
        }

        public void SendGroupBit() // 1 bit
        {
            SendBit(false); // always targetting a unit
        }

        public void SendOnOff(bool @on) // 1bit
        {
            SendBit(@on);
        }

        public void SendUnit(byte unit) //4 bits
        {
            for (var i = 3; i >= 0; i--)
            {
                SendBit((unit & 1 << i) != 0);
            }
        }

        private void SendStopPulse() //1T high, 40T low
        {
            _port.Write(true);
            DelayMicroseconds(_delayInMicroseconds);
            _port.Write(false);
            DelayMicroseconds(_delayInMicroseconds * 40);
        }

        private void SendBit(bool one)
        {
            if (one) // 1 // T, 5T, T, T
            {
                _port.Write(true);
                DelayMicroseconds(_delayInMicroseconds);
                _port.Write(false);
                DelayMicroseconds(_delayInMicroseconds*5);
                _port.Write(true);
                DelayMicroseconds(_delayInMicroseconds);
                _port.Write(false);
                DelayMicroseconds(_delayInMicroseconds);
            }
            else // 0 // T, T, T, 5T
            {
                _port.Write(true);
                DelayMicroseconds(_delayInMicroseconds);
                _port.Write(false);
                DelayMicroseconds(_delayInMicroseconds);
                _port.Write(true);
                DelayMicroseconds(_delayInMicroseconds);
                _port.Write(false);
                DelayMicroseconds(_delayInMicroseconds * 5);
            }
        }

        private void DelayMicroseconds(long microseconds)
        {
            //to bad Thread.Sleep only supports milliseconds, this eats battery
            var delayTime = microseconds * 10;
            var delayStart = Utility.GetMachineTime().Ticks;
            while ((Utility.GetMachineTime().Ticks - delayStart) < delayTime);
        }

    }
}