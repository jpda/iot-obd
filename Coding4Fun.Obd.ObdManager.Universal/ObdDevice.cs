using System;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class ObdDevice
    {
        public event EventHandler<ConnectionChangedEventArgs> ObdConnectionChanged;
        public ObdPort ObdPort { get; set; }

        public void Connect(ObdPort obdport)
        {
            ObdPort = obdport;
            Connect();
        }
        private void Connect()
        {
            ObdPort.Connect();
            if (Convert.ToInt32(ObdPort.Protocol) > 9 || ObdPort.Protocol != Protocol.Unknown)
                throw new ArgumentOutOfRangeException(ObdPort.Protocol.ToString(), "Protocol must be a value between of known type Int(1 and 9), inclusive.");
            if (ObdPort == null)
            {
                throw new ObdException("OBDPort not specified.");
            }

            ObdPort.Connect();
            FireConnectionChangedEvent(ObdPort.Connected);

        }
        public void Disconnect()
        {
            ObdPort.Disconnect();
            FireConnectionChangedEvent(ObdPort.Connected);
        }


        public ObdState GetCurrentState()
        {
            var os = new ObdState();
            //run the object builder routine. 

            return os;
        }
        public ObdResponse GetPidData(ObdRequest req)
        {
            return this.ObdPort.GetPidData(req);
        }
        public ObdResponse Ping()
        {
            //check to see if the port is open and responsive
            return GetPidData(new ObdRequest(0x01, 0x0C));

        }
        private void FireConnectionChangedEvent(bool connected)
        {
            ObdConnectionChanged?.Invoke(this, new ConnectionChangedEventArgs { Connected = connected });
        }
    }
}
