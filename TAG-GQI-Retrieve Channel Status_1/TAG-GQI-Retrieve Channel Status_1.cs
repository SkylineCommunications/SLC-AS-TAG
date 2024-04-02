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

dd/mm/2024	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace TAG_GQI_Retrieve_Channel_Details_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharedMethods;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG Channel Status")]
    public class GetTagOutputs : IGQIDataSource, IGQIOnInit
    {
        private GQIDMS _dms;

        public enum FormattedValueType
        {
            Bitrate,
            MemoryUsage,
            MemoryAllocated,
            CpuUsage,
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
                new GQIStringColumn("Label"),
                new GQIStringColumn("Thumbnail"),
                new GQIStringColumn("Severity"),
                new GQIStringColumn("Active Events"),
                new GQIStringColumn("Bitrate"),
                new GQIStringColumn("Type"),
                new GQIStringColumn("CPU Usage"),
                new GQIStringColumn("Memory Allocated"),
                new GQIStringColumn("Memory Usage"),
                new GQIStringColumn("More Info"),
                new GQIStringColumn("Profile"),
                new GQIStringColumn("Channel ID"),
                new GQIStringColumn("Element ID"),
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
                    var channelStatusOverviewTable = SharedMethods.GetTable(_dms, response, Mcs.ChannelStatusOverview);
                    GetChannelsMcsTableRows(rows, response, channelStatusOverviewTable);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var encoderConfigTable = SharedMethods.GetTable(_dms, response, Mcm.ChannelStatusOverview);
                        GetChannelsMcmTableRows(rows, response, encoderConfigTable);
                    }
                }
            }
            catch (Exception)
            {
                // CreateDebugRow(rows, $"exception: {e}");
            }

            return new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };
        }

        private static string GetFormattedValue(string valueToCheck, FormattedValueType formattedValueType)
        {
            if (double.TryParse(valueToCheck, out double parsedDouble))
            {
                switch (formattedValueType)
                {
                    case FormattedValueType.Bitrate:

                        if (parsedDouble > 1000)
                        {
                            return (parsedDouble / 1000).ToString("F3") + " Gbps";
                        }
                        else
                        {
                            return parsedDouble.ToString("F3") + " Mbps";
                        }

                    case FormattedValueType.MemoryUsage:
                        return (parsedDouble * 1000).ToString("F3") + " kb";
                    case FormattedValueType.MemoryAllocated:
                        if (parsedDouble > 1000)
                        {
                            return (parsedDouble / 1000).ToString("F3") + " Gb";
                        }
                        else
                        {
                            return parsedDouble.ToString("F3") + " Mb";
                        }

                    case FormattedValueType.CpuUsage:
                        return parsedDouble.ToString("F0") + " %";

                    default:
                        return "N/A";
                }
            }
            else
            {
                return "N/A";
            }
        }

        private void GetChannelsMcsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelStatusOverviewTable)
        {
            foreach (var tableRow in channelStatusOverviewTable)
            {
                var bitrate = GetFormattedValue(Convert.ToString(tableRow[7]), FormattedValueType.Bitrate);
                var memoryAllocated = GetFormattedValue(Convert.ToString(tableRow[10]), FormattedValueType.MemoryAllocated);
                var memoryUsage = GetFormattedValue(Convert.ToString(tableRow[11]), FormattedValueType.MemoryUsage);
                var cpuUsage = GetFormattedValue(Convert.ToString(tableRow[9]), FormattedValueType.CpuUsage);
                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[2]) }, // Label
                    new GQICell { Value = Convert.ToString(tableRow[4]) }, // Thumbnail
                    new GQICell { Value = Convert.ToString(tableRow[5]) }, // Severity
                    new GQICell { Value = Convert.ToString(tableRow[6]) }, // Active Events
                    new GQICell { Value = bitrate }, // Bitrate
                    new GQICell { Value = Convert.ToString(tableRow[8]) }, // Type
                    new GQICell { Value = cpuUsage}, // CPU Usage
                    new GQICell { Value = memoryAllocated }, // Memory Allocated
                    new GQICell { Value = memoryUsage }, // Memory Usage
                    new GQICell { Value = "Info" }, // More Info (Index)
                    new GQICell { Value = Convert.ToString(tableRow[13]) }, // Profile
                    new GQICell { Value = Convert.ToString(tableRow[0]) }, // ChannelId
                    new GQICell { Value = Convert.ToString(elementID)}, // ElementId
                };

                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                var row = new GQIRow(cells)
                {
                    Metadata = rowMetadata,
                };

                rows.Add(row);
            }
        }

        private void GetChannelsMcmTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelStatusOverviewTable)
        {
            var channelEventsOverviewTable = SharedMethods.GetTable(_dms, response, Mcm.ChannelEventsOverview);

            var eventNamesAdded = new List<string>();

            foreach (var tableRow in channelStatusOverviewTable)
            {
                var name = Convert.ToString(tableRow[12]);
                if (eventNamesAdded.Contains(name))
                {
                    continue;
                }

                eventNamesAdded.Add(name);

                var channelEventsRows = channelEventsOverviewTable.Where(x => Convert.ToInt32(x[6 /* Status */]).Equals(1)).ToList();
                var activeEvents = channelEventsRows.Count;
                var type = SharedMethods.GetValueFromStringDictionary(Mcm.ChannelConfigAccessTypeDict, Convert.ToString(tableRow[16]));
                var severity = SharedMethods.GetValueFromStringDictionary(Mcm.ChannelConfigSeverityDict, Convert.ToString(tableRow[4]));
                var elementID = new ElementID(response.DataMinerID, response.ElementID);

                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[12]) }, // Label
                    new GQICell { Value = "N/A" }, // Thumbnail
                    new GQICell { Value = severity }, // Severity
                    new GQICell { Value = Convert.ToString(activeEvents) }, // Active Events
                    new GQICell { Value = "N/A" }, // Bitrate
                    new GQICell { Value = type}, // Type
                    new GQICell { Value = "N/A" }, // CPU Usage
                    new GQICell { Value = "N/A" }, // Memory Allocated
                    new GQICell { Value = "N/A" }, // Memory Usage
                    new GQICell { Value = "Info" }, // More Info (Index)
                    new GQICell { Value = "N/A" }, // Profile
                    new GQICell { Value = Convert.ToString(tableRow[0]) }, // ChannelId
                    new GQICell { Value = Convert.ToString(elementID)}, // ElementId
                };

                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                var row = new GQIRow(cells)
                {
                    Metadata = rowMetadata,
                };

                rows.Add(row);
            }
        }

        // private static void CreateDebugRow(List<GQIRow> rows, string message)
        // {
        //    var debugCells = new[]
        //    {
        //        new GQICell { Value = message },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //        new GQICell { Value = null },
        //    };

        // var row = new GQIRow(debugCells);
        //    rows.Add(row);
        // }
    }
}