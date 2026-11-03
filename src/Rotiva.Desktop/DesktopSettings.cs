using System.Text.Json;

namespace Rotiva.Desktop;

public enum WheelSize { Small, Medium, Large }

/// <summary>
/// Pengaturan Rotiva di macOS/Linux, disimpan sebagai JSON di folder data aplikasi
/// (~/Library/Application Support/Rotiva di macOS, ~/.config/Rotiva di Linux).
/// </summary>
sealed class DesktopSettings
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Rotiva", "settings.json");

    public WheelSize WheelSize { get; set; } = WheelSize.Medium;

    /// <summary>Skala roda terhadap ukuran desainnya (sama dengan versi Windows).</summary>
    public double WheelScale => WheelSize switch
    {
        WheelSize.Small => 0.65,
        WheelSize.Large => 1.0,
        _ => 0.8,
    };

    public static DesktopSettings Load()
    {
        try
        {
            return File.Exists(FilePath)
                ? JsonSerializer.Deserialize<DesktopSettings>(File.ReadAllText(FilePath)) ?? new()
                : new();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Pengaturan tidak tersimpan; tetap berlaku sampai Rotiva ditutup.
        }
    }
}
