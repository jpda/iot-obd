using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class Rpm : BasePid
    {
        public override object GetComputedValue()
        {
            return 30000;
        }
    }
}
