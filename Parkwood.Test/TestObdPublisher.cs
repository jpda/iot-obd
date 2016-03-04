using System;
using System.Diagnostics;
using Parkwood.Obd;

namespace Parkwood.Test
{
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