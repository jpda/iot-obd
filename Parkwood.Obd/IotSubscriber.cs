using System;

namespace Parkwood.Obd
{
    public class IotSubscriber : StateSubscriber
    {


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
