namespace Common.StaticData
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Common.TableClasses;

    public class MCM_StaticData : IStaticData
    {
        public int ChannelStatusOverview { get; } = 240;

        public int AllChannelsProfile { get; } = 8000;

        public int ChannelEventsOverview { get; } = 430;

        public int CpuUsage { get; } = 9401;

        public int AllocatedMemory { get; } = 9402;

        public int LayoutsTableId { get; } = 1560;

        public int CPU_Pid { get; } = 9401;

        public int Memory_Pid { get; } = 9401;

        public IReadOnlyDictionary<string, string> ChannelConfigAccessTypeDict { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"1","SPTS"},
                {"2","MTPS"},
                {"3","HLS"},
                {"4","RTMP"},
                {"5","2022-6"},
            };

        public IReadOnlyDictionary<string, string> ChannelConfigServiceTypeDict { get; }
            = new Dictionary<string, string>
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

        public IReadOnlyDictionary<string, string> ChannelConfigMonitoringLevelDict { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"1","Full"},
                {"5","Light"},
                {"10","Extra Light"},
            };

        public IReadOnlyDictionary<string, string> ChannelConfigSeverityDict { get; }
            = new Dictionary<string, string>
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

        public IReadOnlyDictionary<string, string> ChannelEventsStatus { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"0","Not Active"},
                {"1","Active"},
            };

        public IReadOnlyDictionary<string, string> ChannelEventsAcknowledge { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"0","No"},
                {"1","Yes"},
            };

        public IReadOnlyDictionary<string, string> ResolutionDict { get; }
            = new Dictionary<string, string>
            {
                {"1","1920x1080px"},
                {"2","1080x1920px"},
                {"3","1280x720px"},
                {"4","800x488px"},
                {"5","640x480px"},
                {"6","640x360px"},
            };

        public IReadOnlyDictionary<string, string> FrameRateDict { get; }
            = new Dictionary<string, string>
            {
                {"1","25 fps"},
                {"2","12.5/25 fps"},
                {"3","5/25 fps"},
                {"4","2.5/25 fps"},
                {"5","2/25 fps"},
                {"6","1/25 fps"},
                {"7","23.976 fps"},
                {"8","24 fps"},
                {"9","29.97 fps"},
                {"10","30 fps"},
            };

        public AllChannelsProfileIds AllChannelsProfileIds => new AllChannelsProfileIds(profileID: 8000, channelIdPid: 8001, channelTitleIdx: 9);

        public AllLayouts AllLayouts => new AllLayouts(allLayoutsTableId: 10300, position_Idx: 5, channelTitle_Idx: 2, layoutName_Pid: 10305, titleColumnPid: 10353);

        public Outputs Outputs => new Outputs(outputsTableId: 1500, outputs_LayoutsColumnId: 1612, outputsTable_OutputColumnId: 1501);

        public IReadOnlyDictionary<string, string> ChannelConfigRecordingDict => throw new NotSupportedException();

        public IReadOnlyList<int> ChannelsTableIds => throw new NotSupportedException();
    }
}
