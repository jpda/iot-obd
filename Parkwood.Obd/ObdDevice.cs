using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdDevice : IObservable<ObdState>
    {

        private List<IObserver<ObdState>> observers;
        private ObdPort port;
        private bool publish;


        public ObdDevice(ObdPort p)
        {
            this.port = p;
            observers = new List<IObserver<ObdState>>();
            //begin publishing pid data
            Publish();
        }

        private void Publish()
        {
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
            foreach (var observer in observers)
            {
                //fill out the state object
                ObdState state = new ObdState(port);
                //publish state to each subscriber
                observer.OnNext(state);
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
    }
}
