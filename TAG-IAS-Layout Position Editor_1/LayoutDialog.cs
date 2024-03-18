namespace TAG_IAS_Layout_Position_Editor_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private const int MCMChannelStatusTableId = 240;
        private static readonly List<int> MCSChannelsTableIds = new List<int> { 2100, 2200 };

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

        public void GetLayoutsFromElement(IDmsElement element)
        {
            var channelsList = new List<string> { "< None >" };

            if (element.Protocol.Name.Contains("MCM"))
            {
                var tableData = element.GetTable(MCMChannelStatusTableId).GetData();
                var channelsToAdd = tableData.Values
                    .Where(row => Convert.ToInt32(row[14 /* Monitored */]) == (int)Monitored.Yes)
                    .Select(row => Convert.ToString(row[12 /* Name */])).ToList();
                channelsList.AddRange(channelsToAdd);
            }
            else
            {
                foreach (var tableId in MCSChannelsTableIds)
                {
                    var channelsTableData = element.GetTable(tableId).GetData();
                    var channelsToAdd = channelsTableData.Values
                        .Where(row => !Convert.ToString(row[6 /* Device */]).Equals("Not Set"))
                        .Select(row => Convert.ToString(row[1 /* Label */])).ToList();
                    channelsList.AddRange(channelsToAdd);
                }
            }

            channelsList.Sort();
            var distinctValues = channelsList.Distinct();
            this.ChannelsDropDown.Options = distinctValues;
            this.ChannelsDropDown.Selected = String.Empty;
        }
    }
}