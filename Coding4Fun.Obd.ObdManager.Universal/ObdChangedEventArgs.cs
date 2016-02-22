using System;

namespace Coding4Fun.Obd.ObdManager
{
	public class ObdChangedEventArgs : EventArgs
	{
		public ObdState ObdState { get; set; }

		public ObdChangedEventArgs(ObdState state)
		{
			ObdState = state;
		}
	}
}