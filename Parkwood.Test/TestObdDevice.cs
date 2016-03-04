using System;
using System.Collections.Generic;
using System.Linq;
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

    internal class TextObdPort : ObdPort
    {
        private readonly List<string> _lines;
        private readonly Dictionary<string, List<string>> _pidData = new Dictionary<string, List<string>>();

        public TextObdPort(IEnumerable<string> lines)
        {
            _lines = lines.ToList();
            ParseText();
        }

        private void ParseText()
        {
            var pids = _lines.Where(x => x.StartsWith("PID:")).Select(y => y.Substring(0, y.IndexOf(',')));
            var uniquePids = pids.Distinct();
            foreach (var p in uniquePids)
            {
                _pidData.Add(p, new List<string>());
            }

            foreach (var p in _lines)
            {
                
            }
        }

        public override string SendCommand(string cmd)
        {
            var command = cmd.Split(' ');
            var mode = command[0];
            var pid = command[1];
            var data = _pidData[pid];
            var r = new Random();
            return data[r.Next(data.Count - 1)]; //get a random data point
        }

        public override Task<string> ReadResponse()
        {
            throw new NotImplementedException();
        }
    }
}
