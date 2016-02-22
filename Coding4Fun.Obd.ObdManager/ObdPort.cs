using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4Fun.Obd.ObdManager
{
    public abstract class ObdPort
    {
        public void Connect()
        {
            this.Connected = true;
        }
        public virtual void  Disconnect()
        { 
            this.Connected = false;
        }
        public bool Connected { get; protected set; }
        public bool Poll { get; set;}
        public abstract void WriteLine(string line);
    }
}
