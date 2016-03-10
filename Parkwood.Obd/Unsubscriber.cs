using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    internal class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<ObdState>> _observers;
        private readonly IObserver<ObdState> _observer;

        public Unsubscriber(List<IObserver<ObdState>> observers, IObserver<ObdState> observer)
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
