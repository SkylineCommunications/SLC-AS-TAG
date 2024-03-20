namespace SharedMethods
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Analytics.GenericInterface;
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

    public class MCM_TablesIDs
    {
        public static readonly int ChannelStatusTableId = 240;
        public static readonly int EncoderConfigTableId = 1500;
        public static readonly int LayoutsTableId = 1560;
        public static readonly int EncoderConfigLayoutsColumnId = 1612;
        public static readonly int AllLayoutsTable_TitlePid = 10353;
    }

    public class MCS_TablesIDs
    {
        public static readonly int OutputsLayoutsTableId = 3400;
        public static readonly int OutputsLayoutsLayoutColumnId = 3456;
        public static readonly int LayoutsTableIds = 3600;
        public static readonly int AllLayoutsTable_TitlePid = 5653;

        private static readonly List<int> ChTableIds = new List<int> { 2100, 2200 };

        public static List<int> ChannelsTableIds => ChTableIds;
    }
}
