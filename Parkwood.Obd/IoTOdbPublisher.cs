﻿using System;
using Parkwood.Stuff;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        private IDisposable _unsubscriber;

        public IotSubscriber()
        {
           
        }
        
        public override void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(State value)
        {
            throw new NotImplementedException();
        }
    }

    public class DebugSubscriber : StateSubscriber
    {
        public override void OnCompleted()
        {
            Logger.DebugWrite("Subscriber completed");
        }

        public override void OnError(Exception error)
        {
            Logger.DebugWrite(error.Message);
        }

        public async override void OnNext(State value)
        {
            await AzureIoTHub.SendDeviceToCloudMessageAsync(value.ToJson());
        }
    }

    public abstract class StateSubscriber : IObserver<State>
    {
        public abstract void OnCompleted();

        public abstract void OnError(Exception error);

        public abstract void OnNext(State value);
    }
}
