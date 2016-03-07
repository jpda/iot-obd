using Parkwood.Stuff;

namespace Parkwood.Obd
{
    public class DebugSubscriber : StateSubscriber
    {
        public override void OnNext(State value)
        {
            Logger.DebugWrite(value.ToJson());
        }
    }
}