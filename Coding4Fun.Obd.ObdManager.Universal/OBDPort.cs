using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public abstract class ObdPort
    {
        public ObdResponse LastResponse { get; set; }
        public ObdRequest LastRequest { get; set; }
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
        public abstract ObdResponse RequestPid(ObdRequest req);
    }
}
