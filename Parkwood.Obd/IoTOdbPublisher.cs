using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{

    public class IoTOdbPublisher : IObserver<ObdState>
    {
        private IDisposable unsubscriber;
        private string instName;

        public IoTOdbPublisher(string name)
        {
            this.instName = name;
        }

        public string Name
        { get { return this.instName; } }

        public virtual void Subscribe(IObservable<ObdState> provider)
        {
            if (provider != null)
                unsubscriber = provider.Subscribe(this);
        }

        public virtual void OnCompleted()
        {
            this.Unsubscribe();
        }

        public virtual void OnError(Exception e)
        {
            //probably need to do more here. 
            Parkwood.Stuff.Logger.DebugWrite(e.Message);
        }

        public virtual void OnNext(ObdState value)
        {
            //send data to Azure
            Parkwood.Stuff.Logger.DebugWrite(value.ToJson());
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }
    }
}
