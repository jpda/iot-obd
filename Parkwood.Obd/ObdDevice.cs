using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Parkwood.Stuff;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdDevice : IObservable<ObdState>
    {
        private readonly List<IObserver<ObdState>> _observers;
        private bool _publish;
        private readonly ObdPort _port;
        private List<ObdEcu> _ecus = new List<ObdEcu>();
        private List<ObdPid> _desiredPids = new List<ObdPid>();
        private readonly Protocol _protocol;
        private ObdState _state;
        private readonly List<ObdPid> _targetPids = new List<ObdPid>();

        //todo: we know this is our protocol for testing; need a better discovery mechanism
        public ObdDevice(ObdPort port, Protocol protocol = Protocol.Iso157654Can11Bit500Kbaud)
        {
            _observers = new List<IObserver<ObdState>>();
            _port = port;
            _protocol = protocol;
            
        }

        public async Task Connect()
        {
            do
            {
                await _port.Connect();
                if (_port.Connected) { return Task.FromResult(null); } //if we manage to connect, let's roll - otherwise, we'll delay
                Logger.DebugWrite("Can't connect to OBD device! Sleeping for a few...");
                Task.Delay(5000).Wait();
            } while (!_port.Connected);
        } 

        public void Startup()
        {
            Init();
            GetSupportedPids();
            GetPreferredPids();
            Publish();
        }

        private void Init()
        {
            //todo: may need to check these for Ok for proper response. Not required though. 
            new List<string>() { "ATZ", "ATE0", "ATL0", "ATSP6", "ATH1" }.ForEach(x => System.Diagnostics.Debug.WriteLine(_port.SendCommandWaitForString(x)));
        }

        private void Publish()
        {
            _publish = true;
            while (_publish)
            {
                if (_observers.Count > 0)
                {
                    PublishState();
                }
            }
        }

        private void PublishState()
        {
            GetState();
            foreach (var observer in _observers)
            {
                observer.OnNext(_state);
            }
        }

        /// <summary>
        /// Subscribe to publishing events.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>Unsubscription object.</returns>
        public IDisposable Subscribe(IObserver<ObdState> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        /// <summary>
        /// Call when no further publications will be made to clear subscribers
        /// </summary>
        public void EndTransmission()
        {
            foreach (var observer in _observers.ToArray().Where(observer => _observers.Contains(observer)))
                observer.OnCompleted();
            _observers.Clear();
        }

        private void BuildPidList()
        {
            foreach (var ecu in _ecus)
            {
                Logger.DebugWrite($"Attempting to join {_desiredPids.Count} desired PIDs with {ecu.SupportedPids.Count} supported PIDs for ECU {ecu.Id}");
                Logger.DebugWrite($"Desired: {string.Join(", ", _desiredPids.Select(x => x.Pid))}");
                Logger.DebugWrite($"Available: {string.Join(", ", ecu.SupportedPids.Select(x => x.Pid))}");

                //since our formulas were loaded from XML (_desiredPids), we'll select out of that list to preserve the extra data. should probably fix that at some point (E.g., not use ObdPid for GetSupportedPids)
                _targetPids.AddRange(_desiredPids.Where(x => ecu.SupportedPids.Select(y => y.Pid).Contains(x.Pid)));
            }

            Logger.DebugWrite($"Found {_targetPids.Count} pids to ask for.");

        }

        private void GetState()
        {
            _state = new ObdState(new List<EcuResponse>());

            if (!_targetPids.Any())
            {
                BuildPidList();
            }
            try
            {
                foreach (var pid in _targetPids)
                {
                    var pidVal = _port.SendCommandWaitForString(pid.PidCommand);
                    var parsedResult = PidDecoder.ParsePidCmd(pidVal, _protocol); //raw command - returns ECU dict key, byte array of values, strips ECU header

                    //build the EcuResponse and add it to state
                    foreach (var result in parsedResult) //get each ECU's response. likely only a single on for most calls, I'd assume. 
                    {
                        var ecuResponse = new EcuResponse(pid);
                        ecuResponse.ECU = result.Key.ToString();
                        ecuResponse.ReturnedRawData = result.Value;
                        _state._ecuResponses.Add(ecuResponse);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.DebugWrite($"{ex.Message}: at {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Startup method for asking the ECU for supported PIDs.
        /// </summary>
        private void GetSupportedPids()
        {
            var pidRequestRanges = new List<string>() { "00", "20", "40" };

            var ecus = new List<ObdEcu>();

            foreach (var chunk in pidRequestRanges)
            {
                var pidGetResult = PidDecoder.ParsePidCmd(_port.SendCommandWaitForString(new ObdPid() { Mode = "01", Pid = chunk }.PidCommand), _protocol).ToList();
                foreach (var ecuLineResponse in pidGetResult)
                {
                    var ecu = new ObdEcu();
                    var ecuExists = ecus.Any(x => x.Id == ecuLineResponse.Key);

                    if (ecuExists)
                    {
                        ecu = ecus.Single(x => x.Id == ecuLineResponse.Key);
                    }
                    else
                    {
                        ecu.Id = ecuLineResponse.Key;
                    }


                    var pidz = PidDecoder.DecodeSupportedPids(ecuLineResponse.Value, int.Parse(chunk)).Select(x => new ObdPid() { Mode = "01", Pid = x.ToString("X2") }).ToList();

                    if (ecu.SupportedPids?.Any() ?? false)
                    {
                        ecu.SupportedPids.AddRange(pidz);
                    }
                    else { ecu.SupportedPids = pidz; }
                    if (!ecuExists)
                    {
                        ecus.Add(ecu);
                    }
                }
            }
            _ecus = ecus;
        }

        private void GetPreferredPids()
        {
            var xpids = XDocument.Load("Assets/WellKnownPids.xml");
            var pids = xpids.Element("Document").Elements("MessageType");
            _desiredPids = pids.Select(x => new ObdPid() { Mode = x.Element("Mode").Value, Name = x.Element("Name").Value, Pid = x.Element("PID").Value, StandardValueType = x.Element("Units").Value, ConversionFunctionName = x.Element("ConversionFunctionName").Value }).ToList();
        }
    }
}