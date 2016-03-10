using System.Collections.Generic;

namespace Parkwood.Obd
{
    public struct ObdEcu
    {
        public int Id { get; set; }

        public List<ObdPid> SupportedPids { get; set; }
        
    }
}