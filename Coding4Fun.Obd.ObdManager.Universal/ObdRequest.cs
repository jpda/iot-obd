namespace Coding4Fun.Obd.ObdManager.Universal
{
    public struct ObdRequest
    {
        public ObdRequest(int mode, int pid)
        {
            Pid = pid;
            Mode = mode;
        }
        public int Mode;
        public int Pid; 
    }
}
