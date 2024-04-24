namespace Common.StaticData
{
    using Common.TableClasses;
    using System;
    using System.Collections.Generic;

    public class MCS_StaticData : IStaticData
    {
        public int ChannelStatusOverview { get; } = 5300;

        public int ChannelsConfiguration { get; } = 2100;

        public int ChannelEventsOverview { get; } = 5100;

        public IReadOnlyList<int> ChannelsTableIds { get; }
            = new List<int> { 2100, 2200 };

        public int LayoutsTableId { get; } = 3600;

        public int CPU_Pid { get; } = 9401;

        public int Memory_Pid { get; } = 9401;

        public IReadOnlyDictionary<string, string> ChannelConfigAccessTypeDict { get; }
            = new Dictionary<string, string>
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

        public IReadOnlyDictionary<string, string> ChannelConfigServiceTypeDict { get; }
            = new Dictionary<string, string>
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

        public IReadOnlyDictionary<string, string> ChannelConfigRecordingDict { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"0","Disabled"},
                {"1","Enabled"},
            };

        public IReadOnlyDictionary<string, string> ChannelConfigMonitoringLevelDict { get; }
            = new Dictionary<string, string>
            {
                {"-1","N/A"},
                {"1","Full"},
                {"2","Light"},
                {"3","Extra Light"},
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
                {"0","1920x1080"},
                {"1","1080x1920 (90deg)"},
                {"2","1280x720"},
                {"3","800x488"},
                {"4","640x480"},
                {"5","640x360"},
                {"6","3840x2160"},
                {"7","2160x3840 (90deg)"},
            };

        public IReadOnlyDictionary<string, string> FrameRateDict { get; }
            = new Dictionary<string, string>
            {
                {"1","1/25 fps"},
                {"2","2/25 fps"},
                {"3","2.5/25 fps"},
                {"4","5/25 fps"},
                {"5","12.5/25 fps"},
                {"6","23.976 fps"},
                {"7","24 fps"},
                {"8","25 fps"},
                {"9","29.97 fps"},
                {"10","30 fps"},
                {"11","50 fps"},
                {"12","59.94 fps"},
                {"13","60 fps"},
            };

        public AllChannelsProfileIds AllChannelsProfileIds => new AllChannelsProfileIds(profileID: 2400, channelIdPid: 2403, channelTitleIdx: 3);

        public AllLayouts AllLayouts => new AllLayouts(allLayoutsTableId: 5600, position_Idx: 5, channelTitle_Idx: 2, layoutName_Pid: 5605, titleColumnPid: 5653);

        public Outputs Outputs => new Outputs(outputsTableId: 3400, outputs_LayoutsColumnId: 3456, outputsTable_OutputColumnId: 3403);

        public IReadOnlyDictionary<string, string> ChannelConfigSeverityDict => throw new NotImplementedException();
    }
}
