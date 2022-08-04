// Copyright (c) GTAPoliceMods LLC. All Rights Reserved.
//
// All code and assets contained in this file are STRICTLY
// the intellectual property of GTAPoliceMods unless otherwise
// noted by a license provided. Do not copy or redistribute.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FivePD.Common.ConfigModels;
using FivePD.Common.Localization;
using Newtonsoft.Json;

namespace FivePD.JsonGenerator
{
    /// <summary>
    /// Generates JSON files from the given types.
    /// </summary>
    public static class JsonGenerator
    {
        private static readonly string[] Directories =
        {
            "Localization",
            "Config",
        };

        private static string rootPath = "../FivePD.Gamemode.Statics";
        private static bool createTestLocalization = true;

        /// <summary>
        /// Generates JSON files from the given types..
        /// </summary>
        /// <param name="args">
        /// If the first arg is set it'll be used as the output path,
        /// otherwise they'll be written to the Statics folder.
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                rootPath = args[0];
                createTestLocalization = false;
            }

            CreateDirectories();

            Write(typeof(Localization), "Localization/en.json");
            if (createTestLocalization)
            {
                Write(CreateLocalizationTestData(new Localization()) as Localization, "Localization/test.json");
            }

            Write(CreateDefaultDatabase(), "Config/database.json");
            Write(CreateDefaultLoadouts(), "Config/loadouts.json");
            Write(CreateDefaultItems(), "Config/items.json");
            Write(CreateDefaultVehicles(), "Config/vehicles.json");
            Write(typeof(MenuConfig), "Config/menu.json");

            Copy("TextFiles/LastNames.txt", "Config/LastNames.txt");
            Copy("TextFiles/MaleFirstNames.txt", "Config/MaleFirstNames.txt");
            Copy("TextFiles/FemaleFirstNames.txt", "Config/FemaleFirstNames.txt");
        }

        private static void CreateDirectories()
        {
            foreach (var dir in Directories)
            {
                var path = $"{rootPath}/{dir}";
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't create directories: {e}");
                }
            }
        }

        private static void Copy(string sourceFileName, string destFileName)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(assemblyFolder))
            {
                return;
            }

            var path = Path.Combine(assemblyFolder, sourceFileName);
            File.Copy(path, $"{rootPath}/{destFileName}", true);
        }

        private static void Write(Type typeOfObject, string path)
        {
            var data = JsonConvert.SerializeObject(Activator.CreateInstance(typeOfObject), Formatting.Indented);
            File.WriteAllText($"{rootPath}/{path}", data);
        }

        private static void Write<T>(T defaultValue, string path)
        {
            var data = JsonConvert.SerializeObject(defaultValue, Formatting.Indented);
            File.WriteAllText($"{rootPath}/{path}", data);
        }

        private static object CreateLocalizationTestData(object o, string parent = null)
        {
            var t = o.GetType();
            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prp in props)
            {
                if (prp.PropertyType.Module.ScopeName != "CommonLanguageRuntimeLibrary")
                {
                    CreateLocalizationTestData(prp.GetValue(o), parent == null ? t.Name : parent + "." + t.Name);
                }
                else
                {
                    var value = prp.GetValue(o);
                    prp.SetValue(o, $"T: {(value is null ? string.Empty : value.ToString())}");
                }
            }

            return o;
        }

        private static List<Loadout> CreateDefaultLoadouts()
        {
            var loadout = new Loadout
            {
                Title = "Loadout 1",
                Weapons = new List<Loadout.Weapon>
                {
                    new Loadout.Weapon
                    {
                        Key = "WEAPON_PISTOL",
                        ComponentKeys = new List<string>
                        {
                            "COMPONENT_PISTOL_CLIP_02",
                            "COMPONENT_AT_PI_FLSH",
                            "COMPONENT_AT_PI_SUPP_02",
                        },
                        Ammunition = 1000,
                    },
                },
            };
            return new List<Loadout>
            {
                loadout,
            };
        }

        private static List<Item> CreateDefaultItems()
        {
            var item = new Item
            {
                Title = "Test item 1",
            };
            var item2 = new Item
            {
                Title = "Test item 2",
                Location = Item.ItemLocation.Vehicle,
            };
            var item3 = new Item
            {
                Title = "Test item 3",
                Location = Item.ItemLocation.Ped,
            };

            return new List<Item>
            {
                item,
                item2,
                item3,
            };
        }

        private static Vehicles CreateDefaultVehicles()
        {
            var vehicles = new Vehicles
            {
                Police = new List<Vehicles.PoliceVehicle>
                {
                    new Vehicles.PoliceVehicle
                    {
                        Name = "Police 1",
                        Model = "police",
                    },
                },
                Ambulance = new List<Vehicles.ServiceVehicle>
                {
                    new Vehicles.ServiceVehicle
                    {
                        Model = "ambulance",
                    },
                },
            };

            return vehicles;
        }

        private static DatabaseConfig CreateDefaultDatabase()
        {
            return new DatabaseConfig
            {
                Address = "127.0.0.1",
                Port = "3306",
                Database = "fivepd",
                User = "root",
                Password = string.Empty,
            };
        }
    }
}