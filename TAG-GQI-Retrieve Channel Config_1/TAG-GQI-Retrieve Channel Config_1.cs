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

namespace TAG_GQI_Retrieve_Channel_Config_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using Common.StaticData;
    using SharedMethods;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG Channel Config")]
    public class GetTagOutputs : IGQIDataSource, IGQIOnInit
    {
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
                new GQIStringColumn("Label"),
                new GQIStringColumn("Access Type"),
                new GQIStringColumn("Service Type"),
                new GQIStringColumn("Recording"),
                new GQIStringColumn("Device"),
                new GQIStringColumn("Monitoring Level"),
                new GQIStringColumn("More Info"),
                new GQIStringColumn("Channel ID"),
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
                var mcsStaticData = new MCS_StaticData();
                foreach (var response in mcsResponses.Select(x => (LiteElementInfoEvent)x))
                {
                    var channelConfigurationTable = SharedMethods.GetTable(_dms, response, mcsStaticData.ChannelsConfiguration);
                    GetChannelsMcsTableRows(rows, response, channelConfigurationTable, mcsStaticData);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    var mcmStaticdata = new MCM_StaticData();
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var allChannelsProfileTable = SharedMethods.GetTable(_dms, response, mcmStaticdata.AllChannelsProfile);
                        GetChannelConfigMcmTableRows(rows, response, allChannelsProfileTable, mcmStaticdata);
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

        private void GetChannelsMcsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] channelConfigurationTable, IStaticData staticData)
        {
            foreach (var tableRow in channelConfigurationTable)
            {
                var deviceName = Convert.ToString(tableRow[6]).Equals("Not Set") ? "Unmonitored" : Convert.ToString(tableRow[6]);
                var accessType = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigAccessTypeDict, Convert.ToString(tableRow[2]));
                var serviceType = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigServiceTypeDict, Convert.ToString(tableRow[3]));
                var recording = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigRecordingDict, Convert.ToString(tableRow[4]));
                var monitoringLevel = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigMonitoringLevelDict, Convert.ToString(tableRow[12]));

                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[1]) }, // Label
                    new GQICell { Value = accessType }, // Access Type
                    new GQICell { Value = serviceType }, // Service Type
                    new GQICell { Value = recording }, // Recording
                    new GQICell { Value = deviceName }, // Device
                    new GQICell { Value = monitoringLevel}, // Monitoring Level
                    new GQICell { Value = "Info" }, // More Info (Index)
                    new GQICell { Value = Convert.ToString(tableRow[0]) }, // More Info (Index)
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

        private void GetChannelConfigMcmTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] allChannelsProfileTable, IStaticData staticData)
        {
            var channelStatusTable = SharedMethods.GetTable(_dms, response, staticData.ChannelStatusOverview);
            foreach (var tableRow in allChannelsProfileTable)
            {
                var matchingRow = channelStatusTable.FirstOrDefault(x => Convert.ToString(x[12]).Equals(Convert.ToString(tableRow[9])));
                string deviceName;
                if (matchingRow != null)
                {
                    deviceName = Convert.ToString(matchingRow[19]).IsNullOrEmpty() ? "Unmonitored" : Convert.ToString(matchingRow[19]);
                }
                else
                {
                    // empty table/rows
                    continue;
                }

                var accessType = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigAccessTypeDict, Convert.ToString(tableRow[13]));
                var serviceType = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigServiceTypeDict, Convert.ToString(tableRow[19]));
                var monitoringLevel = SharedMethods.GetValueFromStringDictionary(staticData.ChannelConfigMonitoringLevelDict, Convert.ToString(tableRow[32]));

                GQICell[] cells = new[]
                {
                    new GQICell { Value = Convert.ToString(tableRow[9]) }, // Label
                    new GQICell { Value = accessType }, // Access Type
                    new GQICell { Value = serviceType }, // Service Type
                    new GQICell { Value = "N/A" }, // Recording
                    new GQICell { Value = deviceName }, // Device
                    new GQICell { Value = monitoringLevel}, // Monitoring Level
                    new GQICell { Value = "Info" }, // More Info (Index)
                    new GQICell { Value = Convert.ToString(tableRow[0]) }, // Channel ID
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