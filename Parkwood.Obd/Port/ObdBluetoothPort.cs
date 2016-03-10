using System;
using System.Collections.Generic;
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

        public override string SendCommandWaitForString(string cmd)
        {
            return Encoding.ASCII.GetString(SendCommandWaitForBytes(cmd));
        }

        public override byte[] SendCommandWaitForBytes(string cmd)
        {
            var eol = Encoding.ASCII.GetBytes(">").Single();
            var writer = new DataWriter(_socket.OutputStream);
            writer.WriteString(cmd + "\r\n");
            var bytesWritten = Task.Run(async () => await writer.StoreAsync()).Result;
            Logger.DebugWrite($"Wrote {bytesWritten} bytes: {cmd}");
            ////todo: Do we need this delay?
            //Task.Delay(500);
            var data = new List<byte>();
            var done = false;
            while (!done)
            {
                var response = Task.Run(async () => await ReadBytes()).Result;
                data.AddRange(response);
                done = response.Contains(eol);
            }
            writer.DetachStream();
            return data.ToArray();
        }

        public override async Task<string> ReadString()
        {
            return Encoding.ASCII.GetString(await ReadBytes());
        }

        public override async Task<byte[]> ReadBytes()
        {
            //todo: would a response ever be more than 1kB? i don't think so, but know we should loop through the buffer if we get one larger. taking the assumption that won't happen.
            //http://www.elmelectronics.com/DSheets/ELM327DSF.pdf OBD buffer is 256 Byte RS232 Transmit buffer
            const uint readBufferLength = 1024;
            try
            {
                if (_socket.InputStream == null) return null;
                var reader = new DataReader(_socket.InputStream) { InputStreamOptions = InputStreamOptions.Partial };
                var bytes = new byte[await reader.LoadAsync(readBufferLength)];

                reader.ReadBytes(bytes);
                reader.DetachStream();
                return bytes;
            }
            catch (Exception ex)
            {
                Logger.DebugWrite("ReadAsync: " + ex.Message);
            }
            return null;
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