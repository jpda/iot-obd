using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Parkwood.Obd;

namespace Parkwood.Test
{
    internal class TestObdDevice : ObdDevice
    {
        public TestObdDevice(ObdPort p) : base(p)
        {

        }
    }

    internal class TestObdPort : ObdPort
    {
        public override string SendCommand(string cmd)
        {
            return string.IsNullOrEmpty(cmd) ? "hello" : $"Received:{cmd}";
        }

        public override Task<string> ReadResponse()
        {
            throw new NotImplementedException();
        }
    }

    internal class TestObdPublisher : IObserver<State>
    {
        public string State { get; set; }

        public void OnCompleted()
        {
            Debug.WriteLine("oncompleted");
        }

        public void OnError(Exception error)
        {
            Debug.WriteLine(error.Message);
        }

        public void OnNext(State value)
        {
            State = value.ToJson();
            Debug.WriteLine(value.ToJson());
        }
    }
}
