using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {
        public override async void OnNext(State value)
        {
                await AzureIoTHub.SendDeviceToCloudMessageAsync(value.ToJson());
            }
    }
}
