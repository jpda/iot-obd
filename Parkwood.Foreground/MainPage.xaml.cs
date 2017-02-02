using Parkwood.Obd;
using Parkwood.Obd.Port;
using Parkwood.Stuff;
using System;
using Windows.UI.Xaml.Controls;

namespace Parkwood.Foreground
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Startup();
        }

        public async void Startup()
        {
            //todo: replace with confi setting builders: port type and ID data
            Logger.DebugWrite("Trying to connect...");
            var btp = new ObdBluetoothPort("Port");
            var provider = new ObdDevice(btp);
            await provider.Connect();

            var debug = new DebugSubscriber();
            var iot = new IotSubscriber("iot-obd.azure-devices.net", "rpi3audi", "v2mjgQbzYCc0vuImro+rMDl0DieCFx0Hc0CdKEY+dUY=");

            provider.Subscribe(debug);
            provider.Subscribe(iot);
            provider.Startup();
        }
    }
}