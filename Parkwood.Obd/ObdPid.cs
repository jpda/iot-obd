namespace Parkwood.Obd
{
    public struct ObdPid
    {
        public byte[] RawData { get; set; }

        public string Name { get; set; }

        public byte Mode { get; set; }

        public byte Pid { get; set; }

        public string PidCommand => $"{Mode} {Pid}";
    }
}