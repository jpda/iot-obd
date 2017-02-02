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

        public override async Task Connect()
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
            await base.Connect();
        }

        public override string SendCommandWaitForString(string cmd)
        {
            var eol = ">";
            var writer = new DataWriter(_socket.OutputStream);
            writer.WriteString(cmd + "\r\n");
            var bytesWritten = Task.Run(async () => await writer.StoreAsync()).Result;
            Logger.DebugWrite($"Wrote {bytesWritten} bytes: {cmd}");
            ////todo: Do we need this delay?
            //Task.Delay(500);
            var done = false;
            var data = string.Empty;
            while (!done)
            {
                var response = Task.Run(async () => await ReadString()).Result;
                data = data + response;
                done = response.Contains(eol);
            }
            writer.DetachStream();
            return data;
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

        //public override async Task<string> ReadString()
        //{
        //    return Encoding.ASCII.GetString(await ReadBytes());
        //}

        public override async Task<byte[]> ReadBytes()
        {
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

        public override async Task<string> ReadString()
        {
            uint readBufferLength = 1024;
            var reader = new DataReader(_socket.InputStream) { InputStreamOptions = InputStreamOptions.Partial };
            var loadAsyncTask = reader.LoadAsync(readBufferLength).AsTask();

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    string recvdtxt = reader.ReadString(bytesRead);
                    System.Diagnostics.Debug.WriteLine(recvdtxt);
                    return recvdtxt;
                }
                catch (Exception ex)
                {
                    Logger.DebugWrite("ReadAsync: " + ex.Message);
                }
            }
            return "";
        }

        private async Task ConnectDeviceAsync(DeviceInformation pairedDevice)
        {
            try
            {
                _service = await RfcommDeviceService.FromIdAsync(pairedDevice.Id);

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