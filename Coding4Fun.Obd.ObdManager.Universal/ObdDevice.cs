using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.SerialCommunication;

using System.Threading;
using Coding4Fun.Obd.ObdManager.Universal;

namespace Coding4Fun.Obd.ObdManager.Universal
{
	public class ObdDevice
	{
		private AutoResetEvent _event;
		public event EventHandler<ConnectionChangedEventArgs> ObdConnectionChanged;
        public ObdPort ObdPort{get; set;}
       
        public void Connect(ObdPort obdport)
        {
            this.ObdPort = obdport;
            Connect();
        }
        private void Connect()
        {
            ObdPort.Connect();
            if (Convert.ToInt32(this.ObdPort.Protocol) > 9 || this.ObdPort.Protocol != Protocol.Unknown)
                throw new ArgumentOutOfRangeException(this.ObdPort.Protocol.ToString(), "Protocol must be a value between of known type Int(1 and 9), inclusive.");
            if (this.ObdPort == null)
            {
                throw new ObdException("OBDPort not speficied.");
            }
            else { 
                this.ObdPort.Connect();
                FireConnectionChangedEvent(this.ObdPort.Connected);
            }
            
        }
        public void Disconnect()
        {
            ObdPort.Disconnect();
            FireConnectionChangedEvent(this.ObdPort.Connected);
        }

        
        public ObdState GetCurrentState()
        {
            ObdState os = new ObdState();
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
			if(ObdConnectionChanged != null)
				ObdConnectionChanged(this, new ConnectionChangedEventArgs { Connected = connected });
		}



    }
}
