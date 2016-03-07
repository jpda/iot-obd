using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Parkwood.Stuff;
using System.Text;

namespace Parkwood.Obd.Port
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

        public override void Connect()
        {
            try
            {
                var devices = Task.Run(async () => await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort))).Result;
                var device = devices.Where(x => x.Name.ToLower().Contains(_deviceName.ToLower())).ToList();

                if (!device.Any())
                {
                    Logger.DebugWrite("No devices found");
                    return;
                }

                Task.Run(async () => await ConnectDeviceAsync(device.Single())).Wait();
            }
            catch (Exception ex)
            {
                Logger.DebugWrite(ex.Message);
            }
            base.Connect();
        }

        public override string SendCommand(string cmd)
        {
            var writer = new DataWriter(_socket.OutputStream);
            writer.WriteString(cmd + "\r\n");
            var bytesWritten = Task.Run(async () => await writer.StoreAsync()).Result;
            Logger.DebugWrite($"Wrote {bytesWritten} bytes: {cmd}");
            Task.Delay(500);
            var data = new StringBuilder();
            var done = false;
            while (!done)
            {
                var response = Task.Run(async () => await ReadResponse()).Result;
                data.Append(response);
                done = response.Contains(">");
            }
            writer.DetachStream();
            return data.ToString();
        }

        public override async Task<string> ReadResponse()
        {
            try
            {
                if (_socket.InputStream == null) return string.Empty;
                const uint readBufferLength = 1024; //todo: would a response ever be more than 1kB? i don't think so, but know we should loop through the buffer if we get one larger. taking the assumption that won't happen.
                var reader = new DataReader(_socket.InputStream) { InputStreamOptions = InputStreamOptions.Partial };
                var bytesRead = await reader.LoadAsync(readBufferLength);

                var response = reader.ReadString(bytesRead);
                Logger.DebugWrite(response);
                reader.DetachStream();
                return response;
            }
            catch (Exception ex)
            {
                Logger.DebugWrite("ReadAsync: " + ex.Message);
            }
            //reader.Dispose();
            return string.Empty;
        }

        private async Task ConnectDeviceAsync(DeviceInformation pairedDevice)
        {
            try
            {
                _service = Task.Run(async () => await RfcommDeviceService.FromIdAsync(pairedDevice.Id)).Result;

                //todo: dispose in case it exists? not sure, really
                _socket?.Dispose();
                _socket = new StreamSocket();

                var success = false;
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

                }
            }
            catch (Exception ex)
            {
                Logger.DebugWrite("Overall Connect: " + ex.Message);
                _socket?.Dispose();
                _socket = null;
            }
        }
    }
}