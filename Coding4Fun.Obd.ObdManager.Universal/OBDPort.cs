namespace Coding4Fun.Obd.ObdManager.Universal
{
    public abstract class ObdPort : TelemetryBase
    {
        public virtual void Connect()
        {
            Connected = true;
        }

        public virtual void Disconnect()
        {
            Connected = false;
        }

        public Protocol Protocol { get; set; }

        public bool Connected { get; protected set; }

        public ObdResponse GetPidData(ObdRequest req)
        {
            return GetPidData(req.Mode, req.Pid);
        }

        protected abstract ObdResponse GetPidData(int mode, int pid);
    }

    public class TelemetryBase
    {
        
    }
}