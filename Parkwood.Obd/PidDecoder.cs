using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkwood.Obd
{
    public static class PidDecoder
    {
        public static int GetEngineLoad(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x04);
            return 100 * obdData[0] / 255;
        }

        public static int GetEngineCoolantTemperature(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x05);
            return obdData[0] - 40;
        }

        public static int GetRpm(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x0C);
            return (obdData[0] * 256 + obdData[1]) / 4;
        }

        public static int GetKilometersPerHour(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x0D);
            return obdData[0];
        }

        public static int GetIntakeAirTemperature(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x0F);
            return obdData[0] - 40;
        }

        public static double GetMassAirflowRate(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x10);
            return ((obdData[0] * 256) + obdData[1]) / 100.0;
        }

        public static int GetThrottlePosition(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x11);
            return 100 * obdData[0] / 255;
        }

        public static int GetRuntimeSinceEngineStart(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x1F);
            return obdData[0] * 256 + obdData[1];
        }

        public static int GetDistanceTraveledWithMilOn(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x21);
            return obdData[0] * 256 + obdData[1];
        }

        public static int GetFuelLevelInput(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x2F);
            return 100 * obdData[0] / 255;
        }

        public static int GetBarometricPressure(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x33);
            return obdData[0];
        }

        public static int GetRelativeThrottlePosition(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x45);
            return 100 * obdData[0] / 255;
        }

        public static int GetFuelType(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x51);
            return 100 * obdData[0] / 255; 
        }

        public static int GetOilTemperature(byte[] obdData)
        {
            //Mode and Pid Request(0x01, 0x5c);
            return obdData[0] - 40;
        }

    }
}
