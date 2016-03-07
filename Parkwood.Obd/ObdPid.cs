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

    }

    public class ObdPidValue : ObdPid
    {
        public ObdPidValue() { }

        public ObdPidValue(ObdPid pid)
        {
            Name = pid.Name;
            Mode = pid.Mode;
            Pid = pid.Pid;
            ConversionFunctionName = pid.ConversionFunctionName;
        }

        public byte[] RawData { get; set; }

        public string Value
        {
            //todo: LOL this is ridiculous
            get
            {
                var conversions = typeof(PidDecoder).GetMethods(BindingFlags.Public).Where(x => x.Name == ConversionFunctionName).ToList();
                if (!conversions.Any() || conversions.Count > 1) { return Encoding.ASCII.GetString(RawData); }
                var conversion = conversions.Single();
                return conversion.Invoke(null, new object[] { RawData }) as string;
            }
        }
    }
}