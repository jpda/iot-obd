namespace Coding4Fun.Obd.ObdManager
{
	public class ObdHelpers
	{
		public static int KilometersPerHourToMilesPerHour(int kph)
		{
			return (int)(kph * 0.621371192);
		}

		public static int MilesPerHourToKilometersPerHour(int mph)
		{
			return (int)(mph * 1.609344);             
		}

		public static double GetMilesPerGallon(int kph, double massAirflow)
		{
			return 710.7 * kph / massAirflow;
		}

		public static double GetKilometersPerGallon(int kph, double massAirflow)
		{
			// this may not be right...
			return 11.4382 * kph / massAirflow;
		}
	}
}
