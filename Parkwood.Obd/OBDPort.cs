using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace Parkwood.Obd
{
    public abstract class ObdPort
    {
        public async virtual Task Connect()
        {
            Connected = true;
            return;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }
        public bool Connected { get; protected set; }

        public abstract string SendCommandWaitForString(string cmd);

        public abstract byte[] SendCommandWaitForBytes(string cmd);

        public abstract Task<string> ReadString();

        public abstract Task<byte[]> ReadBytes();
    }
}

