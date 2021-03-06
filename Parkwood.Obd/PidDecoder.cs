﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;
using System.Globalization;

namespace Parkwood.Obd
{
    public static class PidDecoder
    {
        public static Dictionary<int, byte[]> ParsePidCmd(string commandResult, Protocol protocol)
        {
            var payload = new Dictionary<int, byte[]>();
            if (commandResult.Contains("NO DATA") || commandResult.Contains("ERROR")) { return null; }

            var ecus = commandResult.Trim().Split('\r');

            foreach (var cleaned in from ecuResponse in ecus where !string.IsNullOrEmpty(ecuResponse) && ecuResponse != ">" select ecuResponse.Replace(">", string.Empty))
            {
                int offset;
                int ecuByte;

                switch (protocol)
                {
                    case Protocol.Iso91412:
                        offset = 5;
                        ecuByte = 2;
                        break;
                    case Protocol.Iso157654Can11Bit500Kbaud:
                        offset = 4;
                        ecuByte = 0;
                        break;
                    case Protocol.Iso157654Can29Bit500Kbaud:
                        offset = 7;
                        ecuByte = 3;
                        break;
                    default:
                        throw new ObdException("Unhandled protocol type.");
                }


                var strings = cleaned.Trim().Split(' ');
                var bytes = new byte[strings.Length - offset]; // get rid of the header and the trailing checksum byte

                for (var i = offset; i < strings.Length - 1; i++)
                {
                    if (!string.IsNullOrWhiteSpace(strings[i]) && !strings[i].Contains("STOPPED"))
                    {
                        bytes[i - offset] = Convert.ToByte(strings[i].Trim(), 16);
                    }
                }

                payload[Convert.ToInt32(strings[ecuByte].Trim(), 16)] = bytes;
            }

            return payload;
        }
        private static int ParseString(string str, int bytes)
        {
            return int.Parse(str.Substring(4, bytes * 2), NumberStyles.HexNumber);
        }
        //lifted from mydriving aka.ms/mydriving
        public static string ParseObd01Msg(string input)
        {
            string str = input.Replace("\r", "").Replace("\n", "").Replace(" ", "").Replace(">", "").Trim();
            if (!str.StartsWith("41") || str.Length < 6)
                return "-255";
            string pid = str.Substring(2, 2);
            int result;
            switch (pid)
            {
                case "04": //EngineLoad
                    return (ParseString(str, 1) * 100 / 255).ToString();
                case "06": //ShortTermFuelBank1
                    return ((ParseString(str, 1) - 128) * 100 / 128).ToString();
                case "07": //LongTermFuelBank1
                    return ((ParseString(str, 1) - 128) * 100 / 128).ToString();
                case "0C": //RPM
                    result = (ParseString(str, 2) / 4);
                    if (result < 0 || result > 16383)
                        result = -255;
                    return result.ToString();
                case "0D": //Speed
                    result = ParseString(str, 1);
                    if (result < 0 || result > 255)
                        result = -255;
                    return result.ToString();
                case "0F": //InsideTemperature
                    return (ParseString(str, 1) - 40).ToString();
                case "10": //MAF air flow rate
                    result = (ParseString(str, 2) / 100);
                    if (result < 0 || result > 655)
                        result = -255;
                    return result.ToString();
                case "11": //Throttle position
                    return (ParseString(str, 1) * 100 / 255).ToString();
                case "1F": //Runtime 
                    return ParseString(str, 2).ToString();
                case "21": //DistancewithML  
                    return ParseString(str, 2).ToString();
                case "2C": //Commanded EGR
                    return (ParseString(str, 1) * 100 / 255).ToString();
                case "2D": //EGR Error
                    return ((ParseString(str, 1) - 128) * 100 / 128).ToString();
                case "33": //BarometricPressure
                    return ParseString(str, 1).ToString();
                case "45": //Relative throttle position
                    return (ParseString(str, 1) * 100 / 255).ToString();
                case "46": //OutsideTemperature
                    return (ParseString(str, 1) - 40).ToString();
                case "5E": //EngineFuelRate
                    result = (ParseString(str, 2) / 20);
                    if (result < 0 || result > 3212)
                        result = -255;
                    return result.ToString();
            }
            return "ERROR";
        }

        public static IEnumerable<int> DecodeSupportedPids(IReadOnlyList<byte> data, int offset)
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

        #region conversions

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

        #endregion

    }
}