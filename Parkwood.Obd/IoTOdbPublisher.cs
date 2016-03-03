using System;

namespace Parkwood.Obd
{

    public class IoTOdbPublisher : StatePublisher
    {
        private IDisposable _unsubscriber;

        public IoTOdbPublisher(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

       
    }

    public class DebugPublisher : StatePublisher
    {
        
    }

    public abstract class StatePublisher : IObservable<State>
    {
        public IDisposable Subscribe(IObserver<State> observer)
        {
            throw new NotImplementedException();
        }
    }
}
