namespace TAG_UMD_Editor
{
    using System;
    using System.Linq;
    using System.Threading;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using Skyline.DataMiner.Utils.InteractiveAutomationScript;

    public class UmdDialog : Dialog
    {
        public UmdDialog(IEngine engine, string elementId, string selectedLayout, string titleIndex) : base(engine)
        {
            var dms = engine.GetDms();

            Tag = new Tag(engine, dms, elementId, selectedLayout, titleIndex);

            Clear();
            Title = "UMD Editor";

            RadioButtonPanel = new UmdRadioButtonPanel(Tag.isMCS);
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

        public Tag Tag { get; set; }

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

        public void ApplySets(IEngine engine)
        {
            var selectedUmd = RadioButtonPanel.UmdRadioButtons.Selected;
            var umdColumnId = GetParamIdBySelectedUmd(selectedUmd);

            if (Tag.isMCS)
            {
                var layoutsTable = Tag.TagElement.GetTable((int)Tag.TagMcs.LayoutsTable);
                Tag.TagElement.GetStandaloneParameter<string>(5999).SetValue(Tag.SelectedLayout); // Layout Drop-down Write
                Thread.Sleep(1000);
                layoutsTable.GetColumn<string>(umdColumnId).SetValue(Tag.TitleIndex, StaticTopPanel.UmdTextBox.Text);
            }
            else
            {
                // TAG MCM Actions
                var tallyLayoutsTable = Tag.TagElement.GetTable((int)Tag.TagMcm.TallyLayouts);
                var tallyLayoutRow = tallyLayoutsTable.GetRows().Where(x => Convert.ToString(x[0]).Contains($"{Tag.SelectedLayout}/{Tag.TitleIndex}"));

                if (tallyLayoutRow.Any())
                {
                    var row = tallyLayoutRow.First();

                    Tag.TagEngineElement.SetParameterByPrimaryKey(umdColumnId, Convert.ToString(row[0]), StaticTopPanel.UmdTextBox.Text);
                    Tag.TagEngineElement.SetParameterByPrimaryKey(2819, Convert.ToString(row[0]), 1);
                }
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
            var umdColumnId = GetReadIdBySelectedUmd(selectedUmd);

            if (Tag.isMCS)
            {
                var layoutsTable = Tag.TagElement.GetTable((int)Tag.TagMcs.LayoutsTable);
                Tag.TagElement.GetStandaloneParameter<string>(5999).SetValue(Tag.SelectedLayout); // Layout Drop-down Write
                Thread.Sleep(1000);
                var umdElementValue = layoutsTable.GetColumn<string>(umdColumnId).GetValue(Tag.TitleIndex, KeyType.PrimaryKey);
                return umdElementValue;
            }
            else
            {
                // TAG MCM Actions
                var tallyLayoutsTable = Tag.TagElement.GetTable((int)Tag.TagMcm.TallyLayouts).GetRows().Where(x => Convert.ToString(x[0]).Contains($"{Tag.SelectedLayout}/{Tag.TitleIndex}"));

                if (tallyLayoutsTable.Any())
                {
                    var row = tallyLayoutsTable.First();
                    return Convert.ToString(row[umdColumnId]);
                }

                return string.Empty;
            }
        }

        private int GetParamIdBySelectedUmd(string selectedValue)
        {
            if (Tag.isMCS)
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return (int)Tag.TagMcs.Umd1Write; // Write
                    case "UMD 2":
                        return (int)Tag.TagMcs.Umd2Write; // Write
                    case "UMD 3":
                        return (int)Tag.TagMcs.Umd3Write; // Write
                    case "UMD 4":
                        return (int)Tag.TagMcs.Umd4Write; // Write
                    default:
                        return 0;
                }
            }
            else // MCM
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return (int)Tag.TagMcm.Umd1Read; // Read
                    case "UMD 2":
                        return (int)Tag.TagMcm.Umd2Read; // Read
                    default:
                        return 0;
                }
            }
        }

        private int GetReadIdBySelectedUmd(string selectedValue)
        {
            if (Tag.isMCS)
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return (int)Tag.TagMcs.Umd1Read;
                    case "UMD 2":
                        return (int)Tag.TagMcs.Umd2Read;
                    case "UMD 3":
                        return (int)Tag.TagMcs.Umd3Read;
                    case "UMD 4":
                        return (int)Tag.TagMcs.Umd4Read;
                    default:
                        return 0;
                }
            }
            else // MCM
            {
                switch (selectedValue)
                {
                    case "UMD 1":
                        return (int)Tag.TagMcm.Umd1Idx;
                    case "UMD 2":
                        return (int)Tag.TagMcm.Umd2Idx;
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

        internal void FocusCursor()
        {
            throw new NotImplementedException();
        }
    }

    public class Tag
    {
        public Tag(IEngine engine, IDms dms, string elementId, string selectedLayout, string titleIndex)
        {
            TagElement = dms.GetElement(new DmsElementId(elementId));
            SelectedLayout = selectedLayout;
            TitleIndex = titleIndex;

            isMCS = TagElement.Protocol.Name.Contains("MCS");

            if (!isMCS)
            {
                var splittedId = elementId.Split('/');
                var dmaId = Convert.ToInt32(splittedId[0]);
                var element = Convert.ToInt32(splittedId[1]);
                TagEngineElement = engine.FindElement(dmaId,element);
            }
        }

        public enum TagMcs
        {
            LayoutsTable = 5000,
            Umd1Read = 5005,
            Umd2Read = 5006,
            Umd3Read = 5007,
            Umd4Read = 5008,
            Umd1Write = 5025,
            Umd2Write = 5026,
            Umd3Write = 5027,
            Umd4Write = 5028,
        }

        public enum TagMcm
        {
            Umd1Idx = 5,
            Umd2Idx = 6,
            Umd1Read = 2806,
            Umd2Read = 2807,
            TallyLayouts = 2800,
        }

        public IDmsElement TagElement { get; set; }

        public Element TagEngineElement { get; set; }

        public string SelectedLayout { get; set; }

        public string TitleIndex { get; set; }

        public bool isMCS { get; set; }
    }
}
