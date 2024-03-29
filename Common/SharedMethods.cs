﻿namespace SharedMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Analytics.GenericInterface;
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

        public static string GetValueFromStringDictionary(Dictionary<string, string> dict, string dictionaryKey)
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
        public static readonly int ChannelStatusOverview = 240;
        public static readonly int AllChannelsProfile = 8000;
        public static readonly int ChannelEventsOverview = 430;
        public static readonly int CpuUsage = 9401;
        public static readonly int AllocatedMemory = 9402;

        public MCM(IDmsElement element)
        {
            this.element = element;
        }

        public static int ChannelStatusTableId { get => 240; }

        public override int OutputsTableId { get => 1500; }

        public override int Outputs_LayoutsColumnId { get => 1612; }

        public override int LayoutsTableId { get => 1560; }

        public override int OutputsTable_OutputColumnId { get => 1501; }

        public override int AllLayouts_TitleColumnId { get => 10353; }

        public static Dictionary<string, string> ChannelConfigAccessTypeDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","SPTS"},
            {"2","MTPS"},
            {"3","HLS"},
            {"4","RTMP"},
            {"5","2022-6"},
        };

        public static Dictionary<string, string> ChannelConfigServiceTypeDict = new Dictionary<string, string>
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

        public static Dictionary<string, string> ChannelConfigMonitoringLevelDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","Full"},
            {"5","Light"},
            {"10","Extra Light"},
        };

        public static Dictionary<string, string> ChannelConfigSeverityDict = new Dictionary<string, string>
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
    }

    public class MCS : TAG
    {
        public static readonly int ChannelStatusOverview = 5300;
        public static readonly int ChannelsConfiguration = 2100;

        public MCS(IDmsElement element)
        {
            this.element = element;
        }

        public static List<int> ChannelsTableIds { get => new List<int> { 2100, 2200 }; }

        public override int OutputsTableId { get => 3400; }

        public override int Outputs_LayoutsColumnId { get => 3456; }

        public override int LayoutsTableId { get => 3600; }

        public override int OutputsTable_OutputColumnId { get => 3403; }

        public override int AllLayouts_TitleColumnId { get => 5653; }

        public static Dictionary<string, string> ChannelConfigAccessTypeDict = new Dictionary<string, string>
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

        public static Dictionary<string, string> ChannelConfigServiceTypeDict = new Dictionary<string, string>
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

        public static Dictionary<string, string> ChannelConfigRecordingDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"0","Disabled"},
            {"1","Enabled"},
        };

        public static Dictionary<string, string> ChannelConfigMonitoringLevelDict = new Dictionary<string, string>
        {
            {"-1","N/A"},
            {"1","Full"},
            {"2","Light"},
            {"3","Extra Light"},
        };
    }
}