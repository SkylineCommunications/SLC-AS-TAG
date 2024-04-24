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

dd/mm/2024	1.0.0.1		BSM, Skyline	Initial version
****************************************************************************
*/

namespace TagLibrary_1
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Common.StaticData;
    using Common.TableClasses;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    public class TAG
    {
        public IStaticData StaticInfo { get; set; }

        private IDmsElement element;

        protected TAG(IDmsElement idmsElement)
        {
            Element = idmsElement;
        }

        public IDmsElement Element { get => element; set => element = value; }

        public static string GetElementType(string protocolName)
        {
            return protocolName.Contains("MCM") ? "MCM" : "MCS";
        }

        public static TAG GetDeviceByType(IDmsElement element, string elementType)
        {
            var deviceByName = new Dictionary<string, TAG>
            {
                { "MCM", new MCM(element) },
                { "MCS", new MCS(element) },
            };

            return deviceByName[elementType];
        }

        public List<object[]> GetLayoutsByOutput(string outputId)
        {
            var outputsLayoutsTable = Element.GetTable(StaticInfo.Outputs.TableId);
            var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = StaticInfo.Outputs.OutputColumnId, Value = outputId } };
            return outputsLayoutsTable.QueryData(filter).ToList();
        }

        public List<string> GetLayoutsFromElement()
        {
            var layoutsList = new List<string>();

            var tableData = Element.GetTable(StaticInfo.LayoutsTableId).GetData();
            var layoutsToAdd = tableData.Values.Select(row => Convert.ToString(row[1 /* Title */])).ToList();
            layoutsList.AddRange(layoutsToAdd);

            layoutsList.Sort();
            return layoutsList.Distinct().ToList();
        }

        public string GetChannelById(string channelId)
        {
            var tableData = Element.GetTable(StaticInfo.AllChannelsProfileIds.ProfileId);
            var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = StaticInfo.AllChannelsProfileIds.ChannelId_Pid, Value = channelId } };
            var matchingChannels = tableData.QueryData(filter).ToList();

            if (!matchingChannels.Any())
            {
                return "N/A";
            }

            var matchingChannel = matchingChannels.First();
            return Convert.ToString(matchingChannel[StaticInfo.AllChannelsProfileIds.ChannelTitle_Idx]);
        }

        public Dictionary<string, AllLayoutRowValues> GetPositionsAndChannelsInLayout(string layoutName, IEngine engine)
        {
            var positionChannelDict = new Dictionary<string, AllLayoutRowValues>();
            var allLayoutsTable = Element.GetTable(StaticInfo.AllLayouts.TableId);
            var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = StaticInfo.AllLayouts.LayoutName_Pid, Value = layoutName } };
            var allLayoutsTableRows = allLayoutsTable.QueryData(filter);

            if (!allLayoutsTableRows.Any())
            {
                return positionChannelDict;
            }

            foreach (var row in allLayoutsTableRows)
            {
                var primaryKey = Convert.ToString(row[0]);
                var layoutPosition = Convert.ToString(row[StaticInfo.AllLayouts.Position_Idx]);
                var channelName = Convert.ToString(row[StaticInfo.AllLayouts.ChannelTitle_Idx]);
                positionChannelDict[primaryKey] = new AllLayoutRowValues { Index = primaryKey, ChannelTitle = channelName, Position = layoutPosition };
            }

            return positionChannelDict;
        }
    }

    public class MCM : TAG
    {
        public MCM(IDmsElement element) : base(element)
        {
            StaticInfo = new MCM_StaticData();
        }
    }

    public class MCS : TAG
    {
        public MCS(IDmsElement element) : base(element)
        {
            StaticInfo = new MCS_StaticData();
        }
    }
}