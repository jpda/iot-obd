using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Coding4Fun.Obd.ObdManager
{
	public class ObdDevice
	{
		public const int UnknownProtocol = -1;
		public const int UnsupportedPidValue = -1;

		private SerialPort _serial;
		private AutoResetEvent _event;
		private readonly SynchronizationContext _context = SynchronizationContext.Current;
		private Dictionary<int,List<int>> _supportedPids = new Dictionary<int,List<int>>();
		private int _protocol;
		private int _currentEcu;
		private bool _connected;
		private int _errorCount;

		public event EventHandler<ObdChangedEventArgs> ObdChanged;
		public event EventHandler<ConnectionChangedEventArgs> ObdConnectionChanged;
	
		public string LastResponse { get; set; }
		public ObdState ObdState { get; set; }

		public void Connect(string comPort, int baud)
		{
			Connect(comPort, baud, UnknownProtocol, false);
		}

		public void Connect(string comPort, int baud, int protocol)
		{
			Connect(comPort, baud, protocol, false);
		}

		public void Connect(string comPort, int baud, int protocol, bool poll)
		{
			if(protocol > 9 || protocol != UnknownProtocol)
				throw new ArgumentOutOfRangeException(protocol.ToString(), "Protocol must be a value between 1 and 9, inclusive.");

			ObdState = new ObdState();

			_serial = new SerialPort(comPort, baud);
			_serial.NewLine = ">";		// responses end with the > prompt character
			_serial.Open();

			_errorCount = 0;
			_connected = true;
			FireConnectionChangedEvent(_connected);

			LastResponse = WriteAndCheckResponse("ATZ"); // reset
			LastResponse = WriteAndCheckResponse("ATE0"); // echo off
			LastResponse = WriteAndCheckResponse("ATL0"); // line feeds off

			// no longer allow the ELM's auto detect since we need to know which protocol we're using
			if(protocol == UnknownProtocol || protocol == 0)
			{
				for(protocol = 1; protocol <= 9; protocol++)
				{
					LastResponse = WriteAndCheckResponse("ATSP" + protocol); // OBD protocol
					try
					{
						LastResponse = WriteAndCheckResponse("01 00");	// send command to initialize comm bus (i.e. get PIDs supported)
						break;
					}
					catch(ObdException)
					{
						Trace.WriteLine("It's not protocol " + protocol);
					}
				}

				if(protocol == 10)
					throw new ObdException("Could not find compatible protocol. Ensure the cable is securely connected to the OBD port on the vehicle.");
			}
			else
			{
				LastResponse = WriteAndCheckResponse("ATSP" + protocol); // OBD protocol
				LastResponse = WriteAndCheckResponse("01 00");	// send command to initialize comm bus (i.e. get PIDs supported)
			}

			_protocol = protocol;

			LastResponse = WriteAndCheckResponse("ATH1"); // turn on headers (needed for ECU)

			_supportedPids = GetSupportedPids();

			int count = 0;

			foreach(var pidEntry in _supportedPids)
			{
				if(pidEntry.Value.Count > count)
				{
					_currentEcu = pidEntry.Key;
					count = pidEntry.Value.Count;
				}
			}

			Trace.WriteLine("Using ECU " + _currentEcu + " with " + count + " PIDs");

			if(poll)
			{
				_event = new AutoResetEvent(false);

				Thread t = new Thread(PollObd);
				t.IsBackground = true;
				t.Name = "ObdPoller";
				t.Start();
			}
		}

		private string WriteAndCheckResponse(string line)
		{
			WriteLine(line);
			string response = _serial.ReadLine();
			Debug.Write(line + ", " + response);
			if(!response.Contains("41") && !response.Contains("OK") && !response.Contains("ELM") && !response.Contains("SEARCHING"))
				throw new ObdException(string.Format("Error initializing OBD: {0} - {1}", line, response));
			return response;
		}

		public void Disconnect()
		{
			if(!_serial.IsOpen)
				return;

			_connected = false;

			// wait for the poller to end
			if(_event != null)
				_event.WaitOne(2000, false);

			_serial.Close();

			FireConnectionChangedEvent(_connected);
		}

		private void WriteLine(string line)
		{
			_serial.Write(line + "\r");
		}

		private void PollObd()
		{
			while(_connected)
			{
				UpdateState();

				if(_context != null)
				{
					_context.Post(delegate
					{
						if(ObdChanged != null)
							ObdChanged(this, new ObdChangedEventArgs(ObdState));
					}, null);
				}
				else
				{
					if(ObdChanged != null)
						ObdChanged(this, new ObdChangedEventArgs(ObdState));
				}
			}
			_event.Set();
		}

		private void FireConnectionChangedEvent(bool connected)
		{
			if(ObdConnectionChanged != null)
				ObdConnectionChanged(this, new ConnectionChangedEventArgs { Connected = connected });
		}

		public void UpdateState()
		{
			ObdState.KilometersPerHour = GetKilometersPerHour();
			ObdState.MassAirflowRate = GetMassAirflowRate();

			ObdState.Rpm = GetRpm();
			ObdState.MilesPerHour = GetMilesPerHour(ObdState.KilometersPerHour);
			ObdState.MilesPerGallon = GetMilesPerGallon(ObdState.MilesPerHour, ObdState.MassAirflowRate);

			ObdState.FuelLevel = GetFuelLevelInput();
			ObdState.EngineCoolantTemperature = GetEngineCoolantTemperature();
		}

		private Dictionary<int,byte[]> GetPidData(byte mode, byte pid)
		{
			if(!_connected)
				return null;

			Dictionary<int,byte[]> payload = new Dictionary<int, byte[]>();

			// if it's not a supported pid, don't poll it
			if(pid != 0x00 && pid != 0x20 && pid != 0x40)
			{
				bool found = false;

				foreach(List<int> values in _supportedPids.Values)
				{
					found = values.Contains(pid);
					if(found)
						break;
				}

				if(!found)
					return null;
			}

			lock(_serial)
			{
				string result;

				Debug.Write("PID: " + pid.ToString("X2") + ", ");

				try
				{
					WriteLine(mode.ToString("X2") + pid.ToString("X2"));
					result = _serial.ReadLine();
				}
				catch(Exception)
				{
					_connected = false;
					FireConnectionChangedEvent(_connected);
					throw;
				}

				Debug.Write(result);

				if(result.Contains("NO DATA") || result.Contains("ERROR"))
				{
					if(_errorCount++ > 10)
						Disconnect();

					return null;
				}

				string[] ecuResponses = result.Trim().Split('\r');
				foreach(string ecuResponse in ecuResponses)
				{
					int offset;
					int ecuByte;

					switch(_protocol)
					{
						case 3:
							offset = 5;
							ecuByte = 2;
							break;
						case 6:
							offset = 4;
							ecuByte = 0;
							break;
						case 7:
							offset = 7;
							ecuByte = 3;
							break;
						default:
							throw new ObdException("Unhandled protocol type.  Feel free to add it and send us the changes!");
					}

					string[] strings = ecuResponse.Trim().Split(' ');
					byte[] bytes = new byte[strings.Length-offset-1];	// get rid of the header and the trailing checksum byte

					for(int i = offset; i < strings.Length-1; i++)
					{
						if(!string.IsNullOrWhiteSpace(strings[i]) && !strings[i].Contains("STOPPED"))
							bytes[i-offset] = Convert.ToByte(strings[i].Trim(), 16);
					}

					payload[Convert.ToInt32(strings[ecuByte].Trim(), 16)] = bytes;
				}

				return payload;
			}
		}

		public Dictionary<int,List<int>> GetSupportedPids()
		{
			Dictionary<int,List<int>> supportedPids = new Dictionary<int,List<int>>();

			var result = GetPidData(0x01, 0x00);
			foreach(var payload in result)
				supportedPids[payload.Key] = DecodeSupportedPids(payload.Value, 0x00);

			result = GetPidData(0x01, 0x20);
			foreach(var payload in result)
				supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x20));

			result = GetPidData(0x01, 0x40);
			foreach(var payload in result)
				supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x40));

			return supportedPids;
		}

		private List<int> DecodeSupportedPids(byte[] data, int offset)
		{
			List<int> pids = new List<int>();

			if(data == null)
				return pids;

			for(int byteIdx = 0; byteIdx < data.Length; byteIdx++)
			{
				for(int i = 0; i < 8; i++)
				{
					if((data[byteIdx] << i & 0x80) == 0x80)
						pids.Add(byteIdx * 8 + i + 1 + offset);
				}
			}
			return pids;
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
			var result = GetPidData(0x01, 0x01);
			if(result == null || !result.ContainsKey(_currentEcu))
				return false;
			return (result[_currentEcu][0] & 0x80) == 0x80;
		}

		public int GetEngineLoad()
		{
			var result = GetPidData(0x01, 0x04);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return 100 * result[_currentEcu][0] / 255;
		}

		public int GetEngineCoolantTemperature()
		{
			var result = GetPidData(0x01, 0x05);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0] - 40;
		}

		public int GetRpm()
		{
			var result = GetPidData(0x01, 0x0C);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return (result[_currentEcu][0] * 256 + result[_currentEcu][1]) / 4;
		}

		public int GetKilometersPerHour()
		{
			var result = GetPidData(0x01, 0x0D);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0];
		}

		public int GetIntakeAirTemperature()
		{
			var result = GetPidData(0x01, 0x0F);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0] - 40;
		}

		public double GetMassAirflowRate()
		{
			var result = GetPidData(0x01, 0x10);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return ((result[_currentEcu][0] * 256) + result[_currentEcu][1]) / 100.0;
		}

		public int GetThrottlePosition()
		{
			var result = GetPidData(0x01, 0x11);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return 100 * result[_currentEcu][0] / 255;
		}

		public int GetRuntimeSinceEngineStart()
		{
			var result = GetPidData(0x01, 0x1F);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0] * 256 + result[_currentEcu][1];
		}

		public int GetDistanceTraveledWithMilOn()
		{
			var result = GetPidData(0x01, 0x21);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0] * 256 + result[_currentEcu][1];
		}

		public int GetFuelLevelInput()
		{
			var result = GetPidData(0x01, 0x2F);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return 100 * result[_currentEcu][0] / 255;
		}

		public int GetBarometricPressure()
		{
			var result = GetPidData(0x01, 0x33);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0];
		}

		public int GetRelativeThrottlePosition()
		{
			var result = GetPidData(0x01, 0x45);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return 100 * result[_currentEcu][0] / 255;
		}

		public int GetFuelType()
		{
			var result = GetPidData(0x01, 0x51);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return 100 * result[_currentEcu][0] / 255; // TODO: enum
		}

		public int GetOilTemperature()
		{
			var result = GetPidData(0x01, 0x5c);
			if(result == null || !result.ContainsKey(_currentEcu))
				return UnsupportedPidValue;
			return result[_currentEcu][0] - 40;
		}
	}
}
