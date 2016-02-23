using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public abstract class ObdPort
    {
        private readonly Dictionary<int, List<int>> _supportedPids = new Dictionary<int, List<int>>();

        public virtual void Connect()
        {
            Connected = true;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }

        public Protocol Protocol { get; set; }

        public bool Connected { get; protected set; }

        public ObdResponse GetPidData(ObdRequest req)
        {
            return (ObdResponse)GetPidData(req.Mode, req.Pid);
        }

        internal Dictionary<int, byte[]> GetPidData(byte mode, byte pid)
        {
            // if it's not a supported pid, don't poll it
            if (pid != 0x00 && pid != 0x20 && pid != 0x40)
            {
                var found = _supportedPids.Values.Any(x => x.Contains(pid));
                if (!found) return null;
            }

            //todo: so i'm guessing here we should ask for the data? 

            //todo: appears to be limited to the 'supported PIDs,' which is something I knew we had from the original code, but not sure if we're actually getting that supported list anywhere currently

            //todo: SendCommand(mode, pid)? seems like we'd want either some sort of synchronous wait here, or a callback delegate. but also not 100% sure what this method is trying to achieve

            

            //lock (_serial)
            //{
            //    string result;

            //    try
            //    {
            //        result = _serial.ReadLine();
            //    }
            //    catch (Exception)
            //    {
            //        Connected = false;
            //        //FireConnectionChangedEvent(_connected);
            //        throw; //todo-jpd: stop throwing and start fixing
            //    }

            //    //this is trying to be resilient - retry but fail at some point. ABR
            //    if (result.Contains("NO DATA") || result.Contains("ERROR"))
            //    {
            //        if (_errorCount++ > 10)
            //            Disconnect();

            //        return null;
            //    }

            //todo: moved everything to ParseEcuResponse
            //todo: figure out what data actually needs to come in here and from where it comes
            return ParseEcuResponse(string.Empty);

        }

        private Dictionary<int, byte[]> ParseEcuResponse(string result)
        {
            var payload = new Dictionary<int, byte[]>();

            var ecuResponses = result.Trim().Split('\r');
            foreach (var ecuResponse in ecuResponses)
            {
                var offset = 0;
                var ecuByte = 0;

                //todo-jpd: ask jack about what should happen here
                switch (Protocol)
                {
                    //case 3:
                    //    offset = 5;
                    //    ecuByte = 2;
                    //    break;
                    //case 6:
                    //    offset = 4;
                    //    ecuByte = 0;
                    //    break;
                    //case 7:
                    //    offset = 7;
                    //    ecuByte = 3;
                    //    break;
                    case Protocol.Unknown:
                        break;
                    case Protocol.ElmAutomatic:
                        break;
                    case Protocol.SaeJ1850Pwm:
                        break;
                    case Protocol.SaeJ1850Vpw:
                        break;
                    case Protocol.Iso142304Kwp5Baud104Kbaud:
                        break;
                    case Protocol.Iso142304KwpFast104Kbaud:
                        break;
                    case Protocol.Iso157654Can11Bit250Kbaud:
                        break;
                    case Protocol.Iso157654Can29Bit250Kbaud:
                        break;
                    case Protocol.Iso91412:
                        break;
                    case Protocol.Iso157654Can11Bit500Kbaud:
                        break;
                    case Protocol.Iso157654Can29Bit500Kbaud:
                        break;
                    default:
                        throw new ObdException("Unhandled protocol type.");
                }

                var strings = ecuResponse.Trim().Split(' ');
                var bytes = new byte[strings.Length - offset - 1];   // get rid of the header and the trailing checksum byte

                for (var i = offset; i < strings.Length - 1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(strings[i]) && !strings[i].Contains("STOPPED"))
                        bytes[i - offset] = Convert.ToByte(strings[i].Trim(), 16);
                }

                payload[Convert.ToInt32(strings[ecuByte].Trim(), 16)] = bytes;
            }
            return payload;
        }

        public abstract void SendCommand(string cmd);

        public abstract Task<string> ReadResponse();

        public abstract ObdResponse GetPidData(int mode, int pid);
    }
}