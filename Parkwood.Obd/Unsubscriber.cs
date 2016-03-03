using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    //used to remove and dispose subscribers during and unsubscribe call
    internal class Unsubscriber : IDisposable
    {
        private List<IObserver<ObdState>> _observers;
        private IObserver<ObdState> _observer;

        public Unsubscriber(List<IObserver<ObdState>> observers, IObserver<ObdState> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
