using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;



namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class OBDSerialPort : ObdPort
    {
        private SerialDevice  _serial;
        public OBDSerialPort(string comPort, int baud)
        {
            this.Baud = baud;
            this.ComPort = comPort;
        }

        public string ComPort { get; set; }
        public int Baud { get; set; }

        public override void Connect()
        {
            Connect(this.ComPort, this.Baud, Convert.ToInt32(this.Protocol));
        }
        public void Connect(string comPort, int baud)
        {
            Connect(comPort, baud, Convert.ToInt32(this.Protocol));
        }

        public void Connect(string comPort, int baud, int protocol)
        {
           
        }

        public override void Disconnect()
        {
            //if (_serial.IsOpen)
            //{
            //    _serial.Close();
            //}
            
            base.Disconnect();
        }


        protected override ObdResponse GetPidData(int mode, int pid)
        {
            return new ObdResponse();
        }
    }
}
