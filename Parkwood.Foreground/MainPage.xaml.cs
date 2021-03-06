﻿using Parkwood.Obd;
using Parkwood.Obd.Port;
using Parkwood.Stuff;
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
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
            var devices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
            var device = devices[0].Id;
            var btd = await BluetoothDevice.FromIdAsync(device);
            await btd.RequestAccessAsync();

            var btp = new ObdBluetoothPort("Port");
            var provider = new ObdDevice(btp);
            await provider.Connect();

            var debug = new DebugSubscriber();
            var iot = new IotSubscriber("<YOUR IOT HUB NAME>", "<YOUR DEVICE>", "<YOUR DEVICE KEY>");

            provider.Subscribe(debug);
            provider.Subscribe(iot);
            provider.Startup();
        }
    }
}
