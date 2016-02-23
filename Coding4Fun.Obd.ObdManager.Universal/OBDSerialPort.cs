using System;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class ObdSerialPort : ObdPort
    {
        private SerialDevice _serial;

        public ObdSerialPort(string comPort, int baud, SerialDevice serial)
        {
            Baud = baud;
            _serial = serial;
            ComPort = comPort;
        }

        public string ComPort { get; set; }

        public int Baud { get; set; }

        public override void Connect()
        {
            Connect(ComPort, Baud, Convert.ToInt32(Protocol));
        }

        public void Connect(string comPort, int baud)
        {
            Connect(comPort, baud, Convert.ToInt32(Protocol));
        }

        public void Connect(string comPort, int baud, int protocol)
        {
            //implement connect logic
            base.Connect();
        }

        public override void Disconnect()
        {
            //implement disconnect logic
            base.Disconnect();
        }

        //protected internal ObdResponse GetPidData(byte mode, byte pid)
        //{
        //    return new ObdResponse();
        //}

        public override void SendCommand(string cmd)
        {
            throw new NotImplementedException();
        }

        public override Task<string> ReadResponse()
        {
            throw new NotImplementedException();
        }

        public override ObdResponse GetPidData(int mode, int pid)
        {
            throw new NotImplementedException();
        }
    }
}
