using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkwood.Odb
{
    public abstract class ObdPort
    {
        private readonly Dictionary<int, List<int>> _supportedPids = new Dictionary<int, List<int>>();

        public virtual void Connect()
        {
            Connected = true;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }
        public bool Connected { get; protected set; }
        
        public abstract void SendCommand(string cmd);

        public abstract Task<string> ReadResponse();
        
    }
}