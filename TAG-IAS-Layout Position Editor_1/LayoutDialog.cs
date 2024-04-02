namespace TAG_IAS_Layout_Position_Editor_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharedMethods;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public enum Monitored
    {
        No = 0,
        Yes = 1,
    }

    internal class LayoutDialog : Dialog
    {
        public LayoutDialog(IEngine engine) : base(engine)
        {
            Title = "Edit Layout Position";

            Label = new Label("Select a Channel:");
            ChannelsDropDown = new DropDown();
            UpdateButton = new Button("Update");
            CancelButton = new Button("Cancel");

            AddWidget(Label, 0, 0);
            AddWidget(ChannelsDropDown, 0, 1, 1, 2);
            AddWidget(UpdateButton, 1, 1, HorizontalAlignment.Right);
            AddWidget(CancelButton, 1, 2, HorizontalAlignment.Left);

            Label.Width = 150;
            ChannelsDropDown.Width = 400;
            UpdateButton.Width = 110;
            CancelButton.Width = 110;

            Label.Style = TextStyle.None;
        }

        public Label Label { get; private set; }

        public DropDown ChannelsDropDown { get; private set; }

        public Button UpdateButton { get; private set; }

        public Button CancelButton { get; private set; }

        public void GetLayoutsFromElement(IDmsElement element, string elementType)
        {
            var channelsList = new List<string> { "< None >" };

            if (elementType.Equals("MCM"))
            {
                var channelsTableData = element.GetTable(Mcm.ChannelStatusTableId);
                var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.Equal, Pid = 256, Value = Convert.ToString((int)Monitored.Yes) } };
                var matchedChannels = channelsTableData.QueryData(filter).ToList();
                channelsList.AddRange(matchedChannels.Select(row => Convert.ToString(row[12 /* Name */])));
            }
            else
            {
                foreach (var tableId in Mcs.ChannelsTableIds)
                {
                    var channelsTableData = element.GetTable(tableId);
                    var filter = new List<ColumnFilter> { new ColumnFilter { ComparisonOperator = ComparisonOperator.NotEqual, Pid = 2107, Value = "Not Set" } };
                    var matchedChannels = channelsTableData.QueryData(filter).ToList();
                    channelsList.AddRange(matchedChannels.Select(row => Convert.ToString(row[1 /* Label */])));
                }
            }

            channelsList.Sort();
            var distinctValues = channelsList.Distinct();
            this.ChannelsDropDown.Options = distinctValues;
            this.ChannelsDropDown.Selected = String.Empty;
        }
    }
}