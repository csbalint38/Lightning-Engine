using Editor.Common.Enums;
using Editor.GameCode;
using Editor.Utilities;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Editor.Config
{
    public static class ConfigManager
    {
        private const string _configFileName = "preferences.json";

        private static readonly string _configFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LightningEngine",
            _configFileName
         );

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public static Preferences Config { get; private set; }

        static ConfigManager()
        {
            _jsonOptions.Converters.Add(new JsonStringEnumConverter(
                JsonNamingPolicy.SnakeCaseLower,
                allowIntegerValues: false
            ));

            Config = GetDefault();
        }

        public static void TryLoadConfig()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Config = GetDefault();

                    using var create = File.Create(_configFilePath);
                    JsonSerializer.Serialize(create, Config, _jsonOptions);
                }

                using var read = File.OpenRead(_configFilePath);

                var loaded = JsonSerializer.Deserialize<Preferences>(read, _jsonOptions);

                loaded ??= GetDefault();

                Config = loaded;
            }
            catch (JsonException)
            {
                Logger.LogAsync(LogLevel.WARNING, "Failed to load Editor preferences. Falling back to defaults.");

                Config = GetDefault();

                var create = File.Create(_configFilePath);
                JsonSerializer.Serialize(create, Config, _jsonOptions);
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);

                if (directory is not null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var write = File.Create(_configFilePath);
                JsonSerializer.Serialize(write, Config, _jsonOptions);
            }
            catch
            {
                Logger.LogAsync(LogLevel.WARNING, "Failed to save Editor preferences.");
            }
        }

        private static Preferences GetDefault() => new()
        {
            CodeConfig = new()
            {
                CodeEditor = CodeEditor.NOTEPAD,
                MSBuildPath = MSBuild.FindMSBuild() ?? string.Empty
            }
        };
    }
}
