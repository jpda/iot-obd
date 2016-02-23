using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Coding4Fun.Obd.ObdManager
{
    class ObdSerialPort : ObdPort
    {
        private SerialPort _serial;

        public string ComPort { get; set; }
        public int Baud { get; set; }
        public int Protocol { get; set; }
        public new bool Poll { get; set; }

        public override bool Connected
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }



        public void Connect()
        {
            Connect(this.ComPort, this.Baud, this.Protocol,this.Poll);
        }
        public void Connect(string comPort, int baud)
        {
            Connect(comPort, baud, this.Protocol, this.Poll);
        }

        public void Connect(string comPort, int baud, int protocol)
        {
            Connect(comPort, baud, protocol, false);
        }

        public void Connect(string comPort, int baud, int protocol, bool poll)
        {
            if (protocol > 9 || protocol != UnknownProtocol)
                throw new ArgumentOutOfRangeException(protocol.ToString(), "Protocol must be a value between 1 and 9, inclusive.");

            ObdState = new ObdState();

            _serial = new SerialPort(comPort, baud);
            _serial.NewLine = ">";      // responses end with the > prompt character
            _serial.Open();

            _errorCount = 0;
            _connected = true;
            FireConnectionChangedEvent(_connected);

            LastResponse = WriteAndCheckResponse("ATZ"); // reset
            LastResponse = WriteAndCheckResponse("ATE0"); // echo off
            LastResponse = WriteAndCheckResponse("ATL0"); // line feeds off

            // no longer allow the ELM's auto detect since we need to know which protocol we're using
            if (protocol == UnknownProtocol || protocol == 0)
            {
                for (protocol = 1; protocol <= 9; protocol++)
                {
                    LastResponse = WriteAndCheckResponse("ATSP" + protocol); // OBD protocol
                    try
                    {
                        LastResponse = WriteAndCheckResponse("01 00");  // send command to initialize comm bus (i.e. get PIDs supported)
                        break;
                    }
                    catch (ObdException)
                    {
                        Trace.WriteLine("It's not protocol " + protocol);
                    }
                }

                if (protocol == 10)
                    throw new ObdException("Could not find compatible protocol. Ensure the cable is securely connected to the OBD port on the vehicle.");
            }
            else
            {
                LastResponse = WriteAndCheckResponse("ATSP" + protocol); // OBD protocol
                LastResponse = WriteAndCheckResponse("01 00");  // send command to initialize comm bus (i.e. get PIDs supported)
            }

            _protocol = protocol;

            LastResponse = WriteAndCheckResponse("ATH1"); // turn on headers (needed for ECU)

            _supportedPids = GetSupportedPids();

            int count = 0;

            foreach (var pidEntry in _supportedPids)
            {
                if (pidEntry.Value.Count > count)
                {
                    _currentEcu = pidEntry.Key;
                    count = pidEntry.Value.Count;
                }
            }

            Trace.WriteLine("Using ECU " + _currentEcu + " with " + count + " PIDs");

            if (poll)
            {
                _event = new AutoResetEvent(false);

                Thread t = new Thread(PollObd);
                t.IsBackground = true;
                t.Name = "ObdPoller";
                t.Start();
            }
        }

        public void Disconnect()
        {
            if (!_serial.IsOpen)
                return;

            _connected = false;

            // wait for the poller to end
            if (_event != null)
                _event.WaitOne(2000, false);

            _serial.Close();

        }

        public void WriteLine(string line)
        {
            _serial.Write(line + "\r");
        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            //close the connection
        }

        public override void WriteLine(string line)
        {
            throw new NotImplementedException();
        }
    }
}
