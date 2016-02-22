using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coding4Fun.Obd.ObdManager.Universal
{
    public struct ObdRequest
    {
        public ObdRequest(byte mode, byte pid)
        {
            this.Pid = pid;
            this.Mode = mode;
        }
        public byte Mode;
        public byte Pid; 
    }
}
