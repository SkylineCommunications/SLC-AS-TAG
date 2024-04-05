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
    using GQI_TAG_GetEndpoints_1.RealTimeUpdates;
    using SharedMethods;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG All Layouts")]
    public class GetTagLayouts : IGQIDataSource, IGQIOnInit, IGQIUpdateable
    {
        private readonly int mcsAllLayoutTable = 5600;

        private readonly int mcmAllLayoutTable = 10300;

        private int dataminerId;
        private int elementId;

        private GQIDMS _dms;

        private DataProvider _dataProvider;

        private ICollection<GQIRow> _currentRows = Array.Empty<GQIRow>();
        private IGQIUpdater _updater;
        private IEnumerable<LiteElementInfoEvent> _mcsLiteElementsInfoEvents;

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            _dms = args.DMS;
            GetTagArgument();

            StaticDataProvider.Initialize(_dms, dataminerId, elementId);
            _dataProvider = StaticDataProvider.Instance;

            return new OnInitOutputArgs();
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

        public void OnStartUpdates(IGQIUpdater updater)
        {
            _updater = updater;
            _dataProvider.AllLayoutsMcsTable.Changed += TableData_OnChanged;
        }

        public void OnStopUpdates()
        {
            _dataProvider.AllLayoutsMcsTable.Changed -= TableData_OnChanged;
            _updater = null;
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var newRows = CalculateNewRows();
            try
            {
                return new GQIPage(newRows.ToArray())
                {
                    HasNextPage = false,
                };
            }
            finally
            {
                _currentRows = newRows.ToList();
            }
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

        private void TableData_OnChanged(object sender, ParameterTableUpdateEventMessage e)
        {
            var newRows = CalculateNewRows().ToList();
            CreateDebugRow(newRows,"update");
            try
            {
                var comparison = new GqiTableComparer(_currentRows, newRows);

                foreach (var row in comparison.RemovedRows)
                {
                    _updater.RemoveRow(row.Key);
                }

                foreach (var row in comparison.UpdatedRows)
                {
                    _updater.UpdateRow(row);
                }

                foreach (var row in comparison.AddedRows)
                {
                    _updater.AddRow(row);
                }
            }
            finally
            {
                _currentRows = newRows;
            }
        }

        private IEnumerable<GQIRow> CalculateNewRows()
        {
            var rows = new List<GQIRow>();
            var mcmRequest = new GetLiteElementInfo
            {
                ProtocolName = "TAG Video Systems MCM-9000",
                ProtocolVersion = "Production",
            };

            foreach (var response in _mcsLiteElementsInfoEvents)
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
                    var allLayoutsTable = SharedMethods.GetTable(_dms, response, mcmAllLayoutTable);
                    GetAllLayoutsTableRows(rows, response, allLayoutsTable);
                }
            }

            return rows;
        }

        private void GetTagArgument()
        {
            dataminerId = -1;
            elementId = -1;

            var mcsRequest = new GetLiteElementInfo
            {
                ProtocolName = "TAG Video Systems Media control System (MCS)",
                ProtocolVersion = "Production",
            };

            var mcsResponses = _dms.SendMessages(new DMSMessage[] { mcsRequest });
            _mcsLiteElementsInfoEvents = mcsResponses.Select(x => (LiteElementInfoEvent)x);
            foreach (var response in _mcsLiteElementsInfoEvents)
            {
                if (response == null)
                {
                    continue;
                }

                dataminerId = response.DataMinerID;
                elementId = response.ElementID;
                break;
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
            };

            var row = new GQIRow(debugCells);
            rows.Add(row);
        }
    }
}