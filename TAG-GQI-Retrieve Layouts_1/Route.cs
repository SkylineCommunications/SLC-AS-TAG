namespace GQI_EVSCerebrum_GetRoutesForDestination_1
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Messages;
    using System;
    using System.Collections.Generic;
    using System.Net;

    internal class Route
    {
        public Route()
        {
        }

        public Route(object[] row)
        {
            
        }

        public string Instance { get; set; }

        public string Source { get; set; }

        public string SourceLevel { get; set; }

        public string Destination { get; set; }

        public string DestinationLevel { get; set; }

        public static List<Route> CreateRoutes(ParameterValue[] columns)
        {
            if (columns == null || columns.Length == 0) return new List<Route>();

            var routes = new List<Route>();

            for (int i = 0; i < columns[0].ArrayValue.Length; i++)
            {
                var nameValue = columns[5].ArrayValue[i]?.CellValue?.GetAsStringValue();
                if (string.IsNullOrWhiteSpace(nameValue) || nameValue == "Not initialized") continue;

                var route = new Route
                {
                    Instance = columns[0].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Source = columns[4].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    SourceLevel = columns[6].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    Destination = columns[8].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                    DestinationLevel = columns[10].ArrayValue[i]?.CellValue?.GetAsStringValue(),
                };

                routes.Add(route);
            }

            return routes;
        }

        public GQIRow ToRow()
        {
            var row = new GQIRow(
                new[]
                {
                    new GQICell { Value = Destination },
                    new GQICell { Value = DestinationLevel },
                    new GQICell { Value = Source },
                    new GQICell { Value = SourceLevel },
                });

            return row;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Source) && !string.IsNullOrWhiteSpace(SourceLevel) && !string.IsNullOrWhiteSpace(Destination) && !string.IsNullOrWhiteSpace(DestinationLevel);
        }
    }
}