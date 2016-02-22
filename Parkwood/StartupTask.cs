using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;

using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using Coding4Fun.Obd.ObdManager.Universal;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Parkwood
{
    public sealed class StartupTask : IBackgroundTask
    {
        ObservableCollection<PairedDeviceInfo> _pairedDevices;
        private Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService _service;
        private StreamSocket _socket;
        private DataWriter dataWriterObject;
        private DataReader dataReaderObject;
        private CancellationTokenSource ReadCancellationTokenSource;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //prevent the task from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            // Create the deferral by requesting it from the task instance.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            //
            // Call asynchronous method(s) using the await keyword.
            //
            ObdState ostate = new ObdState();
            ostate.ToJson();


            //
            // Once the asynchronous method(s) are done, close the deferral.
            //
            deferral.Complete();
        }


    }
}
