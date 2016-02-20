using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Enumeration;


namespace Parkwood
{
    public class PairedDeviceInfo
    {
        internal PairedDeviceInfo(DeviceInformation deviceInfo)
        {
            this.DeviceInfo = deviceInfo;
            this.ID = this.DeviceInfo.Id;
            this.Name = this.DeviceInfo.Name;
        }

        public string Name { get; private set; }
        public string ID { get; private set; }
        public DeviceInformation DeviceInfo { get; private set; }

        public sealed override string ToString()
        {
            return String.Format("{0} {1} {2}", this.DeviceInfo, this.ID, this.Name);

        }
    }
}
