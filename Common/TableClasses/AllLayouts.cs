namespace Common.TableClasses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class AllLayouts
    {
        public AllLayouts(int allLayoutsTableId, int layoutName_Pid, int position_Idx, int channelTitle_Idx, int titleColumnPid)
        {
            TableId = allLayoutsTableId;
            LayoutName_Pid = layoutName_Pid;
            Position_Idx = position_Idx;
            ChannelTitle_Idx = channelTitle_Idx;
            TitleColumnPid = titleColumnPid;
        }

        public int TableId { get; }

        public int LayoutName_Pid { get; }

        public int Position_Idx { get; }

        public int ChannelTitle_Idx { get; }

        public int TitleColumnPid { get; }
    }
}
