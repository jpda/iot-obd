﻿using System;
using Windows.Devices.SerialCommunication;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class ObdSerialPort : ObdPort
    {
        private SerialDevice  _serial;
        public ObdSerialPort(string comPort, int baud)
        {
            Baud = baud;
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
           
        }

        protected override ObdResponse GetPidData(int mode, int pid)
        {
            return new ObdResponse();
        }
    }
}