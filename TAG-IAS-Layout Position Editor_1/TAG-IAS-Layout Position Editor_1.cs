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

namespace TAG_IAS_Layout_Position_Editor_1
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
	{
		private const int MCMLayoutsTableId = 10353;
		private const int MCSLayoutsTableId = 5653;

		private static string layoutId;
		private static string position;
		private static LayoutEditor layoutEditor;

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public static void Run(IEngine engine)
		{
            // DO NOT REMOVE THIS COMMENTED-OUT CODE OR THE SCRIPT WON'T RUN!
            // DataMiner evaluates if the script needs to launch in interactive mode.
            // This is determined by a simple string search looking for "engine.ShowUI" in the source code.
            // However, because of the toolkit NuGet package, this string cannot be found here.
            // So this comment is here as a workaround.
            //// engine.ShowUI();
            engine.SetFlag(RunTimeFlags.NoCheckingSets);

            try
            {
                var elementId = GetOneDeserializedValue(engine.GetScriptParam("Element ID").Value);
                layoutId = GetOneDeserializedValue(engine.GetScriptParam("Layout ID").Value);
                position = GetOneDeserializedValue(engine.GetScriptParam("Position").Value);
                var action = GetOneDeserializedValue(engine.GetScriptParam("Action").Value);

                var controller = new InteractiveController(engine);
                layoutEditor = new LayoutEditor(engine);

                var elementData = elementId.Split('/');
                if (elementData.Length < 2)
                {
                    engine.GenerateInformation("Element ID format not supported. Please check the incoming data. [Format: DMA ID/Element ID]");
                    return;
                }

                var dms = engine.GetDms();
                var dmsElement = dms.GetElement(new DmsElementId(Convert.ToInt32(elementData[0]), Convert.ToInt32(elementData[1])));
                var element = engine.FindElementByKey(elementId);
                var tablePid = GetTablePidByElement(dmsElement);

                if (action.ToUpperInvariant().Equals("EDIT"))
                {
                    layoutEditor.GetLayoutsFromElement(dmsElement);

                    layoutEditor.UpdateButton.Pressed += (sender, args) => UpdateLayoutChannel(engine, element, tablePid, layoutEditor.ChannelsDropDown.Selected);
                    layoutEditor.CancelButton.Pressed += (sender, args) => engine.ExitSuccess("Layout Update Canceled");

                    controller.Run(layoutEditor);
                }
                else
                {
                    UpdateLayoutChannel(engine, element, tablePid, "None");
                }
            }
            catch (ScriptAbortException)
            {
                // no action
            }
            catch (Exception ex)
            {
                engine.GenerateInformation($"Exception thrown: {ex}");
            }
        }

		private static void UpdateLayoutChannel(IEngine engine, Element element, int tablePid, string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                engine.GenerateInformation($"Value to set on layout with ID {layoutId} and position {position} is null or empty.");
                return;
            }

            element.SetParameterByPrimaryKey(tablePid, $"{layoutId}/{position}", value);
            engine.ExitSuccess("Layout Title updated");
        }

		private static int GetTablePidByElement(IDmsElement element)
        {
            return element.Protocol.Name.Contains("MCM") ? MCMLayoutsTableId : MCSLayoutsTableId;
        }

		private static string GetOneDeserializedValue(string scriptParam) // [ "value" , "value" ]
        {
            if (scriptParam.Contains("[") && scriptParam.Contains("]"))
            {
                return JsonConvert.DeserializeObject<List<string>>(scriptParam)[0];
            }
            else
            {
                return scriptParam;
            }
        }
    }
}