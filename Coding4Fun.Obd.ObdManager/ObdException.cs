using System;

namespace Coding4Fun.Obd.ObdManager
{
	public class ObdException : Exception
	{
		public ObdException(string message) : base(message)
		{
		}
	}
}