using FactorioNexus.ApplicationPresentation.Extensions;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FactorioNexus.Services
{
    public static partial class ApplicationSettingsManager
    {
        private static readonly object IoSyncObj = new object();

        private static string ConfigFilePath
        {
            get
            {
                string nearAppCfgPath = Path.Combine(Environment.CurrentDirectory, "config.cfg");
                if (Directory.Exists(nearAppCfgPath))
                    return nearAppCfgPath;

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio-nexus", "config.cfg");
            }
        }

        public static SettingsContainer Current
        {
            get;
            private set;
        }

        static ApplicationSettingsManager()
        {
            Current = new SettingsContainer();
            LoadSettings();
        }

        public static void SaveSettings()
        {
            lock (IoSyncObj)
            {
                // Checking if file exists
                string configFilePath = ConfigFilePath;
                if (File.Exists(configFilePath))
                    File.Delete(configFilePath); // Deleting if exists

                // Creating new config file
                using StreamWriter configWriter = File.CreateText(configFilePath);
                foreach (PropertyInfo property in Current.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    try
                    {
                        // Checking if property has value
                        if (!Current.HasValue(property.Name))
                            continue;

                        // Getting value to write
                        object? value = property.GetValue(Current, null);
                        if (value == null)
                            continue;

                        // Formatting and writing value
                        string formatedLine = string.Format("{0} = {1}", property.Name, value);
                        configWriter.WriteLine(formatedLine);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to write property {0}. {1}", [property.Name, ex]);
                        continue;
                    }
                }
            }
        }

        public static void LoadSettings()
        {
            lock (IoSyncObj)
            {
                // Creating new container
                Current = new SettingsContainer();
                Type containerType = Current.GetType();
                Regex lineParser = LineParserRegex();

                // Checking if file exists
                string configFilePath = ConfigFilePath;
                if (!File.Exists(configFilePath))
                    return; // Leave container with default values if doesn't exists

                // Reading config file's lines
                foreach (string line in File.ReadLines(configFilePath))
                {
                    try
                    {
                        // Trying to match the config line. Pattern 'name = value'
                        Match match = lineParser.Match(line);
                        if (!match.Success)
                            continue;

                        // Getting property associated with this property
                        PropertyInfo? property = containerType.GetProperty(match.Groups[1].Value, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                        if (property == null)
                            continue;

                        // Trying to cast parsed value to property's type
                        object? settingValue = CastPropertyValue(property.PropertyType.Name, match.Groups[2].Value);
                        if (settingValue == null)
                            continue;

                        // Setting container property with parsed value
                        property.SetValue(Current, settingValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Faile to parse line {0}. {1}", [line, ex]);
                        continue;
                    }
                }
            }
        }

        private static object? CastPropertyValue(string typeName, string stringValue) => typeName switch
        {
            nameof(String) => stringValue,
            nameof(Int32) => int.Parse(stringValue),
            _ => null
        };

        [GeneratedRegex(@"(\S+)\s*=\s*(\S+)")]
        private static partial Regex LineParserRegex();
    }

    public class SettingsContainer : ViewModelBase
    {
        private static readonly string _gamedataDirectory_Default = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Factorio");

        private string? _gamedataDirectory = null;

        public string GamedataDirectory
        {
            get => _gamedataDirectory ?? _gamedataDirectory_Default;
            set => Set(ref _gamedataDirectory, value);
        }

        public bool HasValue(string propertyValue) => null != (propertyValue switch
        {
            nameof(GamedataDirectory) => _gamedataDirectory,
            _ => null
        });
    }
}
