using System.IO;
using System.Text.Json;

namespace GeminiLiveTranslate.Settings;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string ConfigDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "gemini-live-translate-dotnet");

    public string SettingsPath => Path.Combine(ConfigDirectory, "settings.json");

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), JsonOptions) ?? new AppSettings();
                settings.Normalize();
                return settings;
            }
        }
        catch
        {
            // Corrupt settings should not prevent startup; save will rewrite a valid file.
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        settings.Normalize();
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
