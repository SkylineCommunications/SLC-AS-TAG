namespace TAG_IAS_Change_Mosaic_Audio_1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    internal class MosaicDialog : Dialog
    {
        private readonly IEngine engine;
        private IDms dms;
        private List<AudioPidData> audioPidList;
        private List<OutputConfigData> outputList;
        private List<string> listChannelsPerLayout;

        public Dictionary<string, string> AudioDec = new Dictionary<string, string>
        {
            {"0","Don't Care"},
            { "1","MPEG"},
            { "2","AC3"},
            { "3","E-AC3"},
            { "4","DTS"},
            { "5","ADTS/AAC"},
            { "6","ADTS/HE-AAC"},
            { "7","LATM/AAC"},
            { "8","LATM/HE-AAC"},
            { "9","ATMOS"},
        };

        public MosaicDialog(IEngine engine) : base(engine)
        {
            this.engine = engine;

            Title = "Change Mosaic Audio";
            MonitoringTagLabel = new Label("Monitoring TAG:");
            MonitoringTagValue = new Label(String.Empty);
            OutputEncoderLabel = new Label("Output Encoder:");
            OutputEncoderValue = new Label(String.Empty);
            ChannelOutputLabel = new Label("Channel Output:");
            ChannelOutputDropDown = new DropDown();
            ChannelAudioEncodingLabel = new Label("Channel Audio Encoding:");
            ChannelAudioEncodingDropDown = new DropDown();
            ChangeAudioButton = new Button("Change Audio");
            CancelButton = new Button("Cancel");

            AddWidget(MonitoringTagLabel, 0, 0);
            AddWidget(MonitoringTagValue, 0, 1);
            AddWidget(OutputEncoderLabel, 1, 0);
            AddWidget(OutputEncoderValue, 1, 1);
            AddWidget(ChannelOutputLabel, 2, 0);
            AddWidget(ChannelOutputDropDown, 2, 1);
            AddWidget(ChannelAudioEncodingLabel, 3, 0);
            AddWidget(ChannelAudioEncodingDropDown, 3, 1);
            AddWidget(ChangeAudioButton, 4, 1, HorizontalAlignment.Right);
            AddWidget(CancelButton, 4, 0, HorizontalAlignment.Left);

            MonitoringTagLabel.Width = 150;
            MonitoringTagValue.Width = 300;
            OutputEncoderLabel.Width = 150;
            OutputEncoderValue.Width = 300;
            ChannelOutputLabel.Width = 150;
            ChannelOutputDropDown.Width = 300;
            ChannelAudioEncodingLabel.Width = 150;
            ChannelAudioEncodingDropDown.Width = 300;
            ChangeAudioButton.Width = 150;
            CancelButton.Width = 150;
        }

        public Label MonitoringTagLabel { get; private set; }

        public Label MonitoringTagValue { get; private set; }

        public Label OutputEncoderLabel { get; private set; }

        public Label ChannelOutputLabel { get; private set; }

        public Label ChannelAudioEncodingLabel { get; private set; }

        public Label OutputEncoderValue { get; private set; }

        public DropDown ChannelOutputDropDown { get; private set; }

        public DropDown ChannelAudioEncodingDropDown { get; private set; }

        public Button ChangeAudioButton { get; private set; }

        public Button CancelButton { get; private set; }

        internal void ChangeAudio()
        {
            var tagElementName = this.MonitoringTagValue.Text;
            var selectedOutput = this.OutputEncoderValue.Text;
            var selectedAudioEncoderPID = this.ChannelAudioEncodingDropDown.Selected;

            var tagElement = dms.GetElement(tagElementName);
            var tagType = tagElement.Protocol.Name;
            var audioId = audioPidList.Find(x => x.FormattedString.Equals(selectedAudioEncoderPID))?.Id;

            if (tagType.Contains("MCS"))
            {
                var outputAudiosTable = tagElement.GetTable(3302);
                var outputAudiosTableData = outputAudiosTable.GetData().Values;
                var outputAudioRow = outputAudiosTableData.FirstOrDefault(x => Convert.ToString(x[1]).Equals($"{selectedOutput}/1"));

                var primaryKey = Convert.ToString(outputAudioRow[0]);

                outputAudiosTable.GetColumn<double?>(3358).SetValue(primaryKey, Convert.ToInt32(audioId)); // Setting Write to execute QA
                outputAudiosTable.GetColumn<double?>(3360).SetValue(primaryKey, Convert.ToInt32(audioId)); // Setting Write to execute QA
            }
            else
            {
                tagElement.GetStandaloneParameter<string>(2100).SetValue(selectedOutput); // Output Stream
                tagElement.GetStandaloneParameter<double?>(2174).SetValue(Convert.ToInt32(audioId)); // Audio PID
                tagElement.GetStandaloneParameter<double?>(2176).SetValue(1); // Set Audio Channel Button
            }

            engine.ExitSuccess("Script completed");
        }

        internal void SetValues(string elementId, string outputId, string layoutId)
        {
            dms = engine.GetDms();
            var tagElement = dms.GetElement(new DmsElementId(elementId));
            var tagElementName = tagElement.Name;
            var tagType = tagElement.Protocol.Name;
            string outputName;
            this.MonitoringTagValue.Text = tagElementName;
            if (tagType.Contains("MCS"))
            {
                var pidsOverviewTableData = tagElement.GetTable(2500).GetData();
                var allLayoutsTableData = tagElement.GetTable(5600).GetData();
                listChannelsPerLayout = CreateAllLayoutsList(allLayoutsTableData, layoutId);
                audioPidList = CreateMcsAudioPidList(pidsOverviewTableData);
                var outputTable = tagElement.GetTable(3100).GetData(); // replace with the actual TAG MCS Data.
                outputName = GetOutputNameById(outputTable, outputId);
            }
            else
            {
                this.MonitoringTagValue.Text = tagElementName;

                var pidsOverviewTable = tagElement.GetTable(1200).GetData();
                var allLayoutsTableData = tagElement.GetTable(10300).GetData();
                var enconderTable = tagElement.GetTable(1500).GetData();

                outputName = GetOutputNameById(enconderTable, outputId);
                listChannelsPerLayout = CreateAllLayoutsList(allLayoutsTableData, layoutId);
                audioPidList = CreateMcmAudioPidList(pidsOverviewTable);
            }

            outputList = new List<OutputConfigData> { new OutputConfigData { OutputLabel = outputName} };
            this.OutputEncoderValue.Text = outputList[0].OutputLabel;

            if (listChannelsPerLayout.Count > 0)
            {
                this.ChannelOutputDropDown.Options = listChannelsPerLayout;
                this.ChannelOutputDropDown.Selected = listChannelsPerLayout.FirstOrDefault();
            }
            else
            {
                listChannelsPerLayout.Add("No channels available in this layout.");
                this.ChannelOutputDropDown.Options = listChannelsPerLayout;
                this.ChannelOutputDropDown.Selected = listChannelsPerLayout.FirstOrDefault();
                this.ChannelOutputDropDown.IsEnabled = false;
                this.ChangeAudioButton.IsEnabled = false;
            }

            var audiopidList = audioPidList.Where(x => x.Index.Contains(ChannelOutputDropDown.Selected)).Select(x => x.FormattedString).ToList();

            if (audiopidList.Count > 0)
            {
                this.ChannelAudioEncodingDropDown.Options = audiopidList;
                this.ChannelAudioEncodingDropDown.Selected = audiopidList[0];
            }
            else
            {
                audiopidList.Add("No audio PIDs available for the selected layout");
                this.ChannelAudioEncodingDropDown.Options = audiopidList;
                this.ChannelAudioEncodingDropDown.Selected = audiopidList[0];
                this.ChannelAudioEncodingDropDown.IsEnabled = false;
                this.ChangeAudioButton.IsEnabled = false;
            }
        }

        private string GetOutputNameById(IDictionary<string, object[]> enconderTable, string outputId)
        {
            foreach (var row in enconderTable.Values)
            {
                if (Convert.ToString(row[0]).Equals(outputId))
                {
                    return Convert.ToString(row[1]);
                }
            }

            return string.Empty;
        }

        internal void UpdateChannelAudioEncoderOptions()
        {
            var selectedChannelOutput = this.ChannelOutputDropDown.Selected;

            var audioPidOptions = audioPidList.Where(x => x.Index.Contains(selectedChannelOutput)).ToList();
            var formattedList = audioPidOptions.Select(x => x.FormattedString).ToList();

            if (formattedList.Any())
            {
                this.ChannelAudioEncodingDropDown.Options = formattedList;
                this.ChannelAudioEncodingDropDown.Selected = formattedList[0];
                this.ChannelAudioEncodingDropDown.IsEnabled = true;
                this.ChangeAudioButton.IsEnabled = true;
            }
            else
            {
                formattedList.Add("No audio PIDs available for the selected layout");
                this.ChannelAudioEncodingDropDown.Options = formattedList;
                this.ChannelAudioEncodingDropDown.Selected = formattedList[0];
                this.ChannelAudioEncodingDropDown.IsEnabled = false;
                this.ChangeAudioButton.IsEnabled = false;
            }
        }

        private static List<string> CreateAllLayoutsList(IDictionary<string, object[]> allLayoutsTableData, string selectedLayoutID)
        {
            var channelsInLayout = new List<string>();
            foreach (var row in allLayoutsTableData.Values)
            {
                var layoutId = Convert.ToString(row[3]);
                var layoutName = Convert.ToString(row[4 /*Layout*/]);
                var channel = Convert.ToString(row[2 /*Title*/]);
                if (channel.Equals("0" /*None*/) || channel.Equals("Reserved"))
                {
                    continue;
                }

                if (!channelsInLayout.Contains(layoutName) && layoutId.Equals(selectedLayoutID))
                {
                    channelsInLayout.Add(channel);
                }
            }

            channelsInLayout.Sort();

            return channelsInLayout;
        }

        private List<AudioPidData> CreateMcmAudioPidList(IDictionary<string, object[]> audioPids)
        {
            var list = new List<AudioPidData>();
            foreach (var row in audioPids.Values)
            {
                var channelLabel = Convert.ToString(row[1]);
                if (listChannelsPerLayout.Any(x => channelLabel.Contains(x)) && Convert.ToString(row[3]).Equals("Audio" /*Type*/))
                {
                    list.Add(new AudioPidData
                    {
                        Index = Convert.ToString(row[1 /*Index*/]),
                        Id = Convert.ToString(row[2 /*ID*/]),
                        Encoding = Convert.ToString(row[19 /*Encoding*/]),
                        FormattedString = $"{Convert.ToString(row[19 /*Encoding*/])} (PID: {Convert.ToString(row[2 /*ID*/])})",
                    });
                }
            }

            return list;
        }

        private List<AudioPidData> CreateMcsAudioPidList(IDictionary<string, object[]> audioPids)
        {
            var list = new List<AudioPidData>();
            foreach (var row in audioPids.Values)
            {
                var channelLabel = Convert.ToString(row[29]).Split('/')[0];

                if (listChannelsPerLayout.Contains(channelLabel) && Convert.ToString(row[2]).Equals("2" /*Type = Audio*/))
                {
                    list.Add(new AudioPidData
                    {
                        Index = Convert.ToString(row[29 /*Index*/]),
                        Id = Convert.ToString(row[1 /*ID*/]),
                        Encoding = Convert.ToString(row[18 /*Encoding*/]),
                        FormattedString = $"{AudioDec[Convert.ToString(row[18 /*Audio DEC*/])]} (PID: {Convert.ToString(row[1 /*ID*/])})",
                    });
                }
            }

            return list;
        }

    }

    internal class EncoderData
    {
        public string EncoderName { get; set; }

        public string Layout { get; set; }
    }

    internal class OutputConfigData
    {
        public string OutputLabel { get; set; }

        public string Layout { get; set; }
    }

    internal class AudioPidData
    {
        public string Index { get; set; }

        public string Id { get; set; }

        public string Encoding { get; set; }

        public string FormattedString { get; set; }
    }
}