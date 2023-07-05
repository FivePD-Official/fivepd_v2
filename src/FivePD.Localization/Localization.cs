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
        private const string LocalizationFileName = "en-US.json";
        private static readonly string ServerStaticsDirPath = AppDomain.CurrentDomain.BaseDirectory + "../../FivePD.Statics/Localizations";
        private static readonly string LocalizationDirPath = AppDomain.CurrentDomain.BaseDirectory + "../Localization";
        private static readonly string LocalizationPath = LocalizationDirPath + '/' + LocalizationFileName;
        private static readonly string OptimizedLocalizationPath = ServerStaticsDirPath + '/' + LocalizationFileName;
        private static readonly string OptimizedTestLocalizationPath = ServerStaticsDirPath + "/en-US-TEST.json";

        public static void Main()
        {
            FormatLocalization();
            OptimizeLocalization();
        }

        private static void FormatLocalization()
        {
            try
            {
                var promotedKeys = new[] { "TITLE" };
                var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LocalizationPath));
                var orderedJson = json
                    .OrderBy(x => !promotedKeys.Contains(x.Key))
                    .ThenBy(x => x.Key);
                var formattedOrderedJson = JsonConvert.SerializeObject(
                    orderedJson.ToDictionary(item => item.Key.ToString(), item => item.Value.ToString()),
                    Formatting.Indented);
                File.WriteAllText(LocalizationPath, formattedOrderedJson);
                Console.WriteLine("Localization formatted.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void OptimizeLocalization()
        {
            try
            {
                var localization = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LocalizationPath));
                var unflattenedLocalization = JsonHelper.Unflatten(localization);
                var formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                File.WriteAllText(OptimizedLocalizationPath, formattedLocalization);
                Console.WriteLine("Localization optimized.");

                foreach (var key in localization.Keys)
                {
                    localization[key] = $"T: {localization[key]}";
                }

                unflattenedLocalization = JsonHelper.Unflatten(localization);
                formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                File.WriteAllText(OptimizedTestLocalizationPath, formattedLocalization);
                Console.WriteLine("Test localization created.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}