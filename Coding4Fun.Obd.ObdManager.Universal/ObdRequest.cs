namespace Coding4Fun.Obd.ObdManager.Universal
{
    public struct ObdRequest
    {
        public ObdRequest(byte mode, byte pid)
        {
            Pid = pid;
            Mode = mode;
        }
        public byte Mode;
        public byte Pid; 
    }
}
