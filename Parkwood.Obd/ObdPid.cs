using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdPid
    {
        public ObdPid() { }
        public ObdPid(byte mode, byte pid) {
            this.Mode = mode;
            this.Pid = pid;
        }
        public byte[] RawData { get; set; }
        public string Name { get; set; }
        public byte Mode { get; set; }
        public byte Pid { get; set; }
        public string PidCommand
        {
            get
            {
                return $"{this.Mode} {this.Pid}";
            }
                
        }
    }
}
