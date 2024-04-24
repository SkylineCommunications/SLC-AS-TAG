namespace TAG_Send_Channel_To_Layout
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System.Linq;
    using TagLibrary_1;

    public class LayoutDialog : Dialog
    {
        public LayoutDialog(IEngine engine, string elementId, string channelId) : base(engine)
        {
            Dms = engine.GetDms();
            TagElement = Dms.GetElement(new DmsElementId(elementId));
            var protocolName = TagElement.Protocol.Name;
            var tagType = TAG.GetElementType(protocolName);
            Tag = TAG.GetDeviceByType(TagElement, tagType);
            var layoutsInElement = Tag.GetLayoutsFromElement();
            var selectedChannel = Tag.GetChannelById(channelId);

            Title = "Send Channel To Layout";

            SelectedChannel = new Label(selectedChannel);
            ChannelLabel = new Label($"Selected Channel:");
            LayoutLabel = new Label("Select a Layout:");
            LayoutsDropDown = new DropDown(layoutsInElement);
            WarningLabel = new Label("Warning") { Style = TextStyle.Bold, IsVisible = false, };
            LayoutInfoLabel = new Label();
            UpdateButton = new Button("Apply");
            CancelButton = new Button("Cancel");

            ChannelLabel.Width = 150;
            LayoutsDropDown.Width = 400;
            UpdateButton.Width = 110;
            CancelButton.Width = 110;

            if (!layoutsInElement.Any())
            {
                CancelButton = new Button("Cancel");
                LayoutInfoLabel = new Label($"No Layouts available in element {TagElement.Name}");
                AddWidget(LayoutInfoLabel, 0, 0);
                AddWidget(CancelButton, 5, 0, HorizontalAlignment.Left);
            }
            else
            {
                AddWidget(ChannelLabel, 0, 0);
                AddWidget(SelectedChannel, 0, 1);
                AddWidget(LayoutLabel, 1, 0);
                AddWidget(LayoutsDropDown, 1, 1, 1, 2);
                AddWidget(WarningLabel, 2, 1, 1, 2);
                AddWidget(LayoutInfoLabel, 3, 1, 1, 2);
                AddWidget(CancelButton, 5, 1, HorizontalAlignment.Left);
                AddWidget(UpdateButton, 5, 2, HorizontalAlignment.Right);

                CheckLayoutOption();
            }
        }

        public IDmsElement TagElement { get; set; }

        public IDms Dms { get; set; }

        public Label ChannelLabel { get; private set; }

        public Label SelectedChannel { get; private set; }

        public Label LayoutLabel { get; private set; }

        public Label LayoutInfoLabel { get; private set; }

        public Label WarningLabel { get; private set; }

        public DropDown LayoutsDropDown { get; private set; }

        public Button UpdateButton { get; private set; }

        public Button CancelButton { get; private set; }

        public TAG Tag { get; private set; }

        public string LayoutIndex { get; private set; }

        public void CheckLayoutOption()
        {
            WarningLabel.IsVisible = false;
            var positionsDict = Tag.GetPositionsAndChannelsInLayout(LayoutsDropDown.Selected, Engine);
            var availablePosition = positionsDict.FirstOrDefault(x => x.Value.ChannelTitle.Equals("None") || x.Value.ChannelTitle.Equals("0"));

            if (availablePosition.Key == null)
            {
                availablePosition = positionsDict.First();
                WarningLabel.IsVisible = true;
                LayoutInfoLabel.Text = $"No available empty position in {LayoutsDropDown.Selected} Layout. " +
                                       $"The selected channel, {SelectedChannel.Text}, will replace {availablePosition.Value.ChannelTitle} in position {availablePosition.Value.Position}.";
            }
            else
            {
                LayoutInfoLabel.Text = $"Position {availablePosition.Value.Position} in Layout {LayoutsDropDown.Selected} available for channel {SelectedChannel.Text}";
            }

            LayoutIndex = availablePosition.Value.Index;
        }

        public void SetChannelToLayout()
        {
            var allLayoutsTable = TagElement.GetTable(Tag.StaticInfo.AllLayouts.TableId);
            allLayoutsTable.GetColumn<string>(Tag.StaticInfo.AllLayouts.TitleColumnPid).SetValue(LayoutIndex, SelectedChannel.Text);
            Engine.ExitSuccess("Channel Set to Layout");
        }
    }
}