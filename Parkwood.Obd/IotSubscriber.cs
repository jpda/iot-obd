using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        public override async void OnNext(ObdState value)
        {
            var message = value.ToJson();
                await AzureIoTHub.SendDeviceToCloudMessageAsync(message);
            }
    }
}
