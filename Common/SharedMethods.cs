namespace SharedMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;

    public class SharedMethods
    {
        public static string GetOneDeserializedValue(string scriptParam) // [ "value" , "value" ]
        {
            if (scriptParam.Contains("[") && scriptParam.Contains("]"))
            {
                return JsonConvert.DeserializeObject<List<string>>(scriptParam)[0];
            }
            else
            {
                return scriptParam;
            }
        }
    }

    public abstract class TAG
    {
        public IDmsElement element;

        public abstract int OutputsTableId { get; }

        public abstract int Outputs_LayoutsColumnId { get; }

        public abstract int LayoutsTableId { get; }

        public abstract int OutputsTable_OutputColumnId { get; }

        public abstract int AllLayouts_TitleColumnId { get; }

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
            var outputsLayoutsTable = element.GetTable(OutputsTableId);
            var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = OutputsTable_OutputColumnId, Value = outputId } };
            return outputsLayoutsTable.QueryData(filter).ToList();
        }

        public List<string> GetLayoutsFromElement()
        {
            var layoutsList = new List<string>();

            var tableData = element.GetTable(LayoutsTableId).GetData();
            var layoutsToAdd = tableData.Values.Select(row => Convert.ToString(row[1 /* Title */])).ToList();
            layoutsList.AddRange(layoutsToAdd);

            layoutsList.Sort();
            return layoutsList.Distinct().ToList();
        }
    }

    public class MCM : TAG
    {
        public MCM(IDmsElement element)
        {
            this.element = element;
        }

        public static int ChannelStatusTableId { get => 240; }

        public static int AllLayoutsTable_TitlePid { get => 10353; }

        public override int OutputsTableId { get => 1500; }

        public override int Outputs_LayoutsColumnId { get => 1612; }

        public override int LayoutsTableId { get => 1560; }

        public override int OutputsTable_OutputColumnId { get => 1501; }

        public override int AllLayouts_TitleColumnId { get => 10353; }
    }

    public class MCS : TAG
    {
        public MCS(IDmsElement element)
        {
            this.element = element;
        }

        public static List<int> ChannelsTableIds { get => new List<int> { 2100, 2200 }; }

        public static int AllLayoutsTable_TitlePid { get => 5653; }

        public override int OutputsTableId { get => 3400; }

        public override int Outputs_LayoutsColumnId { get => 3456; }

        public override int LayoutsTableId { get => 3600; }

        public override int OutputsTable_OutputColumnId { get => 3403; }

        public override int AllLayouts_TitleColumnId { get => 5653; }
    }
}
