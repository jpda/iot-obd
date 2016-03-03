using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public abstract class ObdPort
    {
        public virtual void Connect()
        {
            Connected = true;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }
        public bool Connected { get; protected set; }
        
        public abstract string SendCommand(string cmd);

        public abstract Task<string> ReadResponse();
        
    }
}

