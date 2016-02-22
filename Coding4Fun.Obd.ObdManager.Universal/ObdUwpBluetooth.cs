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
    class ObdUwpBluetooth: ObdPort
    {
        protected override ObdResponse GetPidData(int mode, int pid)
        {
            throw new NotImplementedException();
        }
        
    }
}
