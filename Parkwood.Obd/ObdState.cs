using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdState: IDisposable
    {
        

        public string VIN { get; private set; }
        public DateTime GenDateTime = DateTime.Now;
        public Dictionary<string, ObdPid> PidData = new Dictionary<string, ObdPid>();

        public void Dispose()
        {
            //nothing to do. 
        }
        
    }
}
