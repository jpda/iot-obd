﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdState: Dictionary<string, object>, IDisposable
    {
        
        public ObdState(string vin, object port)
        {
            this.VIN = vin;
            
        }

        public GetSupportedPids()
        {

        }

        public string VIN { get; private set; }
        public DateTime Generation = DateTime.Now;

        public void Dispose()
        {
            //nothing to do here yet. 
        }
    }
}
