using Windows.ApplicationModel.Background;
using System.Threading;

using Parkwood.Configuration;
using Parkwood.Obd;

namespace Parkwood.Tasks
{
    public sealed class StartupTask : IBackgroundTask
    {
        private CancellationTokenSource ReadCancellationTokenSource;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            
            //should figure out what to do with this deferral
            var deferral = taskInstance.GetDeferral();


            ObdPort p = null;
            // Define a provider and two observers.
            ObdDevice provider = new ObdDevice(p);
            IoTOdbPublisher reporter1 = new IoTOdbPublisher("AzureIot");
            reporter1.Subscribe(provider);

            provider.EndTransmission();
            
            deferral.Complete();
        }

    }
}
