using System;

namespace Coding4Fun.Obd.ObdManager.Universal
{
	public class ObdDevice
	{
		public event EventHandler<ConnectionChangedEventArgs> ObdConnectionChanged;
        public ObdPort ObdPort{get; set;}
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
            ObdPort.Connect();
            if (Convert.ToInt32(ObdPort.Protocol) > 9 || ObdPort.Protocol != Protocol.Unknown)
                throw new ArgumentOutOfRangeException(ObdPort.Protocol.ToString(), "Protocol must be a value between of known type Int(1 and 9), inclusive.");
            if (ObdPort == null)
            {
                throw new ObdException("OBDPort not speficied.");
            }
            else { 
                this.ObdPort.Connect();
                GetSupportedPids();
                FireConnectionChangedEvent(this.ObdPort.Connected);
            }
            
            ObdPort.Connect();
            FireConnectionChangedEvent(ObdPort.Connected);

        }
        public void Disconnect()
        {
            ObdPort.Disconnect();
            FireConnectionChangedEvent(ObdPort.Connected);
        }

        public Dictionary<int, List<int>> GetSupportedPids()
        {
            Dictionary<int, List<int>> supportedPids = new Dictionary<int, List<int>>();
        
            var result = this.ObdPort.GetPidData(0x01, 0x00);
            foreach (var payload in result)
                supportedPids[payload.Key] = DecodeSupportedPids(payload.Value, 0x00);

            result = this.ObdPort.GetPidData(0x01, 0x20);
            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x20));

            result = this.ObdPort.GetPidData(0x01, 0x40);
            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x40));

            return supportedPids;
        }
        private List<int> DecodeSupportedPids(byte[] data, int offset)
        {
            List<int> pids = new List<int>();

            if (data == null)
                return pids;

            for (int byteIdx = 0; byteIdx < data.Length; byteIdx++)
            {
                for (int i = 0; i < 8; i++)
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
            if (kph == UnsupportedPidValue)
                return UnsupportedPidValue;

            return ObdHelpers.KilometersPerHourToMilesPerHour(kph);
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
}
