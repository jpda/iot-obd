using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    internal class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<State>> _observers;
        private readonly IObserver<State> _observer;

        public Unsubscriber(List<IObserver<State>> observers, IObserver<State> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
