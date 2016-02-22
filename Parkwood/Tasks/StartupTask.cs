﻿using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using System.Threading;
using Coding4Fun.Obd.ObdManager.Universal;
using Coding4Fun.Obd.ObdManager.Universal.Bluetooth;

namespace Parkwood.Tasks
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
            // prevent the task from closing prematurely by using BackgroundTaskDeferral as
            // described in http://aka.ms/backgroundtaskdeferral
            // Create the deferral by requesting it from the task instance.
            var deferral = taskInstance.GetDeferral();

            var port = new ObdBluetoothPort();
            
            var obd = new ObdDevice();
            obd.Connect(port);

            obd.ObdConnectionChanged += (sender, args) =>
            {
                //include data in args? prob
                obd.GetCurrentState();
                //persist to wherever
            };

            while (true)
            {
                //keep this method open
            }
            
            //
            // Call asynchronous method(s) using the await keyword.
            //
            ////ObdDevice od = new ObdDevice();
            ////OBDSerialPort osp = new OBDSerialPort();
            //////fill in parameters for serial port

            ////od.Connect(obdp);
            ////get the state
            //od.GetCurrentState().ToJson();

            //
            // Once the asynchronous method(s) are done, close the deferral.
            //
            deferral.Complete();
        }
    }
}
