using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        private IDisposable _unsubscriber;

        public IotSubscriber(string name)
        {
            this.Name = name;
            //init to Azure Hub
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
            await AzureIoTHub.SendDeviceToCloudMessageAsync(value.ToJson());
        }
    }
}