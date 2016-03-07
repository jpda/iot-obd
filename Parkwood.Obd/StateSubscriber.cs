using System;
using Parkwood.Stuff;

namespace Parkwood.Obd
{
    public abstract class StateSubscriber : IObserver<State>
    {
        public virtual void OnCompleted()
        {
            Logger.DebugWrite("Subscriber completed");
        }

        public virtual void OnError(Exception error)
        {
            Logger.DebugWrite($"Subscriber error: {error.Message}");
        }

        public abstract void OnNext(State value);
    }
}
