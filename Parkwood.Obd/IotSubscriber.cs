using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        AzureIoTHub _hub;

        public IotSubscriber(string host, string name, string key) : base()
        {
            _hub = new AzureIoTHub(host, name, key);
        }

        public override async void OnNext(ObdState value)
        {
            var message = value.ToJson();
            await _hub.SendDeviceToCloudMessageAsync(message);
        }
    }
}
