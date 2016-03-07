using System;

namespace Parkwood.Obd
{
    public class ObdPid
    {
        public string Name { get; set; }

        public string Mode { get; set; }

        public string Pid { get; set; }

        public string PidCommand => $"{Mode} {Pid}";

        public string Formula { get; set; }

    }

    public class ObdPidValue : ObdPid
    {
        public ObdPidValue() { }

        public ObdPidValue(ObdPid pid)
        {
            Name = pid.Name;
            Mode = pid.Mode;
            Pid = pid.Pid;
            Formula = pid.Formula;
        }

        public byte[] RawData { get; set; }
    }
}