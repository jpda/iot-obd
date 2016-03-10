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

            //todo: replace with confi setting builders: port type and ID data
            var btp = new ObdBluetoothPort("OBDLink MX");
            var provider = new ObdDevice(btp);

            var debug = new DebugSubscriber();
            //var iot = new IotSubscriber();

            //subscribe
            provider.Subscribe(debug);
            //stop killing my flow. This is so borked. I am just going to run this on premiscees
            //provider.Subscribe(iot);

            provider.Startup();
        }

    }
}
