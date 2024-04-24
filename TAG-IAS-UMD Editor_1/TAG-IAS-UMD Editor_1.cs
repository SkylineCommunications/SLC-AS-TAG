/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2024	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

namespace TAG_IAS_UMD_Editor_1
{
    using SharedMethods;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;
    using System;
    using TAG_UMD_Editor;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        /// <summary>
        /// Gets or sets the script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        ///

        public void Run(IEngine engine)
        {
            // DO NOT REMOVE THIS COMMENTED-OUT CODE OR THE SCRIPT WON'T RUN!
            // DataMiner evaluates if the script needs to launch in interactive mode.
            // This is determined by a simple string search looking for "engine.ShowUI" in the source code.
            // However, because of the toolkit NuGet package, this string cannot be found here.
            // So this comment is here as a workaround.
            //// engine.ShowUI();
            try
            {
                var elementId = SharedMethods.GetOneDeserializedValue(engine.GetScriptParam("Element ID").Value);
                var titleIndex = SharedMethods.GetOneDeserializedValue(engine.GetScriptParam("Title Index").Value);
                var layoutName = SharedMethods.GetOneDeserializedValue(engine.GetScriptParam("Layout Name").Value);

                //// IAS Toolkit code
                var controller = new InteractiveController(engine);
                var dialog = new UmdDialog(engine, elementId, layoutName, titleIndex);

                OnPressedButtons(engine, dialog);

                controller.Run(dialog);
            }
            catch (ScriptAbortException)
            {
                // ignore abort
            }
            catch (InteractiveUserDetachedException)
            {
                // ignore abort
            }
            catch (Exception ex)
            {
                engine.Log($"{ex}");
                engine.GenerateInformation($"{ex}");
            }
        }

        private void OnPressedButtons(IEngine engine, UmdDialog dialog)
        {
            // Filtered Section
            dialog.UmdFilterButtons.TextFormatButton.Pressed += (sender, args) => dialog.TextFormatButtonPressed();
            dialog.UmdFilterButtons.SpecialValuesButton.Pressed += (sender, args) => dialog.SpecialValuesButtonPressed();
            dialog.UmdFilterButtons.TallyAndUmdButton.Pressed += (sender, args) => dialog.TallyAndUmdButtonPressed();
            dialog.UmdFilterButtons.AlarmButton.Pressed += (sender, args) => dialog.AlarmButtonPressed();
            dialog.UmdFilterButtons.AllButton.Pressed += (sender, args) => dialog.AllButtonPressed();
            dialog.RadioButtonPanel.UmdRadioButtons.Changed += (sender, args) => dialog.ChangeUmdOption();

            // Text Attributes
            dialog.TextFormatSection.Bold.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Bold);
            dialog.TextFormatSection.Underlined.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Underlined);
            dialog.TextFormatSection.Italics.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Italics);
            dialog.TextFormatSection.Regular.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.RegularFormat);
            dialog.TextFormatSection.HalfWidth.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.HalfWidth);
            dialog.TextFormatSection.Flash.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Flash);

            // Text Color
            dialog.TextFormatSection.TextColorRed.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.TextRed);
            dialog.TextFormatSection.TextColorGreen.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.TextGreen);
            dialog.TextFormatSection.TextColorYellow.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.TextYellow);
            dialog.TextFormatSection.TextColorCustomRGB.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.TextCustomRGB);

            // Text Color
            dialog.TextFormatSection.BackgroundRed.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.BackgroundRed);
            dialog.TextFormatSection.BackgroundGreen.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.BackgroundGreen);
            dialog.TextFormatSection.BackgroundYellow.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.BackgroundYellow);
            dialog.TextFormatSection.BackgroundCustomRGB.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.BackgroundCustomRGB);

            // Special Values Filter
            dialog.SpecialValuesSection.Bitrate.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Bitrate);
            dialog.SpecialValuesSection.ChannelTitle.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.ChannelTitle);
            dialog.SpecialValuesSection.Codec.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Codec);
            dialog.SpecialValuesSection.ColorSpace.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.ColorSpace);
            dialog.SpecialValuesSection.ColorSpaceShort.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.ColorSpaceShort);
            dialog.SpecialValuesSection.HDType.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.HDType);
            dialog.SpecialValuesSection.Resolution.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Resolution);
            dialog.SpecialValuesSection.SDTName.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.SDTName);
            dialog.SpecialValuesSection.SDTProvider.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.SDTProvider);
            dialog.SpecialValuesSection.Timecode.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Timecode);
            dialog.SpecialValuesSection.TransportId.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.TransportId);

            // Tally Actions
            dialog.TallyAndUmdSection.Tally0Background.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally0Background);
            dialog.TallyAndUmdSection.Tally0Light.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally0Light);
            dialog.TallyAndUmdSection.Tally0TextColor.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally0TextColor);
            dialog.TallyAndUmdSection.Tally1Background.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally1Background);
            dialog.TallyAndUmdSection.Tally1Light.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally1Light);
            dialog.TallyAndUmdSection.Tally1TextColor.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.Tally1TextColor);

            // UMD Actions
            dialog.TallyAndUmdSection.Umd0TextAndColor.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD0TextAndColor);
            dialog.TallyAndUmdSection.Umd0Text.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD0Text);
            dialog.TallyAndUmdSection.Umd0Color.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD0Color);
            dialog.TallyAndUmdSection.Umd1TextAndColor.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD1TextAndColor);
            dialog.TallyAndUmdSection.Umd1Text.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD1Text);
            dialog.TallyAndUmdSection.Umd1Color.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.UMD1Color);

            // Alarm
            dialog.AlarmsSection.AlarmBackground.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.AlarmBackground);
            dialog.AlarmsSection.AlarmTextColor.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.AlarmTextColor);
            dialog.AlarmsSection.AlarmCount.Pressed += (sender, args) => dialog.UmdButtonActions.ValueButtonPressed(ButtonActions.ButtonValues.AlarmCount);

            // Bottom Panel
            dialog.BottomPanelButtons.ApplyButton.Pressed += (sender, args) => dialog.ApplySets();
            dialog.BottomPanelButtons.CancelButton.Pressed += (sender, args) => engine.ExitSuccess("UMD Editor Canceled");
        }
    }
}