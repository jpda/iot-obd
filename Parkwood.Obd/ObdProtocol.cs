using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public enum Protocol
    {
        Unknown = -1,
        ElmAutomatic = 0,
        SaeJ1850Pwm = 1,
        SaeJ1850Vpw = 2,
        Iso91412= 3,
        Iso142304Kwp5Baud104Kbaud = 4,
        Iso142304KwpFast104Kbaud=5,
        Iso157654Can11Bit500Kbaud =6,
        Iso157654Can29Bit500Kbaud=7,
        Iso157654Can11Bit250Kbaud= 8,
        Iso157654Can29Bit250Kbaud= 9
    }
}
