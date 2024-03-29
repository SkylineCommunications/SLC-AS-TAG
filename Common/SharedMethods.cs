namespace SharedMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Net.Messages;

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

        public static object[][] GetTable(GQIDMS _dms, LiteElementInfoEvent response, int tableId)
        {
            var partialTableRequest = new GetPartialTableMessage
            {
                DataMinerID = response.DataMinerID,
                ElementID = response.ElementID,
                ParameterID = tableId,
            };

            var messageResponse = _dms.SendMessage(partialTableRequest) as ParameterChangeEventMessage;
            if (messageResponse.NewValue.ArrayValue != null && messageResponse.NewValue.ArrayValue.Length > 0)
            {
                return BuildRows(messageResponse.NewValue.ArrayValue);
            }
            else
            {
                return new object[0][];
            }
        }

        private static object[][] BuildRows(ParameterValue[] columns)
        {
            int length1 = columns.Length;
            int length2 = 0;
            if (length1 > 0)
                length2 = columns[0].ArrayValue.Length;
            object[][] objArray;
            if (length1 > 0 && length2 > 0)
            {
                objArray = new object[length2][];
                for (int index = 0; index < length2; ++index)
                    objArray[index] = new object[length1];
            }
            else
            {
                objArray = new object[0][];
            }

            for (int index1 = 0; index1 < length1; ++index1)
            {
                ParameterValue[] arrayValue = columns[index1].ArrayValue;
                for (int index2 = 0; index2 < length2; ++index2)
                    objArray[index2][index1] = arrayValue[index2].IsEmpty ? (object)null : arrayValue[index2].ArrayValue[0].InteropValue;
            }

            return objArray;
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

    public class UmdEditor : TAG
    {
        public UmdEditor(IEngine engine, IDms dms, string elementId, string selectedLayout, string titleIndex)
        {
            TagElement = dms.GetElement(new DmsElementId(elementId));
            SelectedLayout = selectedLayout;
            TitleIndex = titleIndex;

            isMCS = TagElement.Protocol.Name.Contains("MCS");

            if (!isMCS)
            {
                var splittedId = elementId.Split('/');
                var dmaId = Convert.ToInt32(splittedId[0]);
                var element = Convert.ToInt32(splittedId[1]);
                TagEngineElement = engine.FindElement(dmaId, element);
            }
        }

        public enum TagMcs
        {
            LayoutsTable = 5000,
            Umd1Read = 5005,
            Umd2Read = 5006,
            Umd3Read = 5007,
            Umd4Read = 5008,
            Umd1Write = 5025,
            Umd2Write = 5026,
            Umd3Write = 5027,
            Umd4Write = 5028,
        }

        public enum TagMcm
        {
            Umd1Idx = 5,
            Umd2Idx = 6,
            Umd1Read = 2806,
            Umd2Read = 2807,
            TallyLayouts = 2800,
        }

        public IDmsElement TagElement { get; set; }

        public Element TagEngineElement { get; set; }

        public string SelectedLayout { get; set; }

        public string TitleIndex { get; set; }

        public bool isMCS { get; set; }

        public override int OutputsTableId => throw new NotImplementedException();

        public override int Outputs_LayoutsColumnId => throw new NotImplementedException();

        public override int LayoutsTableId => throw new NotImplementedException();

        public override int OutputsTable_OutputColumnId => throw new NotImplementedException();

        public override int AllLayouts_TitleColumnId => throw new NotImplementedException();
    }
}
