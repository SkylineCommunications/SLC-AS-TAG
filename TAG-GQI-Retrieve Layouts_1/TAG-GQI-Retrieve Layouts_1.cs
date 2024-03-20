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

namespace TAG_GQI_Retrieve_Layouts_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;
    using SharedMethods;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG All Layouts")]
    public class GetTagLayouts : IGQIDataSource, IGQIOnInit
    {
        private readonly int mcsAllLayoutTable = 5600;

        private readonly int mcmAllLayoutTable = 10300;

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
                new GQIStringColumn("Element ID"),
                new GQIStringColumn("Index"),
                new GQIStringColumn("Layout"),
                new GQIStringColumn("Position"),
                new GQIStringColumn("Channel Source"),
                new GQIStringColumn("Title"),
                new GQIStringColumn("Layout ID"),
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

                var mcsResponses = _dms.SendMessages(mcsRequest);

                foreach (var response in mcsResponses.Select(x => (LiteElementInfoEvent)x))
                {
                    var allLayoutsTable = SharedMethods.GetTable(_dms, response, mcsAllLayoutTable);
                    GetAllLayoutsTableRows(rows, response, allLayoutsTable);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(mcmRequest);
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var allLayoutsTable = SharedMethods.GetTable(_dms,response, mcmAllLayoutTable);
                        GetAllLayoutsTableRows(rows, response, allLayoutsTable);
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

        private void GetAllLayoutsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] allLayoutsTable)
        {
            for (int i = 0; i < allLayoutsTable.Length; i++)
            {
                var deviceAllLayoutsRow = allLayoutsTable[i];
                var title = Convert.ToString(deviceAllLayoutsRow[2]) == "0" ? "None" : Convert.ToString(deviceAllLayoutsRow[2]);
                var channelSource = Convert.ToString(deviceAllLayoutsRow[1]) == "0" ? "None" : Convert.ToString(deviceAllLayoutsRow[1]);

                var cells = new[]
                {
                    new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                    new GQICell { Value = Convert.ToString(deviceAllLayoutsRow[0]) }, // Index
                    new GQICell { Value = Convert.ToString(deviceAllLayoutsRow[4]) }, // Layout
                    new GQICell { Value = Convert.ToString(deviceAllLayoutsRow[5]) }, // Position
                    new GQICell { Value = channelSource }, // Channel Source
                    new GQICell { Value = title }, // Title
                    new GQICell { Value = Convert.ToString(deviceAllLayoutsRow[3]) }, // Layout Id
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

        private void CreateDebugRow(List<GQIRow> rows, string message)
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