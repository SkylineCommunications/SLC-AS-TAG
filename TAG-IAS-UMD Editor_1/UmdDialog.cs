namespace TAG_UMD_Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using SharedMethods;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public class UmdDialog : Dialog
    {
        public UmdDialog(IEngine engine,string elementId, string selectedLayout, string titleIndex) : base(engine)
        {
            Engine = engine;
            var dms = engine.GetDms();
            TagElement = dms.GetElement(new DmsElementId(elementId));
            isMCS = TagElement.Protocol.Name.Contains("MCS");

            SelectedLayout = selectedLayout;
            TitleIndex = titleIndex;
            Clear();
            Title = "UMD Editor";

            RadioButtonPanel = new UmdRadioButtonPanel(isMCS);
            StaticTopPanel = new TopPanel();
            UmdFilterButtons = new FilterButtons();
            TextFormatSection = new TextFormatSection();
            SpecialValuesSection = new SpecialValuesSection();
            TallyAndUmdSection = new TallyAndUmdSection();
            AlarmsSection = new AlarmSection();
            BottomPanelButtons = new BottomPanelButtons();
            UmdButtonActions = new ButtonActions(StaticTopPanel);

            UmdFilterButtons.TextFormatButton.IsEnabled = false; // Default selected option
            var umdValue = CheckUmdValue();
            StaticTopPanel.UmdTextBox.Text = umdValue;

            TextFormatButtonPressed();
        }

        public enum StartRowSectionPosition
        {
            CheckBoxSection = 0,
            StaticTopPanel = 0,
            UmdFilterButtons = 3,
            InitialFilteredSection = 9,
            TextFormat = 9,
            SpecialValuesSection = 18,
            TallySection = 22,
            AlarmSection = 37,
            BottomPanelButtons = 50,
        }

        public enum FilteredBy
        {
            TextFormat,
            SpecialValue,
            TallyAndUmd,
            Alarm,
            All,
        }

        public IDmsElement TagElement { get; set; }

        public IEngine Engine { get; set; }

        public string SelectedLayout { get; set; }

        public string TitleIndex { get; set; }

        public string ElementId { get; set; }

        public bool isMCS { get; set; }

        public UmdRadioButtonPanel RadioButtonPanel { get; private set; }

        public TopPanel StaticTopPanel { get; set; }

        public FilterButtons UmdFilterButtons { get; set; }

        public BottomPanelButtons BottomPanelButtons { get; set; }

        public TextFormatSection TextFormatSection { get; set; }

        public SpecialValuesSection SpecialValuesSection { get; set; }

        public TallyAndUmdSection TallyAndUmdSection { get; set; }

        public AlarmSection AlarmsSection { get; set; }

        public ButtonActions UmdButtonActions { get; set; }

        public void TextFormatButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = false;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = true;

            InitializeUI(FilteredBy.TextFormat);
        }

        public void SpecialValuesButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = true;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = false;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = true;

            InitializeUI(FilteredBy.SpecialValue);
        }

        public void TallyAndUmdButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = true;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = false;
            UmdFilterButtons.AlarmButton.IsEnabled = true;

            InitializeUI(FilteredBy.TallyAndUmd);
        }

        public void AlarmButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = true;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = false;

            InitializeUI(FilteredBy.Alarm);
        }

        public void AllButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = true;
            UmdFilterButtons.AllButton.IsEnabled = false;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = true;

            InitializeUI(FilteredBy.All);
        }

        public void ApplySets()
        {
            var selectedUmd = RadioButtonPanel.UmdRadioButtons.Selected;
            var umdColumnPid = GetWriteIdBySelectedUmd(selectedUmd);

            if (isMCS)
            {
                var layoutsTable = TagElement.GetTable(5000);
                TagElement.GetStandaloneParameter<string>(5999).SetValue(SelectedLayout); // Layout Drop-down Write
                Thread.Sleep(1000);
                layoutsTable.GetColumn<string>(umdColumnPid).SetValue(TitleIndex, StaticTopPanel.UmdTextBox.Text);
            }
            else
            {
                // TAG MCM Actions
            }

            Engine.ExitSuccess("UMD Set Applied.");
        }

        public void ChangeUmdOption()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = false;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = true;

            var umdValue = CheckUmdValue();

            StaticTopPanel.UmdTextBox.Text = umdValue;

            InitializeUI(FilteredBy.TextFormat);
        }

        private string CheckUmdValue()
        {
            var selectedUmd = RadioButtonPanel.UmdRadioButtons.Selected;
            var umdColumnPid = GetReadIdBySelectedUmd(selectedUmd);

            if (isMCS)
            {
                var layoutsTable = TagElement.GetTable(5000);
                TagElement.GetStandaloneParameter<string>(5999).SetValue(SelectedLayout); // Layout Drop-down Write
                Thread.Sleep(1000);
                var umdElementValue = layoutsTable.GetColumn<string>(umdColumnPid).GetValue(TitleIndex, KeyType.PrimaryKey);
                Engine.GenerateInformation($"UMD Value: {umdElementValue}");
                return umdElementValue;
            }
            else
            {
                // TAG MCM Actions
                var tallyLayoutsTable = TagElement.GetTable(2800);
                return string.Empty;
            }
        }

        private int GetWriteIdBySelectedUmd(string selectedValue)
        {
            if (isMCS)
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return 5025;
                    case "UMD 2":
                        return 5026;
                    case "UMD 3":
                        return 5027;
                    case "UMD 4":
                        return 5028;
                    default:
                        return 0;
                }
            }
            else // MCM
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return 2856;
                    case "UMD 2":
                        return 2857;
                    default:
                        return 0;
                }
            }
        }

        private int GetReadIdBySelectedUmd(string selectedValue)
        {
            if (isMCS)
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return 5005;
                    case "UMD 2":
                        return 5006;
                    case "UMD 3":
                        return 5007;
                    case "UMD 4":
                        return 5008;
                    default:
                        return 0;
                }
            }
            else // MCM
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return 2806;
                    case "UMD 2":
                        return 2807;
                    default:
                        return 0;
                }
            }
        }

        private void InitializeUI(FilteredBy sectionFilter)
        {
            Clear();

            AddSection(RadioButtonPanel, new SectionLayout((int)StartRowSectionPosition.CheckBoxSection, 0));
            AddSection(StaticTopPanel, new SectionLayout((int)StartRowSectionPosition.StaticTopPanel, 1));
            AddSection(UmdFilterButtons, new SectionLayout((int)StartRowSectionPosition.UmdFilterButtons, 1));

            switch (sectionFilter)
            {
                case FilteredBy.TextFormat:
                    AddSection(TextFormatSection, new SectionLayout((int)StartRowSectionPosition.InitialFilteredSection, 1));
                    break;
                case FilteredBy.SpecialValue:
                    AddSection(SpecialValuesSection, new SectionLayout((int)StartRowSectionPosition.InitialFilteredSection, 1));
                    break;
                case FilteredBy.TallyAndUmd:
                    AddSection(TallyAndUmdSection, new SectionLayout((int)StartRowSectionPosition.InitialFilteredSection, 1));
                    break;
                case FilteredBy.Alarm:
                    AddSection(AlarmsSection, new SectionLayout((int)StartRowSectionPosition.InitialFilteredSection, 1));
                    break;
                case FilteredBy.All:
                    AddSection(TextFormatSection, new SectionLayout((int)StartRowSectionPosition.InitialFilteredSection, 1));
                    AddSection(SpecialValuesSection, new SectionLayout((int)StartRowSectionPosition.SpecialValuesSection, 1));
                    AddSection(TallyAndUmdSection, new SectionLayout((int)StartRowSectionPosition.TallySection, 1));
                    AddSection(AlarmsSection, new SectionLayout((int)StartRowSectionPosition.AlarmSection, 1));
                    break;
                default:
                    // no action
                    break;
            }

            AddSection(BottomPanelButtons, new SectionLayout((int)StartRowSectionPosition.BottomPanelButtons, 1));
        }
    }

    public class UmdRadioButtonPanel : Section
    {
        public UmdRadioButtonPanel(bool isMCS)
        {
            var optionsList = isMCS ? new List<string> { "UMD 1", "UMD 2", "UMD 3", "UMD 4", } : new List<string> { "UMD 1", "UMD 2", };
            UmdRadioButtons.Options = optionsList;
            UmdRadioButtons.Selected = optionsList.First();
            AddWidget(UmdRadioButtons, 0, 0, 5, 1);
        }

        public RadioButtonList UmdRadioButtons { get; set; } = new RadioButtonList();
    }

    public class TopPanel : Section
    {
        public TopPanel()
        {
            AddWidget(UmdTextLabel, 0, 0);
            AddWidget(UmdTextBox, 1, 0, 1, 5);
            AddWidget(DynamicPropertiesLabel, 2, 0);
        }

        public Label UmdTextLabel { get; } = new Label("UMD Text:");

        public TextBox UmdTextBox { get; set; } = new TextBox { IsMultiline = true, Height = 100, PlaceHolder = "This is #B#an example" };

        public Label DynamicPropertiesLabel { get; } = new Label("Dynamic UMD Properties");
    }

    public class FilterButtons : Section
    {
        public FilterButtons()
        {
            AddWidget(TextFormatButton, 0, 0);
            AddWidget(SpecialValuesButton, 0, 1);
            AddWidget(TallyAndUmdButton, 0, 2);
            AddWidget(AlarmButton, 0, 3);
            AddWidget(AllButton, 0, 4);
        }

        public Button AllButton { get; } = new Button("All") { Width = 150 };

        public Button TextFormatButton { get; } = new Button("Text Format") { Width = 150 };

        public Button SpecialValuesButton { get; } = new Button("Special Values") { Width = 150 };

        public Button TallyAndUmdButton { get; } = new Button("Tally & UMD") { Width = 150 };

        public Button AlarmButton { get; } = new Button("Alarm") { Width = 150 };
    }

    public class TextFormatSection : Section
    {
        public TextFormatSection()
        {
            // Text Attributes
            AddWidget(TextFormatLabel, 0, 0);
            AddWidget(Bold, 1, 0);
            AddWidget(Italics, 1, 1);
            AddWidget(Underlined, 1, 2);
            AddWidget(HalfWidth, 1, 3);
            AddWidget(Regular, 1, 4);
            AddWidget(Flash, 1, 5);

            // Text Color
            AddWidget(TextColorLabel, 2, 0);
            AddWidget(TextColorRed, 3, 0);
            AddWidget(TextColorGreen, 3, 1);
            AddWidget(TextColorYellow, 3, 2);
            AddWidget(TextColorCustomRGB, 3, 3);

            // Background Color
            AddWidget(BackgroundColorLabel, 4, 0);
            AddWidget(BackgroundRed, 5, 0);
            AddWidget(BackgroundGreen, 5, 1);
            AddWidget(BackgroundYellow, 5, 2);
            AddWidget(BackgroundCustomRGB, 5, 3);
        }

        public Label TextFormatLabel { get; } = new Label("Text Attributes");

        public Button Bold { get; } = new Button("Bold") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Italics { get; } = new Button("Italics") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Underlined { get; } = new Button("Underlined") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button HalfWidth { get; } = new Button("Half Width") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Regular { get; } = new Button("Regular") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Flash { get; } = new Button("Flash") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Label TextColorLabel { get; } = new Label("Text Color");

        public Button TextColorRed { get; } = new Button("Red") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button TextColorGreen { get; } = new Button("Green") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button TextColorYellow { get; } = new Button("Yellow") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button TextColorCustomRGB { get; } = new Button("Custom RGB") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Label BackgroundColorLabel { get; } = new Label("Background Color");

        public Button BackgroundRed { get; } = new Button("Red") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button BackgroundGreen { get; } = new Button("Green") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button BackgroundYellow { get; } = new Button("Yellow") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button BackgroundCustomRGB { get; } = new Button("Custom RGB") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };
    }

    public class SpecialValuesSection : Section
    {
        public SpecialValuesSection()
        {
            AddWidget(SpecialValuesLabel, 0, 0);
            AddWidget(Bitrate, 1, 0);
            AddWidget(ChannelTitle, 1, 1);
            AddWidget(Codec, 1, 2);
            AddWidget(ColorSpace, 1, 3);
            AddWidget(ColorSpaceShort, 1, 4);
            AddWidget(HDType, 1, 5);
            AddWidget(Resolution, 2, 0);
            AddWidget(SDTName, 2, 1);
            AddWidget(SDTProvider, 2, 2);
            AddWidget(Timecode, 2, 3);
            AddWidget(TransportId, 2, 4);
        }

        public Label SpecialValuesLabel { get; } = new Label("Special Values");

        public Button Bitrate { get; } = new Button("Bitrate") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button ChannelTitle { get; } = new Button("Channel Title") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Codec { get; } = new Button("Codec") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button ColorSpace { get; } = new Button("Color Space") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button ColorSpaceShort { get; } = new Button("Color Space (Short)") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button HDType { get; } = new Button("HD Type") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Resolution { get; } = new Button("Resolution") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button SDTName { get; } = new Button("SDT Name") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button SDTProvider { get; } = new Button("SDT Provider") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Timecode { get; } = new Button("Timecode") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button TransportId { get; } = new Button("Transport ID") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };
    }

    public class TallyAndUmdSection : Section
    {
        public TallyAndUmdSection()
        {
            // Tally Widgets
            AddWidget(TallyLabel, 0, 0);
            AddWidget(Tally0TextColor, 1, 0);
            AddWidget(Tally0Background, 1, 1);
            AddWidget(Tally0Light, 1, 2);
            AddWidget(Tally1TextColor, 2, 0);
            AddWidget(Tally1Background, 2, 1);
            AddWidget(Tally1Light, 2, 2);

            // UMD Widgets
            AddWidget(UmdLabel, 3, 0);
            AddWidget(Umd0TextColor, 4, 0);
            AddWidget(Umd0Background, 4, 1);
            AddWidget(Umd0Text, 4, 2);
            AddWidget(Umd1TextColor, 5, 0);
            AddWidget(Umd1Background, 5, 1);
            AddWidget(Umd1Text, 5, 2);
        }

        public Label TallyLabel { get; } = new Label("Tally");

        public Button Tally0TextColor { get; } = new Button("Tally 0 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Tally0Background { get; } = new Button("Tally 0 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Tally0Light { get; } = new Button("Tally 0 Light") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Tally1TextColor { get; } = new Button("Tally 1 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Tally1Background { get; } = new Button("Tally 1 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Tally1Light { get; } = new Button("Tally 1 Light") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Label UmdLabel { get; } = new Label("UMD");

        public Button Umd0TextColor { get; } = new Button("UMD 0 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Umd0Background { get; } = new Button("UMD 0 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Umd0Text { get; } = new Button("UMD 0 Text") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Umd1TextColor { get; } = new Button("UMD 1 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Umd1Background { get; } = new Button("UMD 1 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button Umd1Text { get; } = new Button("UMD 1 Text") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };
    }

    public class AlarmSection : Section
    {
        public AlarmSection()
        {
            AddWidget(AlarmLabel, 0, 0);
            AddWidget(AlarmTextColor, 1, 0);
            AddWidget(AlarmBackground, 1, 1);
            AddWidget(AlarmCount, 1, 2);
        }

        public Label AlarmLabel { get; } = new Label("Alarm");

        public Button AlarmTextColor { get; } = new Button("Alarm Text Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button AlarmBackground { get; } = new Button("Alarm Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };

        public Button AlarmCount { get; } = new Button("Alarm Count") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };
    }

    public class BottomPanelButtons : Section
    {
        public BottomPanelButtons()
        {
            AddWidget(new WhiteSpace(),0,0);
            AddWidget(CancelButton, 1, 0, HorizontalAlignment.Left);
            AddWidget(ApplyButton, 1, 5, HorizontalAlignment.Right);
        }

        public Button CancelButton { get; set; } = new Button("Cancel") { Width = 100 };

        public Button ApplyButton { get; set; } = new Button("Apply") { Width = 100 };
    }
}
