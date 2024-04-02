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

        public static string GetParameter(GQIDMS _dms, LiteElementInfoEvent response, int parameterId)
        {
            var parameterRequest = new GetParameterMessage(response.DataMinerID, response.ElementID, parameterId);
            var messageResponse = _dms.SendMessage(parameterRequest) as GetParameterResponseMessage;
            return messageResponse.DisplayValue;
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

        public static string GetValueFromStringDictionary(IReadOnlyDictionary<string, string> dict, string dictionaryKey)
        {
            if (dict.TryGetValue(dictionaryKey, out var value))
            {
                return value;
            }
            else
            {
                return "N/A";
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

    public class TAG
    {
        private readonly int outputsTableId;
        private readonly int outputs_LayoutsColumnId;
        private readonly int layoutsTableId;
        private readonly int outputsTable_OutputColumnId;
        private readonly int allLayouts_TitleColumnId;

        public IDmsElement Element;

        protected TAG(
            int constOutputsTableId,
            int outputsLayoutsColumnId,
            int constLayoutsTableId,
            int outputsTableOutputColumnId,
            int allLayoutsTitleColumnId,
            IDmsElement element)
        {
            outputsTableId = constOutputsTableId;
            outputs_LayoutsColumnId = outputsLayoutsColumnId;
            layoutsTableId = constLayoutsTableId;
            outputsTable_OutputColumnId = outputsTableOutputColumnId;
            allLayouts_TitleColumnId = allLayoutsTitleColumnId;
            Element = element;
        }

        public List<string> LayoutsFromElement
        {
            get
            {
                var layoutsList = new List<string>();
                var tableData = Element.GetTable(layoutsTableId).GetData();
                var layoutsToAdd = tableData.Values.Select(row => Convert.ToString(row[1 /* Title */])).ToList();
                layoutsList.AddRange(layoutsToAdd);
                layoutsList.Sort();
                var distinctList = layoutsList.Distinct().ToList();
                return distinctList;
            }
        }

        public int AllLayouts_TitleColumnId { get; internal set; }

        public int Outputs_LayoutsColumnId { get; internal set; }

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
            var outputsLayoutsTable = Element.GetTable(outputsTableId);
            var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = outputsTable_OutputColumnId, Value = outputId } };
            return outputsLayoutsTable.QueryData(filter).ToList();
        }
    }

    public class MCM : TAG
    {
        public static readonly int ChannelStatusOverview = 240;
        public static readonly int AllChannelsProfile = 8000;
        public static readonly int ChannelEventsOverview = 430;
        public static readonly int CpuUsage = 9401;
        public static readonly int AllocatedMemory = 9402;
        public static readonly int ChannelStatusTableId = 240;
        public static readonly int OutputsTableId = 1500;
        public static new readonly int Outputs_LayoutsColumnId = 1612;
        public static readonly int LayoutsTableId = 1560;
        public static readonly int OutputsTable_OutputColumnId = 1501;
        public static new readonly int AllLayouts_TitleColumnId = 10353;

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigAccessTypeDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","SPTS"},
            {"2","MTPS"},
            {"3","HLS"},
            {"4","RTMP"},
            {"5","2022-6"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigServiceTypeDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","SD Video"},
            {"2","HD Video"},
            {"3","Audio"},
            {"4","Data"},
            {"5","Low Res Video"},
            {"6","Contribution Feed"},
            {"7","Wide SD"},
            {"8","SD/HEVC"},
            {"9","HD/HEVC"},
            {"10","UHD/HEVC"},
            {"11","HD/50/60/HEVC"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigMonitoringLevelDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","Full"},
            {"5","Light"},
            {"10","Extra Light"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigSeverityDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"0","Clear"},
            {"1","Critical"},
            {"2","Major"},
            {"3","Minor"},
            {"4","Warning"},
            {"5","Notice"},
            {"6","Info"},
            {"10","None"},
            {"500","Lost Connection to Device"},
            {"501","No Network Interface"},
            {"502","Not Monitored"},
        };

        public MCM(IDmsElement element) : base(
        constOutputsTableId: 1500,
        outputsLayoutsColumnId: 1612,
        constLayoutsTableId: 1560,
        outputsTableOutputColumnId: 1501,
        allLayoutsTitleColumnId: 10353,
        element: element)
        {
        }
    }

    public class MCS : TAG
    {
        public static readonly int ChannelStatusOverview = 5300;
        public static readonly int ChannelsConfiguration = 2100;
        public static readonly IReadOnlyList<int> ChannelsTableIds = new List<int> { 2100, 2200 };
        public static readonly int OutputsTableId = 3400;
        public static new readonly int Outputs_LayoutsColumnId = 3456;
        public static readonly int LayoutsTableId = 3600;
        public static readonly int OutputsTable_OutputColumnId = 3403;
        public static new readonly int AllLayouts_TitleColumnId = 5653;

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigAccessTypeDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","MPEG-TS"},
            {"2","HLS"},
            {"3","MPEG-DASH"},
            {"4","MSS"},
            {"5","RTMP"},
            {"6","2022-6"},
            {"7","2110"},
            {"8","ZIXI"},
            {"9","NDI"},
            {"10","CDI"},
            {"11","SRT"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigServiceTypeDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","OTT Parent"},
            {"2","SD Video"},
            {"3","HD Video"},
            {"4","Audio"},
            {"5","Data"},
            {"6","Low Res Video"},
            {"7","Contribution Feed"},
            {"8","Wide SD"},
            {"9","SD/HEVC"},
            {"10","HD/25/30/HEVC"},
            {"11","UHD/HEVC"},
            {"12","HD/50/60/HEVC/J2K"},
            {"13","Contribution UHD/HEVC"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigRecordingDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"0","Disabled"},
            {"1","Enabled"},
        };

        public static readonly IReadOnlyDictionary<string, string> ChannelConfigMonitoringLevelDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","Full"},
            {"2","Light"},
            {"3","Extra Light"},
        };

        public MCS(IDmsElement element) : base(
        constOutputsTableId: 3400,
        outputsLayoutsColumnId: 3456,
        constLayoutsTableId: 3600,
        outputsTableOutputColumnId: 3403,
        allLayoutsTitleColumnId: 5653,
        element: element)
        {
        }
    }

    public class UmdEditor
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
    }
}
