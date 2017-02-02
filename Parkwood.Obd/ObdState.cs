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
            Vin = "WAUFF-AUDI";
            Timestring = DateTime.UtcNow.ToString("O").Replace(":", string.Empty).Replace(".", string.Empty);
        }

        private string Vin { get; set; }

        public DateTime SnapshotTime { get; set; }

        public List<EcuResponse> _ecuResponses { get; set; }

        public string Timestring { get; set; }
    }
}
