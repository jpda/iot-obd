﻿using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        private IDisposable _unsubscriber;

        public IotSubscriber()
        {
           
        }

        public string Name { get; }
        public override void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override async void OnNext(State value)
        {
            try
            {
                await AzureIoTHub.SendDeviceToCloudMessageAsync(value.ToJson());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Message:" + e.Message);
            }
           
        }
    }
}
