using Parkwood.Obd;
using Parkwood.Obd.Port;
using Parkwood.Stuff;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Parkwood.Foreground
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Startup();
        }

        public void Startup()
        {
            //todo: replace with confi setting builders: port type and ID data
            Logger.DebugWrite("Trying to connect...");
            var btp = new ObdBluetoothPort("Port");
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
