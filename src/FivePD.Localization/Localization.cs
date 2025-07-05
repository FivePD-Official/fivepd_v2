using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

#pragma warning disable

namespace FivePD.Localization
{
    public static class Localization
    {
        private static readonly string[] LocalizationFileNames = new[] { "en-US.json", "hu-HU.json" };

        private static readonly string ServerStaticsDirPath = AppDomain.CurrentDomain.BaseDirectory + "../../FivePD.Statics/Localizations";
        private static readonly string LocalizationDirPath = AppDomain.CurrentDomain.BaseDirectory + "../Localization";
        private static readonly string LocalizationPath = LocalizationDirPath + '/';
        private static readonly string OptimizedLocalizationPath = ServerStaticsDirPath + '/';
        private static readonly string OptimizedTestLocalizationPath = ServerStaticsDirPath + '/';

        public static void Main()
        {
            foreach (var file in LocalizationFileNames)
            {
                FormatLocalization(file);
                OptimizeLocalization(file, file == "en-US.json");
            }
        }

        private static void FormatLocalization(string file)
        {
            try
            {
                var promotedKeys = new[] { "TITLE" };
                var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LocalizationPath + file));
                var orderedJson = json
                    .OrderBy(x => !promotedKeys.Contains(x.Key))
                    .ThenBy(x => x.Key);
                var formattedOrderedJson = JsonConvert.SerializeObject(
                    orderedJson.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString()),
                    Formatting.Indented);
                File.WriteAllText(LocalizationPath + file, formattedOrderedJson);
                Console.WriteLine("Localization formatted.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void OptimizeLocalization(string file, bool createTestLocalization)
        {
            try
            {
                var localization = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LocalizationPath + file));
                var unflattenedLocalization = JsonHelper.Unflatten(localization);
                var formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                File.WriteAllText(OptimizedLocalizationPath + file, formattedLocalization);
                Console.WriteLine("Localization optimized.");

                if (createTestLocalization)
                {
                    foreach (var key in localization.Keys)
                    {
                        localization[key] = $"T: {localization[key]}";
                    }

                    unflattenedLocalization = JsonHelper.Unflatten(localization);
                    formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                    File.WriteAllText(OptimizedTestLocalizationPath + "en-US-TEST.json", formattedLocalization);
                    Console.WriteLine("Test localization created.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}