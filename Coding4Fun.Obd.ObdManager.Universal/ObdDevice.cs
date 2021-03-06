﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class ObdDevice
    {
        public event EventHandler<ConnectionChangedEventArgs> ObdConnectionChanged;

        public ObdPort ObdPort { get; set; }
        public bool Connected { get; private set; }

        private Dictionary<int, List<int>> _supportedPids = new Dictionary<int, List<int>>();
        private int _currentEcu;
        public const int UnsupportedPidValue = -1;

        public void Connect(ObdPort obdport)
        {
            ObdPort = obdport;
            Connect();
        }
        private void Connect()
        {
            //shoudl be implemented in async likely
            ObdPort.Connect();

            if (Convert.ToInt32(ObdPort.Protocol) > 9 || ObdPort.Protocol != Protocol.Unknown)
                throw new ArgumentOutOfRangeException(ObdPort.Protocol.ToString(), "Protocol must be a value between of known type Int(1 and 9), inclusive.");
            if (ObdPort == null)
            {
                throw new ObdException("OBDPort not speficied.");
            }
            if (ObdPort.Connected)
            {
                InitializeDevice();
                this.Connected = true;
            }
            else
            {
                if (ObdPort == null)
                {
                    throw new ObdException("OBDPort connect failed.");
                }
            }

            FireConnectionChangedEvent(ObdPort.Connected);

        }


        private void InitializeDevice()
        {
            //new List<string>() { "ATZ", "ATE0", "ATL0", "ATH1", "ATSP 5" }.ForEach(SendCommand);
            GetSupportedPids();
        }

        public string SendCommand(string command)
        {
            //command send
            this.ObdPort.SendCommand(command);
            //find response
            return "";
        }

        public void Disconnect()
        {
            ObdPort.Disconnect();
            FireConnectionChangedEvent(ObdPort.Connected);
        }

        public Dictionary<int, List<int>> GetSupportedPids()
        {
            var supportedPids = new Dictionary<int, List<int>>();

            var result = ObdPort.GetPidData(0x01, 0x00);
            foreach (var payload in result)
                supportedPids[payload.Key] = DecodeSupportedPids(payload.Value, 0x00);

            result = ObdPort.GetPidData(0x01, 0x20);
            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x20));

            result = ObdPort.GetPidData(0x01, 0x40);
            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x40));

            return supportedPids;
        }
        private static List<int> DecodeSupportedPids(IReadOnlyList<byte> data, int offset)
        {
            var pids = new List<int>();

            if (data == null)
                return pids;

            for (var byteIdx = 0; byteIdx < data.Count; byteIdx++)
            {
                for (var i = 0; i < 8; i++)
                {
                    if ((data[byteIdx] << i & 0x80) == 0x80)
                        pids.Add(byteIdx * 8 + i + 1 + offset);
                }
            }
            return pids;
        }


        public ObdState GetCurrentState()
        {
            var os = new ObdState();
            //run the object builder routine. 

            return os;
        }

        public ObdResponse GetPidData(ObdRequest req)
        {
            return ObdPort.GetPidData(req);
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

        public double GetKilometersPerGallon(int kph, double massAirflow)
        {
            //only calculate if supported
            if (kph == UnsupportedPidValue || massAirflow == UnsupportedPidValue)
                return UnsupportedPidValue;

            return ObdHelpers.GetKilometersPerGallon(kph, massAirflow);
        }

        public double GetMilesPerGallon(int mph, double massAirflow)
        {
            //only calculate if supported
            if (mph == UnsupportedPidValue || massAirflow == UnsupportedPidValue)
                return UnsupportedPidValue;

            return ObdHelpers.GetMilesPerGallon(mph, massAirflow);
        }

        public int GetMilesPerHour(int kph)
        {
            return kph == UnsupportedPidValue ? UnsupportedPidValue : ObdHelpers.KilometersPerHourToMilesPerHour(kph);
        }

        public bool GetMilLightOn()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x01);
            if (result == null || !result.ContainsKey(_currentEcu))
                return false;
            return (result[_currentEcu][0] & 0x80) == 0x80;
        }

        public int GetEngineLoad()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x04);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return 100 * result[_currentEcu][0] / 255;
        }

        public int GetEngineCoolantTemperature()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x05);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0] - 40;
        }

        public int GetRpm()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x0C);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return (result[_currentEcu][0] * 256 + result[_currentEcu][1]) / 4;
        }

        public int GetKilometersPerHour()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x0D);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0];
        }

        public int GetIntakeAirTemperature()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x0F);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0] - 40;
        }

        public double GetMassAirflowRate()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x10);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return ((result[_currentEcu][0] * 256) + result[_currentEcu][1]) / 100.0;
        }

        public int GetThrottlePosition()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x11);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return 100 * result[_currentEcu][0] / 255;
        }

        public int GetRuntimeSinceEngineStart()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x1F);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0] * 256 + result[_currentEcu][1];
        }

        public int GetDistanceTraveledWithMilOn()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x21);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0] * 256 + result[_currentEcu][1];
        }

        public int GetFuelLevelInput()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x2F);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return 100 * result[_currentEcu][0] / 255;
        }

        public int GetBarometricPressure()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x33);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0];
        }

        public int GetRelativeThrottlePosition()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x45);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return 100 * result[_currentEcu][0] / 255;
        }

        public int GetFuelType()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x51);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return 100 * result[_currentEcu][0] / 255; // TODO: enum
        }

        public int GetOilTemperature()
        {
            var result = this.ObdPort.GetPidData(0x01, 0x5c);
            if (result == null || !result.ContainsKey(_currentEcu))
                return UnsupportedPidValue;
            return result[_currentEcu][0] - 40;
        }

       
    }

    public abstract class BasePid : IPid
    {
        protected BasePid(object rawValue)
        {
            RawValue = rawValue;
        }

        public string RequestIdentity { get; set; }

        public object RawValue { get; set; }

        protected abstract void ComputeValue();
    }

    public interface IPid
    {
        string RequestIdentity { get; set; }

        object RawValue { get; set; }

    }

    public class Pid
    {
        public T GetComputedValue<T>(string name, Func<byte[], T> translator)
        {
            var things = new Dictionary<string, byte[]>() { { "Rpm", new byte[8] } };
            var value = things[name];

            var result = translator(value);
            return result;
        }

        public void DoThing()
        {
            var stuff = GetComputedValue("vin", x => Encoding.ASCII.GetString(x));
            var rpm = GetComputedValue("rpm", x =>
            {
                var rpmString = Encoding.ASCII.GetString(x);
                var rpmNumber = int.Parse(rpmString);
                return rpmNumber * 140 * 120;
            });
        }

        public string GetValue(byte[] stuff)
        {
            return Encoding.ASCII.GetString(stuff);
        }
    }
}