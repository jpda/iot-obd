using Windows.ApplicationModel.Background;
using System.Threading;
using Parkwood.Obd;
using Parkwood.Obd.Port;

namespace Parkwood.Tasks
{
    public sealed class StartupTask : IBackgroundTask
    {
        private CancellationTokenSource ReadCancellationTokenSource;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            while (true)
            {
                var btp = new ObdBluetoothPort("OBDLink MX");
                var provider = new ObdDevice(btp);
                var debug = new DebugSubscriber();

                provider.Subscribe(debug);
                provider.Startup();
            }

            deferral.Complete();
        }

    }
}
