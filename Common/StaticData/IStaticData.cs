namespace Common.StaticData
{
    using Common.TableClasses;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IStaticData
    {
        int ChannelStatusOverview { get; }

        int ChannelEventsOverview { get; }

        int LayoutsTableId { get; }

        int CPU_Pid { get; }

        int Memory_Pid { get; }

        AllChannelsProfileIds AllChannelsProfileIds { get; }

        AllLayouts AllLayouts { get; }

        Outputs Outputs { get; }

        IReadOnlyList<int> ChannelsTableIds { get; }

        IReadOnlyDictionary<string, string> ChannelConfigAccessTypeDict { get; }

        IReadOnlyDictionary<string, string> ChannelConfigServiceTypeDict { get; }

        IReadOnlyDictionary<string, string> ChannelConfigMonitoringLevelDict { get; }

        IReadOnlyDictionary<string, string> ChannelConfigSeverityDict { get; }

        IReadOnlyDictionary<string, string> ChannelEventsStatus { get; }

        IReadOnlyDictionary<string, string> ChannelEventsAcknowledge { get; }

        IReadOnlyDictionary<string, string> ChannelConfigRecordingDict { get; }

        IReadOnlyDictionary<string, string> ResolutionDict { get; }

        IReadOnlyDictionary<string, string> FrameRateDict { get; }
    }
}
