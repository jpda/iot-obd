using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "JackPiDevice". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=Parkwood-Iot-Hub.azure-devices.net;DeviceId=JackPi;SharedAccessSignature=SharedAccessSignature sr=Parkwood-Iot-Hub.azure-devices.net%2fdevices%2fJackPi&sig=dmKmM%2fthvB6kKA3zcmYn8e5xbHNx6Lf%2b%2f2GpjR1GSBg%3d&se=1489271958";
    // To monitor messages sent to device "JackPiDevice" use iothub-explorer as follows:
    //    iothub-explorer HostName=Parkwood-Iot-Hub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=IUTsaqV29Vzbthv1XKnbQXOmlogcCef0roq2cztwjrE= monitor-events "JackPiDevice"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-cpp for more information on Microsoft Azure IoT Connected Service

    public static async Task SendDeviceToCloudMessageAsync(string message)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Http1);
        await deviceClient.OpenAsync();
        var enc_message = new Message(Encoding.ASCII.GetBytes(message));
        await deviceClient.SendEventAsync(enc_message);
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Http1);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            //  Note: In this sample, the polling interval is set to 
            //  10 seconds to enable you to see messages as they are sent.
            //  To enable an IoT solution to scale, you should extend this 
            //  interval. For example, to scale to 1 million devices, set 
            //  the polling interval to 25 minutes.
            //  For further information, see
            //  https://azure.microsoft.com/documentation/articles/iot-hub-devguide/#messaging
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
