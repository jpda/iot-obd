using System;
using System.Collections.Generic;

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

        public static Dictionary<int, byte[]> ParsePidCmd(string commandResult, Protocol protocol)
        {
            var payload = new Dictionary<int, byte[]>();
            if (commandResult.Contains("NO DATA") || commandResult.Contains("ERROR"))
            {
                return null;
            }

            var ecuResponses = commandResult.Trim().Split('\r');

            foreach (var ecuResponse in ecuResponses)
            {
                //todo: what are these for? we need to get the decoding offsets from the original project, I think
                const int offset = 5;
                const int ecuByte = 3;

                switch (protocol)
                {
                    case Protocol.Unknown:
                        break;
                    case Protocol.ElmAutomatic:
                        break;
                    case Protocol.SaeJ1850Pwm:
                        break;
                    case Protocol.SaeJ1850Vpw:
                        break;
                    case Protocol.Iso91412:
                        break;
                    case Protocol.Iso142304Kwp5Baud104Kbaud:
                        break;
                    case Protocol.Iso142304KwpFast104Kbaud:
                        break;
                    case Protocol.Iso157654Can11Bit500Kbaud:
                        break;
                    case Protocol.Iso157654Can29Bit500Kbaud:
                        break;
                    case Protocol.Iso157654Can11Bit250Kbaud:
                        break;
                    case Protocol.Iso157654Can29Bit250Kbaud:
                        break;
                    default:
                        throw new ObdException("Unhandled protocol type.  Feel free to add it and send us the changes!");
                }

                var strings = ecuResponse.Trim().Split(' ');
                var bytes = new byte[strings.Length - offset - 1];

                for (var i = offset; i < strings.Length - 1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(strings[i]) && !strings[i].Contains("STOPPED"))
                        bytes[i - offset] = Convert.ToByte(strings[i].Trim(), 16);
                }

                payload[Convert.ToInt32(strings[ecuByte].Trim(), 16)] = bytes;
            }

            return payload;
        }

        public static List<int> DecodeSupportedPids(IReadOnlyList<byte> data, int offset)
        {
            var pids = new List<int>();

            if (data == null)
                return pids;

            for (var byteIdx = 0; byteIdx < data.Count; byteIdx++)
            {
                for (var i = 0; i < 8; i++)
                {
                    if ((data[byteIdx] << i & 0x80) == 0x80)
                        pids.Add(byteIdx * 8 + i + 1 + offset);
                }
            }
            return pids;
        }

    }
}
