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
        public LayoutEditor(IEngine engine) : base(engine)
        {
            Title = "Edit Layout Position";

            Label = new Label("Select a Channel:");
            ChannelsDropDown = new DropDown();
            UpdateButton = new Button("Update");
            CancelButton = new Button("Cancel");

            AddWidget(Label, 1, 0);
            AddWidget(UpdateButton, 4, 1, HorizontalAlignment.Right);
            AddWidget(CancelButton, 5, 1, HorizontalAlignment.Right);

            Label.Width = 400;
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
            var channelsList = new List<string>();

            if (element.Protocol.Name.Contains("MCM"))
            {
                var tableData = element.GetTable(Script.MCMTableId).GetData();
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
                var tableData = element.GetTable(Script.MCMTableId).GetData();
                foreach (var row in tableData.Values)
                {
                    channelsList.Add(Convert.ToString(row[2 /* Label */]));
                }
            }

            this.ChannelsDropDown.Options = channelsList;
            this.ChannelsDropDown.Selected = String.Empty;
        }
    }
}