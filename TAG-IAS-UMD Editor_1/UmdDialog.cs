namespace TAG_UMD_Editor
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public class UmdDialog : Dialog
    {
        public UmdDialog(IEngine engine) : base(engine)
        {
            InitializeUI();
        }

        public enum StartRowSectionPosition
        {
            StaticTopPanel = 0,
            UmdFilterButtons = 3,
            InitialFilteredSection = 9,
            TextFormat = 9,
            SpecialValuesSection = 18,
            TallySection = 22,
            AlarmSection = 37,
        }

        private void InitializeUI()
        {
            Clear();
            Title = "UMD Editor";

            CheckBoxPanel = new UmdCheckPanel();
            StaticTopPanel = new TopPanel();
            UmdFilterButtons = new FilterButtons();
            TextFormatSection = new TextFormatSection();
            SpecialValuesSection = new SpecialValuesSection();
            TallyAndUmdSection = new TallyAndUmdSection();
            AlarmsSection = new AlarmSection();

            UmdFilterButtons.AllButton.IsEnabled = false; // Default selected option

            AddSection(CheckBoxPanel, new SectionLayout(0, 0));
            AddSection(StaticTopPanel, new SectionLayout((int)StartRowSectionPosition.StaticTopPanel, 1));
            AddSection(UmdFilterButtons, new SectionLayout((int)StartRowSectionPosition.UmdFilterButtons, 1));
            AddSection(TextFormatSection, new SectionLayout((int)StartRowSectionPosition.TextFormat, 1));
            AddSection(SpecialValuesSection, new SectionLayout((int)StartRowSectionPosition.SpecialValuesSection, 1));
            AddSection(TallyAndUmdSection, new SectionLayout((int)StartRowSectionPosition.TallySection, 1));
            AddSection(AlarmsSection, new SectionLayout((int)StartRowSectionPosition.AlarmSection, 1));
            AddWidget(CancelButton, 150, 0, HorizontalAlignment.Left);

            //AddSection(DetailsPanel, new SectionLayout(0, 0));
            //int position = DetailsPanel.RowCount;
            //foreach (var slotDefintion in SlotDefinitions)
            //{
            //    AddSection(slotDefintion, new SectionLayout(position, 0));
            //    position++;
            //}

            //AddWidget(AddButton, position++, 0);
            //AddWidget(new WhiteSpace(), position++, 0);
            //AddSection(BottomPanel, position, 0);
        }

        public void TextFormatButtonPressed()
        {
            UmdFilterButtons.TextFormatButton.IsEnabled = false;
            UmdFilterButtons.AllButton.IsEnabled = true;
            UmdFilterButtons.SpecialValuesButton.IsEnabled = true;
            UmdFilterButtons.TallyAndUmdButton.IsEnabled = true;
            UmdFilterButtons.AlarmButton.IsEnabled = true;
        }

        public UmdCheckPanel CheckBoxPanel { get; private set; }

        public TopPanel StaticTopPanel { get; set; }

        public FilterButtons UmdFilterButtons { get; set; }

        public Button CancelButton { get; set; } = new Button("Cancel") { Width = 100 };

        public TextFormatSection TextFormatSection { get; set; }

        public SpecialValuesSection SpecialValuesSection { get; set; }

        public TallyAndUmdSection TallyAndUmdSection { get; set; }

        public AlarmSection AlarmsSection { get; set; }
    }

    public class UmdCheckPanel : Section
    {
        public UmdCheckPanel()
        {
            UmdCheckBoxList.SetOptions(new List<string> { "UMD 1", "UMD 2", "UMD 3", "dasd", "asdsa", "sasa" });
            AddWidget(UmdCheckBoxList, 0, 0, 10, 1);
        }

        public CheckBoxList UmdCheckBoxList { get; set; } = new CheckBoxList();

        public CheckBox UmdCheckBox { get; set; }
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
            AddWidget(AllButton, 0, 0);
            AddWidget(TextFormatButton, 0, 1);
            AddWidget(SpecialValuesButton, 0, 2);
            AddWidget(TallyAndUmdButton, 0, 3);
            AddWidget(AlarmButton, 0, 4);
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

            //Background Color
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
            AddWidget(TransponderId, 2, 4);
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

        public Button TransponderId { get; } = new Button("Transponder ID") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction };
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
}
