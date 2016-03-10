using System;
using System.Collections.Generic;

namespace Parkwood.Obd
{
    public struct ObdState
    {
        public ObdState(List<EcuResponse> ecuResponses)
        {
            _ecuResponses = ecuResponses; //should probably do something here to keep this readonly outside of here. like a view wrapper but whatever
            SnapshotTime = DateTime.UtcNow;
            //todo:need to set Vin..
            Vin = "";
        }

        private string Vin { get; set; }

        public DateTime SnapshotTime { get; set; }

        public List<EcuResponse> _ecuResponses { get; set; }
    }
}
