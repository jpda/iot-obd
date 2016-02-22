using System;
using System.Collections.Generic;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public abstract class ObdPort
    {
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
        public ObdResponse GetPidData(ObdRequest req)
        {
            return GetPidData(req.Mode, req.Pid);
        }
        protected abstract ObdResponse GetPidData(Int32 mode, Int32 pid);
    }
}