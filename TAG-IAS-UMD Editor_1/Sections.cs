namespace TAG_UMD_Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

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

        public Button Bold { get; } = new Button("Bold") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will be in bold format." };

        public Button Italics { get; } = new Button("Italics") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will be in italics." };

        public Button Underlined { get; } = new Button("Underlined") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will be underlined." };

        public Button HalfWidth { get; } = new Button("Half Width") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will be half width." };

        public Button Regular { get; } = new Button("Regular") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will be regular format." };

        public Button Flash { get; } = new Button("Flash") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Following text will flash." };

        public Label TextColorLabel { get; } = new Label("Text Color");

        public Button TextColorRed { get; } = new Button("Red") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set following text color to red in UMD" };

        public Button TextColorGreen { get; } = new Button("Green") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set following text color to green in UMD" };

        public Button TextColorYellow { get; } = new Button("Yellow") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set following text color to yellow in UMD." };

        public Button TextColorCustomRGB { get; } = new Button("Custom RGB") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set following text color with RGB hexadecimal value." };

        public Label BackgroundColorLabel { get; } = new Label("Background Color");

        public Button BackgroundRed { get; } = new Button("Red") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set background color red." };

        public Button BackgroundGreen { get; } = new Button("Green") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set background color green." };

        public Button BackgroundYellow { get; } = new Button("Yellow") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set background color yellow." };

        public Button BackgroundCustomRGB { get; } = new Button("Custom RGB") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Set background color with RGB hexadecimal value." };
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

        public Button Bitrate { get; } = new Button("Bitrate") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert source bitrate as UMD text." };

        public Button ChannelTitle { get; } = new Button("Channel Title") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Configured channel title." };

        public Button Codec { get; } = new Button("Codec") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert source codec as UMD text." };

        public Button ColorSpace { get; } = new Button("Color Space") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Inserts full color space info:\r\n\r\n“GrayScale”, “YUV420-8bit”, “YUV422-10bit”,“YUV422-10bit/BT.709”, “YUV422-10bit/HLG”,“YUV422-10bit/HDR10”,“YUV422-10bit/BT.2020”" };

        public Button ColorSpaceShort { get; } = new Button("Color Space (Short)") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Inserts color space – short format (without HDR info):\r\n\r\n“GrayScale”, “YUV420-8bit”, “YUV422-10bit”,“YUV422-10bit/BT.709”,,“YUV422-10bit/BT.2020”" };

        public Button HDType { get; } = new Button("HD Type") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "HD type (SDR/PQ/HDR10/HLG)\r\n\r\n** Please note HDR Hashtags does not work with 2110" };

        public Button Resolution { get; } = new Button("Resolution") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert source resolution as UMD text." };

        public Button SDTName { get; } = new Button("SDT Name") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert SDT name from compressed MPEGTS as UMD text." };

        public Button SDTProvider { get; } = new Button("SDT Provider") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert SDT provider from compressed MPEGTS as UMD text." };

        public Button Timecode { get; } = new Button("Timecode") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert embedded timecode as UMD text." };

        public Button TransportId { get; } = new Button("Transport ID") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert Transport ID from compressed MPEGTS as UMD text." };
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
            AddWidget(Umd0Color, 4, 0);
            AddWidget(Umd0TextAndColor, 4, 1);
            AddWidget(Umd0Text, 4, 2);
            AddWidget(Umd1Color, 5, 0);
            AddWidget(Umd1TextAndColor, 5, 1);
            AddWidget(Umd1Text, 5, 2);
        }

        public Label TallyLabel { get; } = new Label("Tally");

        public Button Tally0TextColor { get; } = new Button("Tally 0 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use tally index 0 for text color." };

        public Button Tally0Background { get; } = new Button("Tally 0 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use tally index 0 for background color." };

        public Button Tally0Light { get; } = new Button("Tally 0 Light") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Inserts a left tally light/square." };

        public Button Tally1TextColor { get; } = new Button("Tally 1 TextColor") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use tally index 1 for text color." };

        public Button Tally1Background { get; } = new Button("Tally 1 Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use tally index 1 for background color." };

        public Button Tally1Light { get; } = new Button("Tally 1 Light") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Inserts a right tally light/square" };

        public Label UmdLabel { get; } = new Label("UMD");

        public Button Umd0Color { get; } = new Button("UMD 0 Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use UMD index 0 for text color" };

        public Button Umd0TextAndColor { get; } = new Button("UMD 0 Text & Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert UMD index 0 text & color" };

        public Button Umd0Text { get; } = new Button("UMD 0 Text") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert UMD index 0 texts without its color" };

        public Button Umd1Color { get; } = new Button("UMD 1 Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Use UMD index 1 for text color" };

        public Button Umd1TextAndColor { get; } = new Button("UMD 1 Text & Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert UMD index 1 text & color" };

        public Button Umd1Text { get; } = new Button("UMD 1 Text") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Insert UMD index 1 texts without its color" };
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

        public Button AlarmTextColor { get; } = new Button("Alarm Text Color") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Text color from alarm." };

        public Button AlarmBackground { get; } = new Button("Alarm Background") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "Background color from alarm." };

        public Button AlarmCount { get; } = new Button("Alarm Count") { Height = 25, Width = 150, Style = ButtonStyle.CallToAction, Tooltip = "alarm count." };
    }

    public class BottomPanelButtons : Section
    {
        public BottomPanelButtons()
        {
            AddWidget(new WhiteSpace(), 0, 0);
            AddWidget(CancelButton, 1, 0, HorizontalAlignment.Left);
            AddWidget(ApplyButton, 1, 5, HorizontalAlignment.Right);
        }

        public Button CancelButton { get; set; } = new Button("Cancel") { Width = 100 };

        public Button ApplyButton { get; set; } = new Button("Apply") { Width = 100 };
    }
}
