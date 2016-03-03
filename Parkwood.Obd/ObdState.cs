using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdState: Dictionary<string, object>, IDisposable
    {
        ObdPort port;

        public string VIN { get; private set; }
        public DateTime Generation = DateTime.Now;
        public ObdState(ObdPort port)
        {
            this.port = port;             
        }

        public void GetCurrentState()
        {
            //ask for vin
            //ask for supported pids. 
            //add all to state dictionary
            //decode known pids

        }


        public void Dispose()
        {
            //nothing to do here yet. 
        }
    }
}
