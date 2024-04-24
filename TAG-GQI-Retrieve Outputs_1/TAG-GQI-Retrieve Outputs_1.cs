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

namespace TAG_GQI_Retrieve_Outputs_1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Common.StaticData;
	using SharedMethods;
	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    [GQIMetaData(Name = "Get TAG All Outputs")]
    public class GetTagOutputs : IGQIDataSource, IGQIOnInit, IGQIInputArguments
    {
        private readonly GQIBooleanArgument individualRowsLayout = new GQIBooleanArgument("Individual Rows Per Layout") { IsRequired = true };
        private bool isIndividualRowsLayout;

        private GQIDMS _dms;

        public enum McsTableId
        {
            DeviceOutputConfig = 3100,
            OutputLayouts = 3400,
        }

        public enum McmTableId
        {
            DeviceEncoderConfig = 1500,
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
                new GQIStringColumn("Element ID"),
                new GQIStringColumn("Device"),
                new GQIStringColumn("Output ID"),
                new GQIStringColumn("Output"),
                new GQIStringColumn("Resolution"),
                new GQIStringColumn("Frame Rate"),
                new GQIStringColumn("Layout"),
                new GQIStringColumn("Layout ID"),
                new GQIStringColumn("Device IP"),
            };
        }

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { individualRowsLayout };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            isIndividualRowsLayout = args.GetArgumentValue(individualRowsLayout);

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
                    var outputConfigTable = SharedMethods.GetTable(_dms, response, (int)McsTableId.DeviceOutputConfig);
                    GetOutputMcsTableRows(rows, response, outputConfigTable, mcsStaticData);
                }

                // if no MCS in the system, gather MCM data
                if (rows.Count == 0)
                {
                    var mcmResponses = _dms.SendMessages(new DMSMessage[] { mcmRequest });
                    var mcmStaticData = new MCM_StaticData();
                    foreach (var response in mcmResponses.Select(x => (LiteElementInfoEvent)x))
                    {
                        var encoderConfigTable = SharedMethods.GetTable(_dms, response, (int)McmTableId.DeviceEncoderConfig);
                        GetOutputMcmTableRows(rows, response, encoderConfigTable, mcmStaticData);
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

        private void GetOutputMcsTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] outputConfigTable, IStaticData staticData)
        {
            var outputLayoutsTable = SharedMethods.GetTable(_dms, response, (int)McsTableId.OutputLayouts);

            for (int i = 0; i < outputConfigTable.Length; i++)
            {
                var deviceOutputConfigRow = outputConfigTable[i];
                var outputName = Convert.ToString(deviceOutputConfigRow[1]);
                var layoutsInOutput = GetLayoutInOutput(outputLayoutsTable, outputName);

                GQICell[] cells = null;

                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                if (!isIndividualRowsLayout)
                {
                    var layoutId = String.Join(";", layoutsInOutput.Select(x => x.LayoutId));
                    var layoutName = String.Join(";", layoutsInOutput.Select(x => x.LayoutName));

                    cells = new[]
                    {
                        new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                        new GQICell { Value = Convert.ToString(deviceOutputConfigRow[3]).Equals("Not Set") ? "N/A" : Convert.ToString(deviceOutputConfigRow[3])}, // Device
                        new GQICell { Value = Convert.ToString(deviceOutputConfigRow[0]) }, // Output ID
                        new GQICell { Value = outputName }, // Output
                        new GQICell { Value = staticData.ResolutionDict[Convert.ToString(deviceOutputConfigRow[9])] }, // Resolution
                        new GQICell { Value = staticData.FrameRateDict[Convert.ToString(deviceOutputConfigRow[8])] }, // Frame Rate
                        new GQICell { Value = layoutName }, // Layout
                        new GQICell { Value = layoutId }, // Layout ID
                        new GQICell { Value = response.PollingIP }, // Device IP
                    };

                    var row = new GQIRow(cells)
                    {
                        Metadata = rowMetadata,
                    };

                    rows.Add(row);
                }
                else
                {
                    foreach (var layout in layoutsInOutput)
                    {
                        cells = new[]
                        {
                            new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                            new GQICell { Value = Convert.ToString(deviceOutputConfigRow[3]).Equals("Not Set") ? "N/A" : Convert.ToString(deviceOutputConfigRow[3])}, // Device
                            new GQICell { Value = Convert.ToString(deviceOutputConfigRow[0]) }, // Output ID
                            new GQICell { Value = outputName }, // Output
                            new GQICell { Value = staticData.ResolutionDict[Convert.ToString(deviceOutputConfigRow[9])] }, // Resolution
                            new GQICell { Value = staticData.FrameRateDict[Convert.ToString(deviceOutputConfigRow[8])] }, // Frame Rate
                            new GQICell { Value = layout.LayoutName }, // Layout
                            new GQICell { Value = layout.LayoutId }, // Layout ID
                            new GQICell { Value = response.PollingIP }, // Device IP
                        };

                        var row = new GQIRow(cells)
                        {
                            Metadata = rowMetadata,
                        };

                        rows.Add(row);
                    }
                }
            }
        }

        private List<Layout> GetLayoutInOutput(object[][] outputLayoutsTable, string outputName)
        {
            var layoutsInOutput = new List<Layout>();
            for (int i = 0; i < outputLayoutsTable.Length; i++)
            {
                var outputLayoutRow = outputLayoutsTable[i];
                if (Convert.ToString(outputLayoutRow[3 /*Output*/]).Equals(outputName))
                {
                    layoutsInOutput.Add(new Layout { LayoutId = Convert.ToString(outputLayoutRow[4]), LayoutName = Convert.ToString(outputLayoutRow[5]) });
                }
            }

            return layoutsInOutput;
        }

        private void GetOutputMcmTableRows(List<GQIRow> rows, LiteElementInfoEvent response, object[][] encoderConfigTable, IStaticData staticData)
        {
            var deviceOverviewTable = SharedMethods.GetTable(_dms, response, (int)McmTableId.DeviceOverview);
            var deviceRows = deviceOverviewTable.Where(x => !Convert.ToString(x[0]).Equals("Cloud License"));

            for (int i = 0; i < encoderConfigTable.Length; i++)
            {
                var deviceEncoderConfigRow = encoderConfigTable[i];
                var filteredDeviceRow = deviceRows.First(x => Convert.ToString(x[4]).Equals(Convert.ToString(deviceEncoderConfigRow[16])));
                var deviceName = string.Empty;
                deviceName = filteredDeviceRow == null ? "N/A" : Convert.ToString(filteredDeviceRow[0]);

                var elementID = new ElementID(response.DataMinerID, response.ElementID);
                var elementMetadata = new ObjectRefMetadata { Object = elementID };
                var rowMetadata = new GenIfRowMetadata(new[] { elementMetadata });

                GQICell[] cells = null;
                if (!isIndividualRowsLayout)
                {
                    cells = new[]
                    {
                        new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                        new GQICell { Value = Convert.ToString(deviceName) }, // Device
                        new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[0]) }, // Output ID
                        new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[1]) }, // Output
                        new GQICell { Value = staticData.ResolutionDict[Convert.ToString(deviceEncoderConfigRow[4])] }, // Resolution
                        new GQICell { Value = staticData.FrameRateDict[Convert.ToString(deviceEncoderConfigRow[9])] }, // Frame Rate
                        new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[11]) }, // Layout
                        new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[17]) }, // Layout ID
                        new GQICell { Value = response.PollingIP }, // Device IP
                    };

                    var row = new GQIRow(cells)
                    {
                        Metadata = rowMetadata,
                    };

                    rows.Add(row);
                }
                else
                {
                    var layoutNameList = Convert.ToString(deviceEncoderConfigRow[11]).Split(';').ToList();
                    var layoutIdList = Convert.ToString(deviceEncoderConfigRow[17]).Split(';').ToList();

                    for (int j = 0; j < layoutIdList.Count; j++)
                    {
                        cells = new[]
                        {
                            new GQICell { Value = Convert.ToString($"{response.DataMinerID}/{response.ElementID}") }, // Element ID
                            new GQICell { Value = Convert.ToString(deviceName) }, // Device
                            new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[0]) }, // Output ID
                            new GQICell { Value = Convert.ToString(deviceEncoderConfigRow[1]) }, // Output
                            new GQICell { Value = staticData.ResolutionDict[Convert.ToString(deviceEncoderConfigRow[4])] }, // Resolution
                            new GQICell { Value = staticData.FrameRateDict[Convert.ToString(deviceEncoderConfigRow[9])] }, // Frame Rate
                            new GQICell { Value = Convert.ToString(layoutNameList[j]) }, // Layout
                            new GQICell { Value = Convert.ToString(layoutIdList[j]) }, // Layout ID
                            new GQICell { Value = response.PollingIP }, // Device IP
                        };

                        var row = new GQIRow(cells)
                        {
                            Metadata = rowMetadata,
                        };

                        rows.Add(row);
                    }
                }
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
                new GQICell { Value = null},
                new GQICell { Value = null},
                new GQICell { Value = null },
                new GQICell { Value = null },
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

    public class Layout
    {
        public string LayoutId { get; set; }

        public string LayoutName { get; set; }
    }
}