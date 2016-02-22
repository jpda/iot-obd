using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public class ObdUwpBluetooth: ObdPort
    {
        async void InitializeRfcommDeviceService()
        {
            try
            {
                DeviceInformationCollection DeviceInfoCollection = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));


                var numDevices = DeviceInfoCollection.Count();

                // By clearing the backing data, we are effectively clearing the ListBox
                _pairedDevices = new ObservableCollection<PairedDeviceInfo>();
                _pairedDevices.Clear();

                if (numDevices == 0)
                {
                    System.Diagnostics.Debug.WriteLine("InitializeRfcommDeviceService: No paired devices found.");
                }
                else
                {
                    // Found paired devices.
                    foreach (var deviceInfo in DeviceInfoCollection)
                    {
                        PairedDeviceInfo pdi = new PairedDeviceInfo(deviceInfo);
                        _pairedDevices.Add(pdi);
                        System.Diagnostics.Debug.WriteLine(pdi.ToString());
                        //find the correct OBD Sensor
                        if (pdi.ToString().Contains("OBDLink MX"))
                        {
                            ConnectDevice(pdi);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InitializeRfcommDeviceService: " + ex.Message);
            }
        }
        async void ConnectDevice(PairedDeviceInfo pairedDevice)
        {
            DeviceInformation DeviceInfo;
            DeviceInfo = pairedDevice.DeviceInfo;

            bool success = true;
            try
            {
                //_service = await RfcommDeviceService.FromIdAsync(DeviceInfo.Id);
                _service = Task.Run<RfcommDeviceService>(async () =>
                {
                    return await RfcommDeviceService.FromIdAsync(DeviceInfo.Id);
                }).Result;



                if (_socket != null)
                {
                    // Disposing the socket with close it and release all resources associated with the socket
                    _socket.Dispose();
                }

                _socket = new StreamSocket();
                try
                {
                    // Note: If either parameter is null or empty, the call will throw an exception
                    Task.Run(async () => {
                        await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
                    }).Wait();
                    //await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
                }
                catch (Exception ex)
                {
                    success = false;
                    System.Diagnostics.Debug.WriteLine("Connect:" + ex.Message);
                }
                // If the connection was successful, the RemoteAddress field will be populated
                if (success)
                {
                    string msg = String.Format("Connected to {0}!", _socket.Information.RemoteAddress.DisplayName);
                    System.Diagnostics.Debug.WriteLine(msg);

                    //send a message
                    Send("ATZ");
                    Listen();
                    //Send("ATE0");
                    //Send("ATL0");
                    //Send("ATH1");
                    //Send("ATSP 5");
                    //Send(GetMessage());


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Overall Connect: " + ex.Message);
                _socket.Dispose();
                _socket = null;
            }
        }
        public async void Send(string msg)
        {
            try
            {
                // Create the DataWriter object and attach to OutputStream
                dataWriterObject = new DataWriter(_socket.OutputStream);

                //Launch the WriteAsync task to perform the write
                await WriteAsync(msg + "\r\n");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Send(): " + ex.Message);
            }
            finally
            {
                // Cleanup once complete
                if (dataWriterObject != null)
                {
                    dataWriterObject.DetachStream();
                    dataWriterObject = null;
                }
            }
        }
        public string GetMessage()
        {
            byte mode, pid;
            mode = 0x01;
            pid = 0x0C;
            Debug.Write("PID: " + pid.ToString("X2") + "RPM");
            //ask for RPMs right now as a test. 
            return mode.ToString("X2") + pid.ToString("X2");

        }
        private async Task WriteAsync(string msg)
        {
            Task<UInt32> storeAsyncTask;

            // Load the text from the sendText input text box to the dataWriter object
            dataWriterObject.WriteString(msg);

            // Launch an async task to complete the write operation
            storeAsyncTask = dataWriterObject.StoreAsync().AsTask();

            //see if the message wa sent
            UInt32 bytesWritten = await storeAsyncTask;
            if (bytesWritten > 0)
            {
                string status_Text = msg + ", ";
                status_Text += bytesWritten.ToString();
                status_Text += " bytes written successfully!";
                System.Diagnostics.Debug.WriteLine(status_Text);
            }
            Listen();
        }

        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                ReadCancellationTokenSource = new CancellationTokenSource();
                if (_socket.InputStream != null)
                {
                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    System.Diagnostics.Debug.WriteLine("Listen: Reading task was cancelled, closing device and cleaning up");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Listen: " + ex.Message);
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();
            dataReaderObject = new DataReader(_socket.InputStream);
            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    string recvdtxt = dataReaderObject.ReadString(bytesRead);
                    System.Diagnostics.Debug.WriteLine(recvdtxt);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("ReadAsync: " + ex.Message);
                }

            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        public override ObdResponse RequestPid(ObdRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
