using System;

namespace Parkwood.Obd
{
    public struct ObdPid
    {
        public byte[] RawData { get; set; }

        public string Name { get; set; }

        public string Mode { get; set; }

        public string Pid { get; set; }

        public string PidCommand => $"{Mode} {Pid}";

        public Func<byte[], string> Compute
        {
            get; set;
        }

        public string Value
        {
            get
            {
                return Compute(RawData);
            }
        }
    }
}