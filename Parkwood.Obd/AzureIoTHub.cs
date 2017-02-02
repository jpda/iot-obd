using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

public class AzureIoTHub
{
    DeviceClient _client;

    public AzureIoTHub(string host, string name, string key)
    {
        _client = DeviceClient.Create(host, new DeviceAuthenticationWithRegistrySymmetricKey(name, key), TransportType.Http1);
    }

    public async Task SendDeviceToCloudMessageAsync(string message)
    {
        var msg = Encoding.Convert(Encoding.ASCII, Encoding.UTF8, Encoding.ASCII.GetBytes(message));
        var enc_message = new Message(msg);
        await _client.SendEventAsync(enc_message);
    }
}