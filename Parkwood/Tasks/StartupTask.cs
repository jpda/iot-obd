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
            //used to hold process after this thread has moved past the publisher
            var deferral = taskInstance.GetDeferral();

            //connect
            var btp = new ObdBluetoothPort("OBDLink MX");
            var provider = new ObdDevice(btp);

            var debug = new DebugSubscriber();

            var iot = new IotSubscriber();

            //subscribe
            provider.Subscribe(debug);
            provider.Subscribe(iot);

            provider.Startup();
        }

    }
}
