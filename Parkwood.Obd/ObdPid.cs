using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Parkwood.Obd
{
    public class ObdPid
    {
               
        public string Name { get; set; }

        public string Mode { get; set; }

        public string Pid { get; set; }

        public string PidCommand => $"{Mode} {Pid}";

        public string ConversionFunctionName { get; set; }

        public string StandardValueType { get; set; }

    }

}