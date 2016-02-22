using System;
using System.Collections.Generic;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public abstract class ObdPort
    {
        private Dictionary<int, List<int>> _supportedPids = new Dictionary<int, List<int>>();
        public virtual void Connect()
        {
            this.Connected = true;
        }
        public virtual void Disconnect()
        {
            this.Connected = false;
        }
        public Protocol Protocol { get; set; }
        public bool Connected { get; protected set; }
        public ObdResponse GetPidData(ObdRequest req)
        {
            return (ObdResponse)GetPidData(req.Mode, req.Pid);
        }
        internal Dictionary<int, byte[]> GetPidData(byte mode, byte pid)
        {
            Dictionary<int, byte[]> payload = new Dictionary<int, byte[]>();

            // if it's not a supported pid, don't poll it
            if (pid != 0x00 && pid != 0x20 && pid != 0x40)
            {
                bool found = false;

                foreach (List<int> values in _supportedPids.Values)
                {
                    found = values.Contains(pid);
                    if (found)
                        break;
                }

                if (!found)
                    return null;
            }

            lock (_serial)
            {
                string result;

                Debug.Write("PID: " + pid.ToString("X2") + ", ");

                try
                {
                    WriteLine(mode.ToString("X2") + pid.ToString("X2"));
                    result = _serial.ReadLine();
                }
                catch (Exception)
                {
                    _connected = false;
                    FireConnectionChangedEvent(_connected);
                    throw;
                }

                Debug.Write(result);

                if (result.Contains("NO DATA") || result.Contains("ERROR"))
                {
                    if (_errorCount++ > 10)
                        Disconnect();

                    return null;
                }

                string[] ecuResponses = result.Trim().Split('\r');
                foreach (string ecuResponse in ecuResponses)
                {
                    int offset;
                    int ecuByte;

                    switch (_protocol)
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
                    byte[] bytes = new byte[strings.Length - offset - 1];   // get rid of the header and the trailing checksum byte

                    for (int i = offset; i < strings.Length - 1; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(strings[i]) && !strings[i].Contains("STOPPED"))
                            bytes[i - offset] = Convert.ToByte(strings[i].Trim(), 16);
                    }

                    payload[Convert.ToInt32(strings[ecuByte].Trim(), 16)] = bytes;
                }

                return payload;
            }
        }
        public abstract void SendCommand(string cmd);
        public abstract string ReadResponse();

    }
}