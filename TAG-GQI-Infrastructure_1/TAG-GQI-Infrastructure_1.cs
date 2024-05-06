/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

09/02/2024	1.0.0.1		BSM, Skyline	Initial version
****************************************************************************
*/

namespace TAG_GQI_Infrastructure_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Common.StaticData;
	using SharedMethods;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG Infrastructure")]
    public class GetTAGInfrastructure : IGQIDataSource, IGQIOnInit
    {
        private GQIDMS _dms;

        public enum MCSTableId
        {
            Devices = 1200,
            DeviceHardware = 1300,
            DeviceInfo = 1400,
            DeviceCpu = 1500,
            DeviceCpuTemp = 1600,
        }

        public enum MCMTableId
        {
            DeviceOverview = 1000,
        }

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return default;
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Name"),
                new GQIStringColumn("Serial"),
                new GQIStringColumn("Version"),
                new GQIStringColumn("Url"),
                new GQIStringColumn("License Sharing"),
                new GQIDateTimeColumn("Up Time"),
                new GQIStringColumn("Status"),
                new GQIStringColumn("Capacity"),
                new GQIStringColumn("Licenses"),
                new GQIStringColumn("Channels"),
                new GQIStringColumn("Uncompressed"),
                new GQIStringColumn("Outputs"),
                new GQIStringColumn("Descramblers"),
                new GQIStringColumn("Recorders"),
                new GQIDoubleColumn("CPU"),
                new GQIDoubleColumn("Temperature"),
                new GQIStringColumn("Clock Offset"),
                new GQIStringColumn("Model"),
                new GQIStringColumn("Memory"),
                new GQIDoubleColumn("Used Outputs"),
                new GQIDoubleColumn("Limit Outputs"),
                new GQIDoubleColumn("Used Channels"),
                new GQIDoubleColumn("Limit Channels"),
                new GQIStringColumn("Device Key"),
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var rows = new List<GQIRow>();

            try
            {
                var mcmRequest = new GetLiteElementInfo
                {
                    ProtocolName = "TAG Video Systems MCM-9000",
                    ProtocolVersion = "Production",
                };

                var mcsRequest = new GetLiteElementInfo
                {
                    ProtocolName = "TAG Video Systems Media control System (MCS)",
                    ProtocolVersion = "Production",
                };

                var mcsResponses = _dms.SendMessages(new DMSMessage[] { mcsRequest });
                foreach (var response in mcsResponses.Select(x => (LiteElementInfoEvent)x))
                {
                    GetMCSRows(rows, response);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    var mcmStaticData = new MCM_StaticData();
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        GetMCMRows(rows, response, mcmStaticData);
                    }
                }
            }
            catch (Exception e)
            {
                CreateDebugRow(rows, $"exception: {e}");
            }

            return new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };
        }

        private void GetMCMRows(List<GQIRow> rows, LiteElementInfoEvent response, IStaticData staticInfo)
        {
            var devicesRows = SharedMethods.GetTable(_dms, response, (int)MCMTableId.DeviceOverview);
            var cpu = Convert.ToDouble(SharedMethods.GetParameter(_dms, response, staticInfo.CPU_Pid));
            var memory = Convert.ToDouble(SharedMethods.GetParameter(_dms, response, staticInfo.Memory_Pid));

            var cloudLicenseRow = devicesRows.FirstOrDefault(x => Convert.ToString(x[0]) == "Cloud License");

            for (int i = 0; i < devicesRows.Length; i++)
            {
                var deviceRow = devicesRows[i];
                var deviceName = Convert.ToString(deviceRow[0]);
                if (deviceName == "Cloud License")
                {
                    continue;
                }

                var cells = new[]
                {
                    new GQICell { Value = deviceName }, // Name
                    new GQICell { Value = Convert.ToString(deviceRow[4]) }, // Serial
                    new GQICell { Value = Convert.ToString(deviceRow[5]) }, // Version
                    new GQICell { Value = response.PollingIP.Replace("https://", String.Empty) }, // Url
                    new GQICell { Value = Convert.ToString(deviceRow[14]) == "1" ? "Enabled" : "Disabled" }, // License Sharing
                    new GQICell { Value = DateTime.FromOADate(Convert.ToDouble(deviceRow[9])).ToUniversalTime() }, // Up time
                    new GQICell { Value = Convert.ToString(deviceRow[3]) == "4" || Convert.ToString(deviceRow[3]) == "1" ? "Up" : "Down" }, // Status
                    new GQICell { Value = CheckUsage(deviceRow, 17, 16, cloudLicenseRow) }, // Capacity
                    new GQICell { Value = CheckUsage(deviceRow, 27, 28, cloudLicenseRow) }, // Licenses
                    new GQICell { Value = CheckUsage(deviceRow, 26, 7, cloudLicenseRow) }, // Channels
                    new GQICell { Value = "N/A"}, // Uncompressed
                    new GQICell { Value = CheckUsage(deviceRow, 14, -1, cloudLicenseRow) }, // Outputs
                    new GQICell { Value = "N/A" }, // Descramblers
                    new GQICell { Value = CheckUsage(deviceRow, 21, 20, cloudLicenseRow) }, // Recorders
                    new GQICell { Value = cpu, DisplayValue = cpu == -1 ? "N/A" : Convert.ToString(cpu) + " %" }, // CPU
                    new GQICell { Value = -1d, DisplayValue = "N/A" }, // Temperature
                    new GQICell { Value = $"{Convert.ToDouble(deviceRow[25]).ToString("F6")} MHz"}, // Clock Offset
                    new GQICell { Value = Convert.ToString(deviceRow[2]) }, // Model
                    new GQICell { Value = Convert.ToString(memory), DisplayValue = memory == -1 ? "N/A" : Convert.ToString(memory) }, // Memory
                    new GQICell { Value = Convert.ToDouble(deviceRow[29]), DisplayValue = CheckValue(Convert.ToDouble(deviceRow[29])) }, // Used Outputs
                    new GQICell { Value = 0d, DisplayValue = "N/A" }, // Limit Outputs
                    new GQICell { Value = Convert.ToDouble(deviceRow[26]), DisplayValue = CheckValue(Convert.ToDouble(deviceRow[26])) }, // Used Channels
                    new GQICell { Value = Convert.ToDouble(ExtractNumberFromString(deviceRow[7])), DisplayValue = CheckValue(Convert.ToDouble(ExtractNumberFromString(deviceRow[7]))) }, // Limit Channels
                    new GQICell { Value = Convert.ToString(deviceRow[0]) }, // Device Key
                };

                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                var row = new GQIRow(cells)
                {
                    Metadata = rowMetadata,
                };
                rows.Add(row);
            }
        }

        private void GetMCSRows(List<GQIRow> rows, LiteElementInfoEvent response)
        {
            var devicesRows = SharedMethods.GetTable(_dms, response, (int)MCSTableId.Devices);
            var deviceHardwareRows = SharedMethods.GetTable(_dms, response, (int)MCSTableId.DeviceHardware);
            var deviceInfoRows = SharedMethods.GetTable(_dms, response, (int)MCSTableId.DeviceInfo);
            var deviceCpuTable = SharedMethods.GetTable(_dms, response, (int)MCSTableId.DeviceCpu);
            var deviceCpuTempTable = SharedMethods.GetTable(_dms, response, (int)MCSTableId.DeviceCpuTemp);

            var averagedTemperaturesByDevice = new Dictionary<object, double>();
            if (deviceCpuTempTable != null && deviceCpuTempTable.Length > 0 && deviceCpuTempTable[0] != null && deviceCpuTempTable[0].Length > 0)
            {
                averagedTemperaturesByDevice = deviceCpuTempTable
                    .GroupBy(row => row[4])
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(row => Convert.ToDouble(row[3])).Average());
            }

            for (int i = 0; i < devicesRows.Length; i++)
            {
                var deviceRow = devicesRows[i];
                var deviceHardwareRow = CheckForRowInTable(deviceHardwareRows, i, 10);
                var deviceInfoRow = CheckForRowInTable(deviceInfoRows, i, 25);
                var deviceCpuRow = CheckForRowInTable(deviceCpuTable, i, 5);

                var deviceKey = Convert.ToString(deviceRow[0]);
                if (!averagedTemperaturesByDevice.TryGetValue(deviceKey, out double temperature))
                {
                    temperature = -1;
                }

                var cells = new[]
                {
                    new GQICell { Value = Convert.ToString(deviceRow[1]) }, // Name
                    new GQICell { Value = Convert.ToString(deviceHardwareRow[5]) }, // Serial
                    new GQICell { Value = Convert.ToString(deviceHardwareRow[6]) }, // Version
                    new GQICell { Value = Convert.ToString(deviceRow[2]) }, // Url
                    new GQICell { Value = Convert.ToString(deviceRow[7]) == "1" ? "Enabled" : "Disabled" }, // License Sharing
                    new GQICell { Value = DateTime.FromOADate(Convert.ToDouble(deviceHardwareRow[2])).ToUniversalTime() }, // Up time
                    new GQICell { Value = Convert.ToString(deviceInfoRow[20]) }, // Status
                    new GQICell { Value = CheckUsage(deviceInfoRow, 8, 9) }, // Capacity
                    new GQICell { Value = CheckUsage(deviceInfoRow, 5, 6) }, // Licenses
                    new GQICell { Value = CheckUsage(deviceInfoRow, 18, 19) }, // Channels
                    new GQICell { Value = CheckUsage(deviceInfoRow, 10, 11) }, // Uncompressed
                    new GQICell { Value = CheckUsage(deviceInfoRow, 14, 15) }, // Outputs
                    new GQICell { Value = CheckUsage(deviceInfoRow, 16, 17) }, // Descramblers
                    new GQICell { Value = CheckUsage(deviceInfoRow, 12, 13) }, // Recordings
                    new GQICell { Value = CheckValue(Convert.ToString(deviceCpuRow[4]), out double cpuValue), DisplayValue = cpuValue == -1 ? "N/A" : cpuValue + " %" }, // CPU
                    new GQICell { Value = temperature, DisplayValue = CheckValue(temperature) }, // Temperature
                    new GQICell { Value = $"{Convert.ToDouble(deviceInfoRow[4]).ToString("F6")} MHz"}, // Clock Offset
                    new GQICell { Value = Convert.ToString(deviceHardwareRow[4]) }, // Model
                    new GQICell { Value = CheckUsageModified(deviceCpuRow, 2, 3, 1000) }, // Memory
                    new GQICell { Value = CheckValue(Convert.ToString(deviceInfoRow[14]), out double outputsUsed), DisplayValue = outputsUsed == -1 ? "N/A" : Convert.ToString(outputsUsed) }, // Used Outputs
                    new GQICell { Value = CheckValue(Convert.ToString(deviceInfoRow[15]), out double outputsLimit), DisplayValue = outputsLimit == -1 ? "N/A" : Convert.ToString(outputsLimit) }, // Limit Outputs
                    new GQICell { Value = CheckValue(Convert.ToString(deviceInfoRow[18]), out double channelsUsed), DisplayValue = channelsUsed == -1 ? "N/A" : Convert.ToString(channelsUsed) }, // Used Channels
                    new GQICell { Value = CheckValue(Convert.ToString(deviceInfoRow[19]), out double channelsLimit), DisplayValue = channelsLimit == -1 ? "N/A" : Convert.ToString(channelsLimit) }, // Limit Channels
                    new GQICell { Value = Convert.ToString(deviceRow[0]) }, // Device Key
                };

                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                var row = new GQIRow(cells)
                {
                    Metadata = rowMetadata,
                };
                rows.Add(row);
            }
        }

        private static object[] CheckForRowInTable(object[][] deviceCpuTable, int i, int rowLength)
        {
            return i < deviceCpuTable.Length ? deviceCpuTable[i] : Enumerable.Repeat((object)null, rowLength).ToArray();
        }

        private static string CheckUsage(object[] deviceInfoRow, int usedAmountPosition, int limitPosition)
        {
            return CheckUsage(deviceInfoRow, usedAmountPosition, limitPosition, null);
        }

        private static string CheckUsage(object[] deviceInfoRow, int usedAmountPosition, int limitPosition, object[] cloudLicenseRow)
        {
            if (limitPosition == -1)
            {
                return Convert.ToDouble(deviceInfoRow[usedAmountPosition]) < 0 ? "N/A" : deviceInfoRow[usedAmountPosition] + " / 0";
            }

            var deviceLimit = Convert.ToDouble(ExtractNumberFromString(deviceInfoRow[limitPosition]));
            var cloudLimit = cloudLicenseRow != null ? ExtractNumberFromString(cloudLicenseRow[limitPosition]) : double.MinValue;

            var maxLimit = Math.Max(deviceLimit, cloudLimit);

            return Convert.ToDouble(deviceInfoRow[usedAmountPosition]) < 0 ? "N/A" : GetFormattedUsage(deviceInfoRow, usedAmountPosition, maxLimit);
        }

        private static string GetFormattedUsage(object[] deviceInfoRow, int usedAmountPosition, double limitValue)
        {
            return Convert.ToInt32(deviceInfoRow[usedAmountPosition]) + " / " + limitValue;
        }

        private static string CheckUsageModified(object[] deviceInfoRow, int usedAmountPosition, int limitPosition, int modifier)
        {
            return Convert.ToDouble(deviceInfoRow[usedAmountPosition]) < 0 ? "N/A" : GetFormattedUsageModified(deviceInfoRow, usedAmountPosition, limitPosition, modifier);
        }

        private static double ExtractNumberFromString(object value)
        {
            var stringValue = Convert.ToString(value);
            double number;

            if (!String.IsNullOrWhiteSpace(stringValue) && Double.TryParse(Regex.Match(stringValue, @"[0-9]*\.?[0-9]+").Value, out number))
            {
                return number;
            }

            return 0d;
        }

        private static string GetFormattedUsageModified(object[] deviceInfoRow, int usedAmountPosition, int limitPosition, int modifier)
        {
            var usedAmount = Convert.ToDouble(deviceInfoRow[usedAmountPosition]);
            var limitAmount = Convert.ToDouble(deviceInfoRow[limitPosition]);
            if (usedAmount > modifier)
            {
                usedAmount /= modifier;
                limitAmount /= modifier;
            }

            return Math.Round(usedAmount, 2) + " / " + Math.Round(limitAmount, 2);
        }

        private static double CheckValue(string value, out double returnValue)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                returnValue = -1;
                return -1;
            }

            var numericValue = Convert.ToDouble(Regex.Match(value, @"^[+-]?(\d*\.)?\d+$").Value);
            returnValue = numericValue == -1 ? -1 : numericValue;
            return returnValue;
        }

        private static string CheckValue(double value)
        {
            return value == -1 ? "N/A" : Convert.ToString(value);
        }

        private static void CreateDebugRow(List<GQIRow> rows, string message)
        {
            var debugCells = new[]
            {
                new GQICell { Value = message },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null },
                new GQICell { Value = null },
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null},
            };

            var row = new GQIRow(debugCells);
            rows.Add(row);
        }
    }
}