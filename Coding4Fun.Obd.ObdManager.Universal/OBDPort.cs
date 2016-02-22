using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4Fun.Obd.ObdManager
{
    public abstract class ObdPort
    {
        public const int UnknownProtocol = -1;
        public string LastResponse { get; set; }
        public virtual void Connect()
        {
            this.Connected = true;
        }
        public virtual void Disconnect()
        {
            this.Connected = false;
        }
        public Protocol Protocol { get; set; }
        public bool Connected { get; protected set; }
        public bool Poll { get; set; }
        public abstract void WriteLine(string line);
    }
}
