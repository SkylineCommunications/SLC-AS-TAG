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
    [GQIMetaData(Name = "Get TAG All Outputs")]
    public class GetTagOutputs : IGQIDataSource, IGQIOnInit
    {
        private GQIDMS _dms;

        private enum Severity
        {
            Critical = 1,
            Major = 2,
            Minor = 3,
            Warning = 4,
            Notice = 5,
            Info = 6,
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
            };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            return new OnArgumentsProcessedOutputArgs();
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
                    var channelStatusOverviewTable = SharedMethods.GetTable(_dms, response, MCS.ChannelStatusOverview);
                    GetChannelsMcsTableRows(rows, response, channelStatusOverviewTable);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var encoderConfigTable = SharedMethods.GetTable(_dms, response, MCM.ChannelStatusOverview);
                        GetOutputMcmTableRows(rows, response, encoderConfigTable);
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

        private void GetChannelsMcsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelStatusOverviewTable)
        {
            foreach (var tableRow in channelStatusOverviewTable)
            {
                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[2]) }, // Label
                    new GQICell { Value = Convert.ToString(tableRow[4]) }, // Thumbnail
                    new GQICell { Value = Convert.ToString(tableRow[5]) }, // Severity
                    new GQICell { Value = Convert.ToString(tableRow[6]) }, // Active Events
                    new GQICell { Value = Convert.ToString(tableRow[7]) }, // Bitrate
                    new GQICell { Value = Convert.ToString(tableRow[8]) }, // Type
                    new GQICell { Value = Convert.ToString(tableRow[9]) }, // CPU Usage
                    new GQICell { Value = Convert.ToString(tableRow[10]) }, // Memory Allocated
                    new GQICell { Value = Convert.ToString(tableRow[11]) }, // Memory Usage
                    new GQICell { Value = Convert.ToString(tableRow[1]) }, // More Info (Index)
                    new GQICell { Value = Convert.ToString(tableRow[13]) }, // Profile
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

        private void GetOutputMcmTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelStatusOverviewTable)
        {
            var channelEventsOverviewTable = SharedMethods.GetTable(_dms, response, MCM.ChannelEventsOverview);

            var eventNamesAdded = new List<string>();

            foreach (var tableRow in channelStatusOverviewTable)
            {
                var name = Convert.ToString(tableRow[12]);
                eventNamesAdded.Add(name);
                if (eventNamesAdded.Contains(name))
                {
                    continue;
                }

                var channelEventsRows = channelEventsOverviewTable.Where(x => Convert.ToInt32(x[6 /* Status */]).Equals(1)).ToList();
                var activeEvents = channelEventsRows.Count;
                var severity = GetSeverity(channelEventsRows, name);

                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[1]) }, // Label
                    new GQICell { Value = null }, // Thumbnail
                    new GQICell { Value = severity }, // Severity
                    new GQICell { Value = activeEvents }, // Active Events
                    new GQICell { Value = null }, // Bitrate
                    new GQICell { Value = Convert.ToString(tableRow[8]) }, // Type
                    new GQICell { Value = null }, // CPU Usage
                    new GQICell { Value = null }, // Memory Allocated
                    new GQICell { Value = null }, // Memory Usage
                    new GQICell { Value = null }, // More Info (Index)
                    new GQICell { Value = null }, // Profile
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

        private string GetSeverity(List<object[]> channelEventsRows, string name)
        {
            var filteredRows = channelEventsRows.FindAll(x => Convert.ToString(x[1 /* Name */]).Equals(name));
            return ((Severity)filteredRows.Select(x => Convert.ToInt32(x[2])).Min()).ToString();
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
            };

            var row = new GQIRow(debugCells);
            rows.Add(row);
        }
    }
}