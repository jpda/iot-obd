using System;
using System.Diagnostics;
using Parkwood.Obd;

namespace Parkwood.Test
{
    internal class TestSubscriber : StateSubscriber
    {
        public string State { get; set; }

        public override void OnNext(ObdState value)
        {
            State = value.ToJson();
            Debug.WriteLine(value.ToJson());
        }
    }
}