using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Parkwood.Stuff;

namespace Parkwood.Obd
{
    public class ObdDevice : IObservable<State>
    {
        private readonly List<IObserver<State>> _observers;
        private bool _publish;
        private readonly ObdPort _port;
        private List<ObdEcu> _ecus = new List<ObdEcu>();
        private List<ObdPid> _desiredPids = new List<ObdPid>();
        private readonly Protocol _protocol;
        private State _state;

        //todo: we know this is our protocol for testing; need a better discovery mechanism
        public ObdDevice(ObdPort port, Protocol protocol = Protocol.Iso157654Can11Bit500Kbaud)
        {
            _observers = new List<IObserver<State>>();
            _port = port;
            _protocol = protocol;
            _port.Connect();
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
            new List<string>() { "ATZ", "ATE0", "ATL0", "ATSP6", "ATH1" }.ForEach(x => System.Diagnostics.Debug.WriteLine(_port.SendCommand(x)));
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

        private void GetState()
        {
            _state = new State() { };

            var targetPidList = new List<ObdPid>();

            foreach (var ecu in _ecus)
            {
                Logger.DebugWrite($"Attempting to join {_desiredPids.Count} desired PIDs with {ecu.Pidz.Count} supported PIDs for ECU {ecu.Id}");
                Logger.DebugWrite($"Desired: {string.Join(", ", _desiredPids)}");
                Logger.DebugWrite($"Available: {string.Join(", ", ecu.Pidz)}");
                targetPidList.AddRange(ecu.Pidz.Where(y => _desiredPids.Select(x => x.Pid).Contains(y.Pid)));
            }

            Logger.DebugWrite($"Found {targetPidList.Count} pids to ask for.");

            foreach (var pid in targetPidList)
            {
                var pidVal = _port.SendCommand(pid.PidCommand);
                _state.PidValues.Add(pid.ToString(), pidVal);
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
        public IDisposable Subscribe(IObserver<State> observer)
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

        /// <summary>
        /// Startup method for asking the ECU for supported PIDs.
        /// </summary>
        private void GetSupportedPids()
        {
            var pidChunks = new List<string>() { "00", "20", "40" };

            var ecus = new List<ObdEcu>();

            foreach (var chunk in pidChunks)
            {
                var pidGetResult = PidDecoder.ParsePidCmd(_port.SendCommand(new ObdPid() { Mode = "01", Pid = chunk }.PidCommand), _protocol).ToList();
                foreach (var ecuLineResponse in pidGetResult)
                {
                    var ecu = new ObdEcu();
                    var ecuExists = ecus.Any(x => x.Id == ecuLineResponse.Key);

                    if (ecuExists)
                    {
                        ecu = ecus.Single(x => x.Id == ecuLineResponse.Key);
                    }

                    var pidz = PidDecoder.DecodeSupportedPids(ecuLineResponse.Value, int.Parse(chunk)).Select(x => new ObdPid() { Mode = "01", Pid = x.ToString("00") }).ToList();

                    if (ecu.Pidz?.Any() ?? false)
                    {
                        ecu.Pidz.AddRange(pidz);
                    }
                    else { ecu.Pidz = pidz; }
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

            var pidsForMe = pids.Select(x => new ObdPid() { Mode = "01", Name = x.Element("Name").Value, Pid = x.Element("PID").Value }).ToList();
            _desiredPids = pidsForMe;
        }
    }
}