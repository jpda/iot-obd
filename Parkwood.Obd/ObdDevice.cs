using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;



namespace Parkwood.Obd
{
    public class ObdDevice : IObservable<ObdState>
    {

        private List<IObserver<ObdState>> observers;
        private ObdPort Port;
        private bool publish;

        private Dictionary<int, List<int>> SupportedPids = new Dictionary<int, List<int>>();

        public ObdProtocol Protocol { get; set; }

        public ObdDevice(ObdPort p)
        {
            //setup the ports
            Port = p;
            Port = p;
            observers = new List<IObserver<ObdState>>();

            //begin publishing pid data
            Publish();
        }

        private void Publish()
        {
            //connect and load supported pids
            Port.Connect();
            GetSupportedPids();

            publish = true;
            //loop and publish state as often as possible
            while (publish)
            {
                if (observers.Count > 0) {
                    //async call to build state from OdbPort
                    PublishState();
                }
            }
        }

        public IDisposable Subscribe(IObserver<ObdState> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            //return an object to allow unsubscription
            return new Unsubscriber(observers, observer);
        }

        private void PublishState()
        {
            ObdState os = new ObdState();
            


            //set vin
            //get the desired pids

            foreach (var observer in observers)
            {
                //publish state to each subscriber
                observer.OnNext(os);
            }
        }

        /// <summary>
        /// Call when no further publications will be made to clear subscribers
        /// </summary>
        public void EndTransmission()
        {
            //close observers
            foreach (var observer in observers.ToArray())
                if (observers.Contains(observer))
                    observer.OnCompleted();
            //adios
            observers.Clear();
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

            SupportedPids = supportedPids;
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
                int offset;
                int ecuByte;

                switch (Protocol)
                {
                    case ObdProtocol.Iso91412:
                        offset = 5;
                        ecuByte = 2;
                        break;
                    case ObdProtocol.Iso157654Can11Bit500Kbaud:
                        offset = 4;
                        ecuByte = 0;
                        break;
                    case ObdProtocol.Iso157654Can29Bit500Kbaud:
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

C:\Users\jackbrad\Documents\Visual Studio 2015\Projects\Parkwood\Parkwood.Obd\WellKnownPids.xml
        public static ObdState GetCurrentState()
        {
            //check to see if the port is ready
            if (Port == null)
            {
                throw new ObdException("Port is null");
            }

            ObdState os = new ObdState();
            //refresh state
            //ask for the preferred Pids



            //refresh the time
            os.GenDateTime = DateTime.Now;

            //decode known pids
            return os;

        }

        private void GetPreferredPidsList()
        {
            XDocument xpids = XDocument.Load("WellKnownPids.xml");

            List<ObdPid> pid =
                          (from xPid in xpids.Element("Document").Elements("MessageType")
                           select new ObdPid
                           {
                               Pid = int.Parse(xPid.Element("tradegood").Attribute("id").Value),
                               ProductName = xPid.Element("tradegood").Value,
                               BasePrice = double.Parse(xPid.Element("baseprice").Value),
                               MaxQuantity = int.Parse(xPid.Element("quantity").Value),
                               
                           }).ToList;


        }


    }
}
