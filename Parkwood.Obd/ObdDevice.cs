using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Parkwood.Obd
{
    public class ObdDevice : IObservable<State>
    {
        private readonly List<IObserver<State>> _observers;
        private bool _publish;
        private readonly ObdPort _port;
        private Dictionary<string, object> _supportedPids = new Dictionary<string, object>();
        private List<ObdPid> _desiredPids = new List<ObdPid>();
        private readonly Protocol _protocol;
        private State _state;

        public ObdDevice(ObdPort port, Protocol protocol = Protocol.ElmAutomatic)
        {
            _port = port;
            //todo: how do we determine protocol? is that done via...?
            _protocol = protocol;
            GetSupportedPids();
            GetPreferredPids();
            _observers = new List<IObserver<State>>();
            Publish();
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

            var actualPids = _desiredPids.Where(x => _supportedPids.int)

            foreach (var pid in _supportedPids)
            {
                var pidVal = _port.SendCommand($"{pid.Key} {pid.Value}");
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
            var supportedPids = new Dictionary<string, List<string>>();

            //get PIDs 1-20 support
            var result = PidDecoder.ParsePidCmd(_port.SendCommand(new ObdPid { Mode = "01", Pid = "00" }.PidCommand), _protocol);

            foreach (var payload in result)
            {
                supportedPids[payload.Key] = PidDecoder.DecodeSupportedPids(payload.Value, 0x00);
            }

            //get PIDs 21-40 support
            result = PidDecoder.ParsePidCmd(_port.SendCommand(new ObdPid { Mode = "01", Pid = "20" }.PidCommand), _protocol);

            foreach (var payload in result)
            {
                supportedPids[payload.Key].AddRange(PidDecoder.DecodeSupportedPids(payload.Value, 0x20));
            }

            //get PIDs 21-40 support
            result = PidDecoder.ParsePidCmd(_port.SendCommand(new ObdPid { Mode = "01", Pid = "40" }.PidCommand), _protocol);
            foreach (var payload in result)
            {
                supportedPids[payload.Key].AddRange(PidDecoder.DecodeSupportedPids(payload.Value, 0x40));
            }

            _supportedPids = supportedPids;
        }

        private void GetPreferredPids()
        {
            var xpids = XDocument.Load("WellKnownPids.xml");
            var pids = xpids.Element("Document").Elements("MessageType");

            var pidsForMe = pids.Select(x => new ObdPid() { Mode = byte.Parse("01"), Name = x.Element("Name").Value, Pid = byte.Parse(x.Element("PID").Value) }).ToList();
            _desiredPids = pidsForMe;
        }
    }
}