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
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using TAG_GQI_Retrieve_Layouts_1.RealTimeUpdates;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG All Layouts")]
    public class GetTagLayouts : IGQIDataSource, IGQIOnInit, IGQIUpdateable, IGQIInputArguments
    {
        private readonly GQIStringArgument inputElementId = new GQIStringArgument("Element ID") { IsRequired = true };
        private string _elementId;
        private GQIDMS dms;

        private int dataminerId;
        private int elementId;

        private DataProvider _dataProvider;

        private ICollection<GQIRow> _currentRows = Array.Empty<GQIRow>();
        private IGQIUpdater _updater;
        private bool _isMcs;

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            dms = args.DMS;

            return new OnInitOutputArgs();
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { inputElementId };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            _elementId = args.GetArgumentValue(inputElementId);

            return new OnArgumentsProcessedOutputArgs();
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[]
            {
                new GQIStringColumn("Index"),
                new GQIStringColumn("Layout"),
                new GQIStringColumn("Position"),
                new GQIStringColumn("Channel Source"),
                new GQIStringColumn("Title"),
                new GQIStringColumn("Layout ID"),
                new GQIStringColumn("Element ID"),
            };
        }

        public void OnStartUpdates(IGQIUpdater updater)
        {
            GetTagArgument();
            _updater = updater;
            StaticDataProvider.Initialize(dms, dataminerId, elementId, _isMcs);
            _dataProvider = StaticDataProvider.Instance;
            _dataProvider.SourceTable.Changed += TableData_OnChanged;
        }

        public void OnStopUpdates()
        {
            _dataProvider.SourceTable.Changed -= TableData_OnChanged;
            StaticDataProvider.Reset();
            _updater = null;
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var newRows = CalculateNewRows().ToArray();
            try
            {
                return new GQIPage(newRows)
                {
                    HasNextPage = false,
                };
            }
            finally
            {
                _currentRows = newRows;
            }
        }

        private void TableData_OnChanged(object sender, ParameterTableUpdateEventMessage e)
        {
            var newRows = CalculateNewRows().ToList();
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

            var tagRequest = new GetLiteElementInfo
            {
                DataMinerID = dataminerId,
                ElementID = elementId,
            };

            var response = dms.SendMessage(tagRequest) as LiteElementInfoEvent;
            var tagTableData = _dataProvider.SourceTable.GetData();

            if (response != null)
            {
                GetAllLayoutsTableRows(rows, response, tagTableData);
            }
            else
            {
                CreateDebugRow(rows, "response == null");
            }

            return rows;
        }

        private void GetAllLayoutsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, ParameterValue[] allLayoutsTable)
        {
            for (int i = 0; i < allLayoutsTable[0].ArrayValue.Length; i++)
            {
                var title = allLayoutsTable[2].ArrayValue[i]?.CellValue?.GetAsStringValue() == "0" ? "None" : allLayoutsTable[2].ArrayValue[i]?.CellValue?.GetAsStringValue();
                var channelSource = allLayoutsTable[1].ArrayValue[i]?.CellValue?.GetAsStringValue() == "0" ? "None" : allLayoutsTable[1].ArrayValue[i]?.CellValue?.GetAsStringValue();

                var cells = new[]
                {
                    new GQICell { Value = allLayoutsTable[0].ArrayValue[i]?.CellValue?.GetAsStringValue() }, // Index
                    new GQICell { Value = allLayoutsTable[4].ArrayValue[i]?.CellValue?.GetAsStringValue() }, // Layout
                    new GQICell { Value = allLayoutsTable[5].ArrayValue[i]?.CellValue?.GetAsStringValue() }, // Position
                    new GQICell { Value = channelSource }, // Channel Source
                    new GQICell { Value = title }, // Title
                    new GQICell { Value = allLayoutsTable[3].ArrayValue[i]?.CellValue?.GetAsStringValue() }, // Layout Id
                    new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                };

                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                var row = new GQIRow(allLayoutsTable[0].ArrayValue[i]?.CellValue?.GetAsStringValue(), cells)
                {
                    Metadata = rowMetadata,
                };

                rows.Add(row);
            }
        }

        private void GetTagArgument()
        {
            dataminerId = -1;
            elementId = -1;
            var splittedElementId = _elementId.Split('/');
            var tagRequest = new GetLiteElementInfo
            {
                DataMinerID = Convert.ToInt32(splittedElementId[0]),
                ElementID = Convert.ToInt32(splittedElementId[1]),
            };

            var response = dms.SendMessage(tagRequest) as LiteElementInfoEvent;

            if (response.Protocol.Contains("MCS"))
            {
                _isMcs = true;
            }

            dataminerId = response.DataMinerID;
            elementId = response.ElementID;
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