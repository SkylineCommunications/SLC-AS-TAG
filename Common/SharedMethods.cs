namespace SharedMethods
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

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

    public class MCM_TablesIDs
    {
        public static readonly int ChannelStatusTableId = 240;
        public static readonly int EncoderConfigTableId = 1500;
        public static readonly int LayoutsTableId = 1560;
        public static readonly int AllLayoutsTable_TitlePid = 10353;
    }

    public class MCS_TablesIDs
    {
        public static readonly int OutputsLayoutsTableId = 3400;
        public static readonly int LayoutsTableIds = 3600;
        public static readonly int AllLayoutsTable_TitlePid = 5653;

        private static readonly List<int> ChTableIds = new List<int> { 2100, 2200 };

        public static List<int> ChannelsTableIds => ChTableIds;
    }
}
