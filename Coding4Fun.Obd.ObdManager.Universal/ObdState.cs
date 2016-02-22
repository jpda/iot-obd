using System.Text;
using System;

using System.Reflection;
using Coding4Fun.Obd.ObdManager.Universal;

namespace Coding4Fun.Obd.ObdManager
{
	public class ObdState
	{
		public int Rpm { get; set; }
		public int KilometersPerHour { get; set; }
		public int MilesPerHour { get; set; }
		public int ThrottlePosition { get; set; }
		public int EngineRuntime { get; set; }
		public int FuelType { get; set; }
		public int FuelLevel { get; set; }
		public int BarometricPressure { get; set; }
		public int EngineLoad { get; set; }
		public int EngineCoolantTemperature { get; set; }
		public int IntakeAirTemperature { get; set; }
		public int DistanceTraveledWithMilOn { get; set; }
		public int RelativeThrottlePosition { get; set; }
		public double MassAirflowRate { get; set; }
		public int OilTemperature { get; set; }
		public bool MilLightOn { get; set; }
		public double MilesPerGallon { get; set; }
		public double KilometersPerGallon { get; set; }


		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			Type thisClass = typeof(ObdState);

			foreach (System.Reflection.PropertyInfo info in thisClass.GetProperties())
				sb.AppendFormat("{0}: {1} \r\n", info.Name, info.GetValue(this, null));

			return sb.ToString();

		}
         
	}
}