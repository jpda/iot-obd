using Windows.ApplicationModel.Background;
using System.Threading;
using Coding4Fun.Obd.ObdManager.Universal;
using Coding4Fun.Obd.ObdManager.Universal.Bluetooth;
using Parkwood.Configuration;

namespace Parkwood.Tasks
{
    public sealed class StartupTask : IBackgroundTask
    {
        private CancellationTokenSource ReadCancellationTokenSource;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //hack to find any device with OBD in the name if nothing exists in settings
            var deviceName = ConfigurationManager.Get("TargetDeviceName");
            deviceName = deviceName == string.Empty ? "OBD" : ConfigurationManager.Get("TargetDeviceName");

            //should figure out what to do with this deferral
            var deferral = taskInstance.GetDeferral();
            var device = GetDevice(deviceName);

            while (true)
            {
                if (device == null)
                {
                    device = GetDevice(deviceName);
                    device.ObdConnectionChanged += (sender, args) =>
                    {
                        // save device name for 'last known?'
                    };
                }

                //get stats
                device.GetCurrentState();
                //notify subscribers, e.g., event hub, local trace, etc

            }

            deferral.Complete();
        }

        private static ObdDevice GetDevice(string deviceName)
        {
            var port = new ObdBluetoothPort(deviceName);

            var obd = new ObdDevice();
            obd.Connect(port);
            return obd;
        }
    }
}
