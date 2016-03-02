using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public class ObdException : Exception
    {
        public ObdException(string message) : base(message)
        {
        }
    }
}
