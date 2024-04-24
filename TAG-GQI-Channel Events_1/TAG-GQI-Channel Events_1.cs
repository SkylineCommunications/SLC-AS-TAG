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

namespace TAG_GQI_Channel_Severity_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.StaticData;
    using SharedMethods;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get Channel Events")]
    public class GetChannelEvents : IGQIDataSource, IGQIOnInit, IGQIInputArguments
    {
        private readonly GQIStringArgument channelId = new GQIStringArgument("Channel ID") { IsRequired = true };
        private string _channelId;

        private GQIDMS _dms;

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            return default;
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Severity"),
                new GQIStringColumn("Extra Description"),
                new GQIStringColumn("Status"),
                new GQIStringColumn("Acknowledge"),
                new GQIDateTimeColumn("Timestamp"),
                new GQIStringColumn("Occurrences"),
            };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { channelId };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _channelId = args.GetArgumentValue(channelId);
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

                var mcsStaticData = new MCS_StaticData();
                foreach (var response in mcsResponses.Select(x => (LiteElementInfoEvent)x))
                {
                    var channelEventsTable = SharedMethods.GetTable(_dms, response, mcsStaticData.ChannelEventsOverview);
                    GetChannelStatusMcsTableRows(rows, response, channelEventsTable, mcsStaticData);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    var mcmStaticData = new MCM_StaticData();
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var channelsEventsTable = SharedMethods.GetTable(_dms, response, mcmStaticData.ChannelEventsOverview);
                        GetChannelStatusMcmTableRows(rows, response, channelsEventsTable, mcmStaticData);
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

        private void GetChannelStatusMcsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelEventsTable, IStaticData staticData)
        {
            var filteredRows = channelEventsTable.Where(x => Convert.ToString(x[14]).Equals(_channelId));
            foreach (var channelEventRow in filteredRows)
            {
                var severity = Convert.ToString(channelEventRow[7]).IsNullOrEmpty() ? "N/A" : Convert.ToString(channelEventRow[7]);
                var extraDescription = Convert.ToString(channelEventRow[6]).IsNullOrEmpty() ? "N/A" : Convert.ToString(channelEventRow[6]);
                var status = Convert.ToString(channelEventRow[9]).IsNullOrEmpty() ? "N/A" : staticData.ChannelEventsStatus[Convert.ToString(channelEventRow[9])];
                var acknowledge = Convert.ToString(channelEventRow[10]).IsNullOrEmpty() ? "N/A" : staticData.ChannelEventsAcknowledge[Convert.ToString(channelEventRow[10])];
                var occurrences = Convert.ToString(channelEventRow[11]);

                if (occurrences.IsNullOrEmpty() || occurrences.Equals("-1"))
                {
                    occurrences = "N/A";
                }

                DateTime timestamp;

                if (Double.TryParse(Convert.ToString(channelEventRow[8]), out double timestampDouble))
                {
                    timestamp = DateTime.FromOADate(timestampDouble).ToUniversalTime();
                }
                else
                {
                    timestamp = DateTime.MinValue.ToUniversalTime();
                }

                var cells = new[]
                {
                    new GQICell { Value = severity }, // Severity
                    new GQICell { Value = extraDescription }, // Extra Description
                    new GQICell { Value = status }, // Status
                    new GQICell { Value = acknowledge }, // Acknowledge
                    new GQICell { Value = timestamp }, // Timestamp
                    new GQICell { Value = occurrences }, // Occurrences
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

        private void GetChannelStatusMcmTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelEventsTable, IStaticData staticData)
        {
            var filteredRows = channelEventsTable.Where(x => Convert.ToString(x[15]).Equals(_channelId));
            foreach (var channelEventRow in filteredRows)
            {
                var severity = Convert.ToString(channelEventRow[2]).IsNullOrEmpty() ? "N/A" : staticData.ChannelConfigSeverityDict[Convert.ToString(channelEventRow[2])];
                var extraDescription = Convert.ToString(channelEventRow[5]).IsNullOrEmpty() ? "N/A" : Convert.ToString(channelEventRow[5]);
                var status = Convert.ToString(channelEventRow[6]).IsNullOrEmpty() ? "N/A" : staticData.ChannelEventsStatus[Convert.ToString(channelEventRow[6])];
                var acknowledge = Convert.ToString(channelEventRow[8]).IsNullOrEmpty() ? "N/A" : staticData.ChannelEventsAcknowledge[Convert.ToString(channelEventRow[8])];
                var occurrences = Convert.ToString(channelEventRow[9]);

                if (occurrences.IsNullOrEmpty() || occurrences.Equals("-1"))
                {
                    occurrences = "N/A";
                }

                DateTime timestamp;

                if (Double.TryParse(Convert.ToString(channelEventRow[4]), out double timestampDouble))
                {
                    timestamp = DateTime.FromOADate(timestampDouble).ToUniversalTime();
                }
                else
                {
                    timestamp = DateTime.MinValue.ToUniversalTime();
                }

                var cells = new[]
                {
                    new GQICell { Value = severity }, // Severity
                    new GQICell { Value = extraDescription }, // Extra Description
                    new GQICell { Value = status }, // Status
                    new GQICell { Value = acknowledge }, // Acknowledge
                    new GQICell { Value = timestamp }, // Timestamp
                    new GQICell { Value = occurrences }, // Occurrences
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
