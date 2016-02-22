using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;

namespace Coding4Fun.Obd.ObdManager.Universal.Bluetooth
{
    public class ObdBluetoothPort : ObdPort
    {
        private readonly string _deviceName;
        private RfcommDeviceService _service;
        private StreamSocket _socket;

        public ObdBluetoothPort(string deviceName)
        {
            _deviceName = deviceName;
        }

        public override async void Connect()
        {
            try
            {
                var devices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
                var device = devices.Where(x => x.Name.ToLower().Contains(_deviceName.ToLower())).ToList();

                if (!device.Any())
                {
                    Debug.WriteLine("No devices found");
                }

                await ConnectDeviceAsync(device.Single());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeRfcommDeviceService: " + ex.Message);
            }
            base.Connect();
        }

        private async Task ConnectDeviceAsync(DeviceInformation pairedDevice)
        {
            var success = true;
            try
            {
                _service = Task.Run(async () => await RfcommDeviceService.FromIdAsync(pairedDevice.Id)).Result;

                //todo: dispose in case it exists? not sure, really
                _socket?.Dispose();
                _socket = new StreamSocket();

                try
                {
                    await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
                }
                catch (Exception ex)
                {
                    success = false;
                    System.Diagnostics.Debug.WriteLine("Connect:" + ex.Message);
                }

                if (success)
                {
                    var msg = $"Connected to {_socket.Information.RemoteAddress.DisplayName}!";
                    System.Diagnostics.Debug.WriteLine(msg);
                    InitializeDevice();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Overall Connect: " + ex.Message);
                _socket.Dispose();
                _socket = null;
            }
        }

        private async void InitializeDevice()
        {
            //send a message
            //Send("ATZ");
            //Listen();
            //Send("ATE0");
            //Send("ATL0");
            //Send("ATH1");
            //Send("ATSP 5");
            //Send(GetMessage());
            Connected = true;
        }

        protected override ObdResponse GetPidData(int mode, int pid)
        {
            //do socket ops
            throw new NotImplementedException();
        }
    }
}
