using System.Text.Json;

namespace Odin.Core.Configuration;

/// <summary>
/// Application settings that are persisted to a JSON file.
/// </summary>
public class AppSettings
{
    private static readonly string SettingsFileName = "odin-settings.json";
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Odin",
        SettingsFileName);

    /// <summary>
    /// Remote server address.
    /// </summary>
    public string RemoteServerAddress { get; set; } = string.Empty;

    /// <summary>
    /// Remote server port.
    /// </summary>
    public int RemoteServerPort { get; set; } = 10300;

    /// <summary>
    /// Local listening port.
    /// </summary>
    public int LocalPort { get; set; } = 10300;

    /// <summary>
    /// Path to the file to lock (e.g., DAoC executable).
    /// </summary>
    public string LockedFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Loads settings from the JSON file.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch
        {
            // If loading fails, return default settings
        }

        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to the JSON file.
    /// </summary>
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail if saving doesn't work
        }
    }

    /// <summary>
    /// Creates a ProxyConfig from these settings.
    /// </summary>
    public ProxyConfig ToProxyConfig()
    {
        return new ProxyConfig
        {
            LocalTcpPort = LocalPort,
            LocalUdpPort = LocalPort,
            RemoteServerAddress = RemoteServerAddress,
            RemoteServerPort = RemoteServerPort
        };
    }
}
