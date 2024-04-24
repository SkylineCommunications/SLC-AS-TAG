namespace Common.TableClasses
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Outputs
    {
        public int TableId { get; }

        public int LayoutsColumnId { get; }

        public int OutputColumnId { get; }

        public Outputs(int outputsTableId, int outputs_LayoutsColumnId, int outputsTable_OutputColumnId)
        {
            TableId = outputsTableId;
            LayoutsColumnId = outputs_LayoutsColumnId;
            OutputColumnId = outputsTable_OutputColumnId;
        }
    }
}
