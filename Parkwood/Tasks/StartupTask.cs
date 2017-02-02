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
            var btp = new ObdBluetoothPort("OBDII");
            var provider = new ObdDevice(btp);

            var debug = new DebugSubscriber();
            var iot = new IotSubscriber("iot-obd.azure-devices.net", "rpi3audi", "v2mjgQbzYCc0vuImro+rMDl0DieCFx0Hc0CdKEY+dUY=");

            //subscribe
            provider.Subscribe(debug);
            //stop killing my flow. This is so borked. I am just going to run this on premiscees
            provider.Subscribe(iot);

            provider.Startup();
        }
    }
}