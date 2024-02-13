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
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG Infrastructure")]
    public class GetPeacockProvision : IGQIDataSource, IGQIOnInit
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
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var rows = new List<GQIRow>();

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
                foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                {
                    GetMCMRows(rows, response);
                }
            }

            return new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };
        }

        private void GetMCMRows(List<GQIRow> rows, LiteElementInfoEvent response)
        {
            var devicesRows = GetTable(response, (int)MCMTableId.DeviceOverview);

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
                    new GQICell { Value = CheckUsage(deviceRow, 17, 16) }, // Capacity
                    new GQICell { Value = "N/A"/*CheckUsage(deviceRow, 18, 19)*/ }, // Licenses
                    new GQICell { Value = "N/A"/*CheckUsage(deviceRow, 18, 19)*/ }, // Channels
                    new GQICell { Value = "N/A"/*CheckUsage(deviceRow, 10, 11)*/ }, // Uncompressed
                    new GQICell { Value = "N/A"/*CheckUsage(deviceRow, 14, 15)*/ }, // Outputs
                    new GQICell { Value = "N/A"/*CheckUsage(deviceRow, 16, 17)*/ }, // Descramblers
                    new GQICell { Value = CheckUsage(deviceRow, 21, 20) }, // Recorders
                    new GQICell { Value = -1d, DisplayValue = "N/A" }, // CPU
                    new GQICell { Value = -1d, DisplayValue = "N/A" }, // Temperature
                    new GQICell { Value = "-1", DisplayValue = "N/A" }, // Clock Offset
                };

                var row = new GQIRow(cells);
                rows.Add(row);
            }
        }

        private void GetMCSRows(List<GQIRow> rows, LiteElementInfoEvent response)
        {
            var devicesRows = GetTable(response, (int)MCSTableId.Devices);
            var deviceHardwareRows = GetTable(response, (int)MCSTableId.DeviceHardware);
            var deviceInfoRows = GetTable(response, (int)MCSTableId.DeviceInfo);
            var deviceCpuTable = GetTable(response, (int)MCSTableId.DeviceCpu);
            var deviceCpuTempTable = GetTable(response, (int)MCSTableId.DeviceCpuTemp);

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
                var deviceHardwareRow = deviceHardwareRows[i];
                var deviceInfoRow = deviceInfoRows[i];
                var deviceCpuRow = deviceCpuTable[i];

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
                    new GQICell { Value = Convert.ToDouble(deviceCpuRow[4]), DisplayValue = CheckValue(Convert.ToDouble(deviceCpuRow[4])) + " %" }, // CPU
                    new GQICell { Value = temperature, DisplayValue = CheckValue(temperature) }, // Temperature
                    new GQICell { Value = Convert.ToString(deviceInfoRow[4]), DisplayValue = CheckValue(Convert.ToDouble(deviceInfoRow[4])) }, // Clock Offset
                };

                var row = new GQIRow(cells);
                rows.Add(row);
            }
        }

        private static string CheckUsage(object[] deviceInfoRow, int usedAmountPosition, int limitPosition)
        {
            return Convert.ToString(deviceInfoRow[usedAmountPosition]) == "-1" ? "N/A" : GetFormattedUsage(deviceInfoRow, usedAmountPosition, limitPosition);
        }

        private static string GetFormattedUsage(object[] deviceInfoRow, int usedAmountPosition, int limitPosition)
        {
            return Convert.ToString(deviceInfoRow[usedAmountPosition]) + " / " + Convert.ToString(deviceInfoRow[limitPosition]);
        }

        private static string CheckValue(double temperature)
        {
            return temperature == -1 ? "N/A" : Convert.ToString(temperature);
        }

        private object[][] GetTable(LiteElementInfoEvent response, int tableId)
        {
            var partialTableRequest = new GetPartialTableMessage
            {
                DataMinerID = response.DataMinerID,
                ElementID = response.ElementID,
                ParameterID = tableId,
            };

            var messageResponse = _dms.SendMessage(partialTableRequest) as ParameterChangeEventMessage;
            if (messageResponse.NewValue.ArrayValue != null && messageResponse.NewValue.ArrayValue.Length > 0)
            {
                return BuildRows(messageResponse.NewValue.ArrayValue);
            }
            else
            {
                return new object[0][];
            }
        }

        private static object[][] BuildRows(ParameterValue[] columns)
        {
            int length1 = columns.Length;
            int length2 = 0;
            if (length1 > 0)
                length2 = columns[0].ArrayValue.Length;
            object[][] objArray;
            if (length1 > 0 && length2 > 0)
            {
                objArray = new object[length2][];
                for (int index = 0; index < length2; ++index)
                    objArray[index] = new object[length1];
            }
            else
            {
                objArray = new object[0][];
            }

            for (int index1 = 0; index1 < length1; ++index1)
            {
                ParameterValue[] arrayValue = columns[index1].ArrayValue;
                for (int index2 = 0; index2 < length2; ++index2)
                    objArray[index2][index1] = arrayValue[index2].IsEmpty ? (object)null : arrayValue[index2].ArrayValue[0].InteropValue;
            }

            return objArray;
        }
    }
}