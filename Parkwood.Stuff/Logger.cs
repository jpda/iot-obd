using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Parkwood.Stuff
{
    /// <summary>
    /// Should eventually replace with AppInsights
    /// </summary>
    public static class Logger
    {
        public static void DebugWrite(string message, [CallerMemberName] string caller = "", [CallerLineNumber] int lineNumber = 0)
        {
            Debug.WriteLine($"{caller}:{lineNumber}: {message}");
        }
    }
}
