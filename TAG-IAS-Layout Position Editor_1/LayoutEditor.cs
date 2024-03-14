namespace TAG_IAS_Layout_Position_Editor_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface.QueryBuilder;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public enum Monitored
    {
        No = 0,
        Yes = 1,
    }

    internal class LayoutEditor : Dialog
    {
        private const int MCMChannelStatusTableId = 240;
        private static readonly List<int> MCSChannelsTableIds = new List<int> { 2100, 2200 };

        public LayoutEditor(IEngine engine) : base(engine)
        {
            Title = "Edit Layout Position";

            Label = new Label("Select a Channel:");
            ChannelsDropDown = new DropDown();
            UpdateButton = new Button("Update");
            CancelButton = new Button("Cancel");

            AddWidget(Label, 0, 0);
            AddWidget(ChannelsDropDown, 0, 1);
            AddWidget(UpdateButton, 1, 1, HorizontalAlignment.Right);
            AddWidget(CancelButton, 2, 1, HorizontalAlignment.Right);

            Label.Width = 150;
            ChannelsDropDown.Width = 400;
            UpdateButton.Width = 100;
            CancelButton.Width = 100;

            Label.Style = TextStyle.Heading;
        }

        public Label Label { get; private set; }

        public DropDown ChannelsDropDown { get; private set; }

        public Button UpdateButton { get; private set; }

        public Button CancelButton { get; private set; }

        public void GetLayoutsFromElement(IDmsElement element)
        {
            var channelsList = new List<string> { "None", "Reserved" };

            if (element.Protocol.Name.Contains("MCM"))
            {
                var tableData = element.GetTable(MCMChannelStatusTableId).GetData();
                foreach (var row in tableData.Values)
                {
                    if (Convert.ToInt32(row[14 /* Monitored */]) == (int)Monitored.Yes)
                    {
                        channelsList.Add(Convert.ToString(row[12 /* Name */]));
                    }
                }
            }
            else
            {
                foreach (var tableId in MCSChannelsTableIds)
                {
                    var channelsTableData = element.GetTable(tableId).GetData();
                    foreach (var row in channelsTableData.Values)
                    {
                        channelsList.Add(Convert.ToString(row[1 /* Label */]));
                    }
                }
            }

            channelsList.Sort();
            var distinctValues = channelsList.Distinct();
            this.ChannelsDropDown.Options = distinctValues;
            this.ChannelsDropDown.Selected = String.Empty;
        }
    }
}