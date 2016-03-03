using System;
using System.Collections.Generic;

namespace Parkwood.Obd
{
    public struct State
    {
        public State(IDictionary<string, object> pidVals)
        {
            PidValues = pidVals; //should probably do something here to keep this readonly outside of here. like a view wrapper but whatever
            SnapshotTime = DateTime.UtcNow;
            Vin = ""; //get from PidValues?
        }
        private string Vin { get; set; }

        public DateTime SnapshotTime { get; set; }

        public IDictionary<string, object> PidValues { get; set; }
    }
}