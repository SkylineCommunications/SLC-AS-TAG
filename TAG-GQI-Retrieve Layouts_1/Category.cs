using Skyline.DataMiner.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GQI_TAG_GetEndpoints_1
{
    internal class Category
    {
        public string Instance { get; set; }

        public string Name { get; set; }

        public string Mnemonic { get; set; }

        public static List<Category> CreateCategories(ParameterValue[] columns)
        {
            if (columns == null || columns.Length == 0) return new List<Category>();

            var categories = new List<Category>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var category = new Category
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Name = columns[1].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Mnemonic = columns[3].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                };

                categories.Add(category);
            }

            return categories;
        }
    }
}
