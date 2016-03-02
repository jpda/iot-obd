using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public abstract class BasePid
    {
        public byte[] Raw { get; set; }
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
        public abstract object GetComputedValue();

    }
}
