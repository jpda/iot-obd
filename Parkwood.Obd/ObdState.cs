using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdState : Dictionary<string, object>, IDisposable
    {
        public static ObdPort Port;
        private static ObdState singleton;
        private Dictionary<int, List<int>> SupportedPids = new Dictionary<int, List<int>>();

        public string VIN { get; private set; }
        public DateTime GenDateTime = DateTime.MinValue;
        public ObdProtocol Protocol {get; private set;}

        public static ObdState CurrentState
        {
            get
            {
                //check to see if the port is ready
                if (Port == null)
                {
                    throw new ObdException("Port is null");
                }
                //decide what needs to be built or refreshed 
                if (singleton == null)
                {
                    //singleton needs building
                    singleton = new ObdState();
                    //ask for supported pids. 
                    singleton.GetSupportedPids();
                    //ask for vin
                    //get protocol
                    /*.......*/
                }

                //refresh state

                //refresh the time
                singleton.GenDateTime = DateTime.Now;

                
                //decode known pids
                return singleton;
            }
        }
       
        public static void Reset()
        {
            //disconnect and kill the singleton
            singleton = null;
        }
       

        public void Dispose()
        {
            Reset();
        }
        /// <summary>
        /// Runs as singleton
        /// </summary>
        private ObdState()
        {
        }
        private void GetSupportedPids()
        {
            var supportedPids = new Dictionary<int, List<int>>();

            //get PIDs 1-20 support
            var result = ParsePidCmd(Port.SendCommand(new ObdPid(0x01, 0x00).PidCommand));

            foreach (var payload in result)
                supportedPids[payload.Key] = DecodeSupportedPids(payload.Value, 0x00);

            //get PIDs 21-40 support
            result = ParsePidCmd(Port.SendCommand(new ObdPid(0x01, 0x20).PidCommand));

            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x20));

            //get PIDs 21-40 support
            result = ParsePidCmd(Port.SendCommand(new ObdPid(0x01, 0x40).PidCommand));
            foreach (var payload in result)
                supportedPids[payload.Key].AddRange(DecodeSupportedPids(payload.Value, 0x40));

            this.SupportedPids =  supportedPids;
        }
        private Dictionary<int, byte[]> ParsePidCmd(string cmdResult)
        {
            Dictionary<int, byte[]> payload = new Dictionary<int, byte[]>();
            if (cmdResult.Contains("NO DATA") || cmdResult.Contains("ERROR"))
            {
                //commandResult is bad. Eject.   
                return null;
            }
            //there maybe multiple lines to deal with
            string[] ecuResponses = cmdResult.Trim().Split('\r');

            foreach (string ecuResponse in ecuResponses)
            {
                int offset = 5;
                int ecuByte = 3;
                //case 3:
                //        offset = 5;
                //ecuByte = 2;
                //break;
                //    case 6:
                //        offset = 4;
                //ecuByte = 0;
                //break;
                //    case 7:
                //        offset = 7;
                //ecuByte = 3;
                //break;


                switch (this.Protocol)
                {
                    case ObdProtocol.Unknown:
                        break;
                    case ObdProtocol.ElmAutomatic:
                        break;
                    case ObdProtocol.SaeJ1850Pwm:
                        break;
                    case ObdProtocol.SaeJ1850Vpw:
                        break;
                    case ObdProtocol.Iso91412:
                        break;
                    case ObdProtocol.Iso142304Kwp5Baud104Kbaud:
                        break;
                    case ObdProtocol.Iso142304KwpFast104Kbaud:
                        break;
                    case ObdProtocol.Iso157654Can11Bit500Kbaud:
                        break;
                    case ObdProtocol.Iso157654Can29Bit500Kbaud:
                        break;
                    case ObdProtocol.Iso157654Can11Bit250Kbaud:
                        break;
                    case ObdProtocol.Iso157654Can29Bit250Kbaud:
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
        private List<int> DecodeSupportedPids(IReadOnlyList<byte> data, int offset)
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


    }
}
