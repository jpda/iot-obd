using Windows.ApplicationModel.Background;
using System.Threading;
using Parkwood.Obd;
using Parkwood.Obd.Port;
using Parkwood.Stuff;

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
            Logger.DebugWrite("Trying to connect...");
            var btp = new ObdBluetoothPort("Port");
            var provider = new ObdDevice(btp);

            var debug = new DebugSubscriber();
            var iot = new IotSubscriber("<YOUR IOT HUB>", "<IOT HUB DEVICE NAME>", "<HUB DEVICE KEY>");

            provider.Subscribe(debug);
            provider.Subscribe(iot);
            provider.Startup();
        }
    }
}
