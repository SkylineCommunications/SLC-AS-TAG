namespace Common.TableClasses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AllChannelsProfileIds
    {
        public AllChannelsProfileIds(int profileID, int channelIdPid, int channelTitleIdx)
        {
            ProfileId = profileID;
            ChannelId_Pid = channelIdPid;
            ChannelTitle_Idx = channelTitleIdx;
        }

        public int ProfileId { get; }

        public int ChannelId_Pid { get; }

        public int ChannelTitle_Idx { get; }
    }
}
