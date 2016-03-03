using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Parkwood.Stuff;

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
                    Logger.DebugWrite("No devices found");
                    return;
                }

                await ConnectDeviceAsync(device.Single());
            }
            catch (Exception ex)
            {
                Logger.DebugWrite(ex.Message);
            }
            base.Connect();
        }

        public override async void SendCommand(string cmd)
        {
            var writer = new DataWriter(_socket.OutputStream);
            writer.WriteString(cmd);
            var writeTask = writer.StoreAsync().AsTask();
            var bytesWritten = await writeTask;
            Logger.DebugWrite($"Wrote {bytesWritten} bytes.");
        }

        public override async Task<string> ReadResponse()
        {
            try
            {
                var cancellationToken = new CancellationTokenSource();
                if (_socket.InputStream != null)
                {
                    while (true)
                    {
                        var response = await ReadAsync(cancellationToken.Token);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    Logger.DebugWrite("Listen: Reading task was cancelled, closing device and cleaning up");
                }
                else
                {
                    Logger.DebugWrite("Listen: " + ex.Message);
                }
            }
            return string.Empty;
        }

        private async Task<string> ReadAsync(CancellationToken token)
        {
            const uint readBufferLength = 1024;

            token.ThrowIfCancellationRequested();
            var reader = new DataReader(_socket.InputStream) { InputStreamOptions = InputStreamOptions.Partial };
            var loadAsyncTask = reader.LoadAsync(readBufferLength).AsTask(token);
            var bytesRead = await loadAsyncTask;

            if (bytesRead <= 0) return string.Empty;

            try
            {
                return reader.ReadString(bytesRead);
            }
            catch (Exception ex)
            {
                Logger.DebugWrite("ReadAsync: " + ex.Message);
            }

            return string.Empty;
        }

        private async Task ConnectDeviceAsync(DeviceInformation pairedDevice)
        {
            var success = false;
            try
            {
                _service = Task.Run(async () => await RfcommDeviceService.FromIdAsync(pairedDevice.Id)).Result;

                //todo: dispose in case it exists? not sure, really
                _socket?.Dispose();
                _socket = new StreamSocket();

                try
                {
                    await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
                    success = true;
                }
                catch (Exception ex)
                {
                    success = false;
                    Logger.DebugWrite("Connect:" + ex.Message);
                }

                if (success)
                {
                    var msg = $"Connected to {_socket.Information.RemoteAddress.DisplayName}!";
                    Logger.DebugWrite(msg);
                    InitializeDevice();
                }
            }
            catch (Exception ex)
            {
                Logger.DebugWrite("Overall Connect: " + ex.Message);
                _socket?.Dispose();
                _socket = null;
            }
        }

        private void InitializeDevice()
        {
            new List<string>() { "ATZ", "ATE0", "ATL0", "ATH1", "ATSP 5" }.ForEach(SendCommand);
            Connected = true;
        }

        public override ObdResponse GetPidData(int mode, int pid)
        {
            
            throw new NotImplementedException();
        }
    }
}
