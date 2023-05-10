using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using GenHTTP.Engine;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Practices;
using Newtonsoft.Json;

#pragma warning disable

namespace FivePD.Localization
{
    public static class Localization
    {
        private const string LocalizationFileName = "localization.json";
        private static readonly string LocalizationDirPath;
        private static readonly string LocalizationPath;
        private static readonly string OptimizedLocalizationPath;
        private static readonly string OptimizedTestLocalizationPath;
        private static readonly Dictionary<string, string> Localizations = new Dictionary<string, string>();

        static Localization()
        {
            LocalizationDirPath = AppDomain.CurrentDomain.BaseDirectory + "../Localization";
            LocalizationPath = LocalizationDirPath + '/' + LocalizationFileName;
            OptimizedLocalizationPath = LocalizationDirPath + "/Optimized/localization.json";
            OptimizedTestLocalizationPath = LocalizationDirPath + "/Optimized/localization-test.json";

            EnsureDirectories(new[] { LocalizationDirPath + "/Optimized" });
        }

        private static void EnsureDirectories(string[] directories)
        {
            foreach (var item in directories)
            {
                if (!Directory.Exists(item))
                {
                    Directory.CreateDirectory(item);
                }
            }
        }

        private class Options
        {
            [Option('f', "format", Required = false, HelpText = "Formats the original localization file.")]
            public bool Format { get; set; }

            [Option('o', "optimize", Required = false, HelpText = "Optimizes the localization file.")]
            public bool Optimize { get; set; }

            [Option('s', "serve", Required = false, HelpText = "Serves the localization file.")]
            public bool Serve { get; set; }
        }

        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    if (o.Format)
                    {
                        FormatLocalization();
                    }

                    if (o.Optimize)
                    {
                        OptimizeLocalization(o.Serve);
                    }

                    if (o.Serve)
                    {
                        FormatLocalization();
                        OptimizeLocalization();
                        ReadLocalization();
                        ServeLocalization();
                    }
                });
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

        private static void OptimizeLocalization(bool createTestLocalization = false)
        {
            try
            {
                var localization = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(LocalizationPath));
                var unflattenedLocalization = JsonHelper.Unflatten(localization);
                var formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                File.WriteAllText(OptimizedLocalizationPath, formattedLocalization);
                Console.WriteLine("Localization optimized.");

                if (createTestLocalization)
                {
                    foreach (var key in localization.Keys)
                    {
                        localization[key] = $"T: {localization[key]}";
                    }

                    unflattenedLocalization = JsonHelper.Unflatten(localization);
                    formattedLocalization = JsonConvert.SerializeObject(unflattenedLocalization);
                    File.WriteAllText(OptimizedTestLocalizationPath, formattedLocalization);
                    Console.WriteLine("Test localization created.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void ReadLocalization()
        {
            try
            {
                Localizations["en"] = File.ReadAllText(OptimizedLocalizationPath);
                Localizations["test"] = File.ReadAllText(OptimizedTestLocalizationPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void OnLocalizationChange(object source, FileSystemEventArgs e)
        {
            FormatLocalization();
            OptimizeLocalization(true);
            ReadLocalization();
        }

        private static void ServeLocalization()
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = LocalizationDirPath;
            watcher.Filter = LocalizationFileName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnLocalizationChange;
            watcher.EnableRaisingEvents = true;

            var handler = Inline.Create()
                .Get("/localizations", new Func<Dictionary<string, string>>(() => new Dictionary<string, string>
                {
                    { "en", "English" },
                    { "test", "T: English" },
                }))
                .Get("/localizations/:key", new Func<string, string>(key => Localizations[key]));

            ushort port = 1818;
            Console.WriteLine($"Localization server listening on :{port}");
            Host.Create()
                .Console()
                .Defaults()
                .Handler(handler)
                .Port(port)
                .Run();
        }
    }
}