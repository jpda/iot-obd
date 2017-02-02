using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Parkwood.Obd
{
    public class EcuResponse
    {
        public ObdPid Pid { get; set; }
        public EcuResponse(ObdPid pid)
        {
            Pid = pid;
        }
        public DateTime RawValueSetTime;
        private byte[] RawData;
        public string ECU { get; set; }
        public string ComputedStandardValue
        {
            get
            {
                var conversions = typeof(PidDecoder).GetMethods().Where(x => x.Name == this.Pid.ConversionFunctionName).ToList();
                if (!conversions.Any() || conversions.Count > 1) { return Encoding.ASCII.GetString(RawData); }
                var conversion = conversions.Single();
                var result = conversion.Invoke(null, new object[] { RawData }).ToString();
                return result;
            }
        }

        public string RawStringData;

        /// <summary>
        /// Null when the pid has not been requested from the computer. 
        /// </summary>
        public byte[] ReturnedRawData
        {
            get
            {
                return RawData;
            }
            set
            {

                RawData = value;
                RawValueSetTime = DateTime.Now;
            }
        }
    }
}
