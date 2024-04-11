namespace Merge_LCA_themes_1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Web.Common.v1.Dashboards;

    /// <summary>
    /// Represents a DataMiner Automation script.
    /// </summary>
    public class Script
    {
        private static readonly string _themesFilePath = @"C:\Skyline DataMiner\dashboards\Themes.json";

        /// <summary>
        /// The script entry point.
        /// </summary>
        /// <param name="engine">Link with SLAutomation process.</param>
        public void Run(IEngine engine)
        {
            var newThemesFilePath = engine.GetScriptParam("New Themes File Path")?.Value;

            if (string.IsNullOrEmpty(newThemesFilePath))
            {
                throw new FileNotFoundException("Empty or invalid filepath provided");
            }

            if (!File.Exists(newThemesFilePath))
            {
                throw new FileNotFoundException("Themes file could not be found.");
            }

            var appThemeText = File.ReadAllText(newThemesFilePath);
            var appThemes = GetThemesFromText(appThemeText);

            AddOrUpdateTheme(appThemes);
        }

        private static DMADashboardThemes GetThemesFromText(string themeText)
        {
            var themes = JsonConvert.DeserializeObject<DMADashboardThemes>(themeText);

            if (themes == null)
            {
                throw new ArgumentException("Unable to desirialise the new themes.");
            }

            return themes;
        }

        private static void AddOrUpdateTheme(DMADashboardThemes appThemes)
        {
            var themesFileContent = File.ReadAllText(_themesFilePath);
            var dmaThemes = JsonConvert.DeserializeObject<DMADashboardThemes>(themesFileContent);

            if (dmaThemes == null)
            {
                throw new ArgumentException("The themes file could not be deserialized");
            }

            List<int> themeIds = dmaThemes.Themes.Select(x => x.ID).ToList();

            foreach (DMADashboardTheme appTheme in appThemes.Themes)
            {
                var themeToUpdate = Array.Find(dmaThemes.Themes, x => x.Name == appTheme.Name);

                if (themeToUpdate != null)
                {
                    if (themeToUpdate.ID != appTheme.ID && themeIds.Exists(x => x == appTheme.ID))
                    {
                        appTheme.ID = themeIds.Max() + 1;
                    }

                    int itemIndex = Array.FindIndex(dmaThemes.Themes, x => x.Name == appTheme.Name);
                    dmaThemes.Themes[itemIndex] = appTheme;
                }
                else
                {
                    if (themeIds.Exists(x => x == appTheme.ID))
                    {
                        appTheme.ID = themeIds.Max() + 1;
                    }

                    var themesList = dmaThemes.Themes.ToList();
                    themesList.Add(appTheme);
                    dmaThemes.Themes = themesList.ToArray();
                }
            }

            var newThemesFileContent = JsonConvert.SerializeObject(dmaThemes, Formatting.Indented);
            File.WriteAllText(_themesFilePath, newThemesFileContent);
        }
    }
}