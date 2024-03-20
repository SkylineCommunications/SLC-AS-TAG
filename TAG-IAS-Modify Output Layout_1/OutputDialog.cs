namespace TAG_IAS_Modify_Output_Layout_1
{
	using System;
	using System.Collections.Generic;
	using SharedMethods;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;

	internal class OutputDialog : Dialog
    {
        public OutputDialog(IEngine engine, List<object[]> layoutsPerOutput, List<string> layoutsList, string elementType) : base(engine)
        {
            Layouts = new List<Section>();
            foreach (var layout in layoutsPerOutput)
            {
                var defaultLayout = elementType.Equals("MCM") ? Convert.ToString(layout[11 /* Layouts */]) : Convert.ToString(layout[5 /* Layout */]);
                var rowId = Convert.ToString(layout[0]);
                var layoutPanel = new LayoutsPanel(defaultLayout, layoutsList, rowId);
                Layouts.Add(layoutPanel);
            }

            Title = "Edit Channel Layouts";

            UpdateButton = new Button("Update");
            CancelButton = new Button("Cancel");

            int position = 0;
            foreach (var layout in Layouts)
            {
                AddSection(layout, new SectionLayout(position, 0));
                position++;
            }

            AddWidget(UpdateButton, position, 0, HorizontalAlignment.Right);
            AddWidget(CancelButton, position, 1, HorizontalAlignment.Left);

            UpdateButton.Width = 110;
            CancelButton.Width = 110;
        }

        public List<Section> Layouts { get; private set; }

        public Button UpdateButton { get; private set; }

        public Button CancelButton { get; private set; }

        public void SendLayoutUpdate(Element element, string elementType)
        {
            int columnPid = elementType.Equals("MCM") ? MCM_TablesIDs.EncoderConfigLayoutsColumnId : MCS_TablesIDs.OutputsLayoutsLayoutColumnId;
            foreach (var layoutToUpdate in Layouts)
            {
                var layoutPanel = (LayoutsPanel)layoutToUpdate;
                var key = layoutPanel.RowId.Text;
                element.SetParameterByPrimaryKey(columnPid, key, layoutPanel.Layouts.Selected);
            }
        }
    }

	internal class LayoutsPanel : Section
    {
        public LayoutsPanel(string selectedValue, List<string> layoutsList, string rowId)
        {
            RowId = new Label(rowId);

            AddWidget(LayoutLabel, 0, 0);
            AddWidget(Layouts, 0, 1);
            AddWidget(RowId, 0, 2);

            Layouts.Options = layoutsList;
            Layouts.Selected = selectedValue;

            LayoutLabel.Width = 150;
            RowId.IsVisible = false;
        }

        public Label LayoutLabel { get; } = new Label("Select a Layout:");

        public DropDown Layouts { get; } = new DropDown { Width = 200 };

        public Label RowId { get; private set; }
    }
}