namespace TAG_UMD_Editor
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
        }
    }
}
