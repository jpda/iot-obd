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

        public ObdDevice()
        {
            observers = new List<IObserver<ObdState>>();
            //begin publishing pid data
            Publish();
        }

        private void Publish()
        {
            //loop and publish state as often as possible
            while (true)
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
                //need to ask OBD for VIN
                
                //ask for vin
                //ask for specific pids. 
                //add all to state dictionary

                //fill out the state object
                ObdState state = new ObdState("");
                Dictionary<string, ObdPid> currentPids = new Dictionary<string, ObdPid>();

               
                state.Add(p.Name, p);
                       
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
            observers.Clear();
        }
    }
}
