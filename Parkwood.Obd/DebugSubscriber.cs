using Parkwood.Stuff;

namespace Parkwood.Obd
{
    public class DebugSubscriber : StateSubscriber
    {
        public override void OnNext(ObdState value)
        {
            Logger.DebugWrite(value.ToJson());
        }
    }
}