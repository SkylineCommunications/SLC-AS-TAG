namespace TAG_UMD_Editor
{
    using System.Collections.Generic;

    public class ButtonActions
    {
        public ButtonActions(TopPanel staticTopPanel)
        {
            StaticTopPanel = staticTopPanel;
        }

        public TopPanel StaticTopPanel { get; set; }

        public void ValueButtonPressed(ButtonValues buttonValue)
        {
            StaticTopPanel.UmdTextBox.Text += TextDictionary[buttonValue];
        }

        public static readonly Dictionary<ButtonValues, string> TextDictionary = new Dictionary<ButtonValues, string>
        {
            {ButtonValues.Bold,"#B#" },
            {ButtonValues.Underlined,"#U#" },
            {ButtonValues.Italics,"#I#" },
            {ButtonValues.RegularFormat,"#R#" },
            {ButtonValues.HalfWidth,"#H#" },
            {ButtonValues.Flash,"#F#" },
            {ButtonValues.TextRed,"#RED#" },
            {ButtonValues.TextGreen,"#GREEN#" },
            {ButtonValues.TextYellow,"#YELLOW#" },
            {ButtonValues.TextCustomRGB,"#RGBXXXXXX#" },
            {ButtonValues.BackgroundRed,"#BRED#" },
            {ButtonValues.BackgroundGreen,"#BGREEN#" },
            {ButtonValues.BackgroundYellow,"#BYELLOW#" },
            {ButtonValues.BackgroundCustomRGB,"#BRGBXXXXXX#" },
            {ButtonValues.Bitrate,"#BITRATE#" },
            {ButtonValues.ChannelTitle,"#TITLE#" },
            {ButtonValues.Codec,"#CODEC#" },
            {ButtonValues.ColorSpace,"#COLORSPACE#" },
            {ButtonValues.ColorSpaceShort,"#SHORTCS#" },
            {ButtonValues.HDType,"#HDR#" },
            {ButtonValues.Resolution,"#RESOLUTION#" },
            {ButtonValues.SDTName,"#SDTNAME#" },
            {ButtonValues.SDTProvider,"#SDTPROV#" },
            {ButtonValues.Timecode,"#TIMECODE#" },
            {ButtonValues.TransportId,"#TSID#" },
            {ButtonValues.Tally0Background,"#TBC0#" },
            {ButtonValues.Tally0Light,"#T0#" },
            {ButtonValues.Tally0TextColor,"#TC0#" },
            {ButtonValues.Tally1Background,"#TBC1#" },
            {ButtonValues.Tally1Light,"#T1#" },
            {ButtonValues.Tally1TextColor,"#TC1#" },
            {ButtonValues.UMD0TextAndColor,"#U0#" },
            {ButtonValues.UMD0Text,"#UF0#" },
            {ButtonValues.UMD0Color,"#UC0#" },
            {ButtonValues.UMD1TextAndColor,"#U1#" },
            {ButtonValues.UMD1Text,"#UF1#" },
            {ButtonValues.UMD1Color,"#UC1#" },
            {ButtonValues.AlarmTextColor,"#TCA#" },
            {ButtonValues.AlarmBackground,"#BCA#" },
            {ButtonValues.AlarmCount,"#AC#" },
        };

        public enum ButtonValues
        {
            Bold,
            Underlined,
            Italics,
            RegularFormat,
            HalfWidth,
            Flash,
            TextRed,
            TextGreen,
            TextYellow,
            TextCustomRGB,
            BackgroundRed,
            BackgroundGreen,
            BackgroundYellow,
            BackgroundCustomRGB,
            Bitrate,
            ChannelTitle,
            Codec,
            ColorSpace,
            ColorSpaceShort,
            HDType,
            Resolution,
            SDTName,
            SDTProvider,
            Timecode,
            TransportId,
            Tally0Background,
            Tally0Light,
            Tally0TextColor,
            Tally1Background,
            Tally1Light,
            Tally1TextColor,
            UMD0TextAndColor,
            UMD0Text,
            UMD0Color,
            UMD1TextAndColor,
            UMD1Text,
            UMD1Color,
            AlarmTextColor,
            AlarmBackground,
            AlarmCount,
        }
    }
}
