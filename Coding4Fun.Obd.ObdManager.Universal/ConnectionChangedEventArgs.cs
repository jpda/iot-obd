using System;

namespace Coding4Fun.Obd.ObdManager
{
	public class ConnectionChangedEventArgs : EventArgs
	{
		public bool Connected { get; set; }
	}
}