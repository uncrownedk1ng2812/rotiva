using Microsoft.Win32;

namespace Rotiva;

public enum WheelSize { Small, Medium, Large }

/// <summary>Pengaturan kecil aplikasi, disimpan di HKCU\Software\Rotiva (sama dengan pilihan menu klik kanan).</summary>
internal static class AppSettings
{
    const string SettingsKey = @"Software\Rotiva";
    const string WheelSizeValue = "WheelSize";
    const string LanguageValue = "Language";

    public static WheelSize WheelSize
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue(WheelSizeValue) is int value && Enum.IsDefined(typeof(WheelSize), value)
                ? (WheelSize)value
                : WheelSize.Medium;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            key.SetValue(WheelSizeValue, (int)value, RegistryValueKind.DWord);
        }
    }

    /// <summary>Kode bahasa UI ("en", "id", ...); kosong = ikuti bahasa Windows.</summary>
    public static string Language
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue(LanguageValue) as string ?? "";
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKey);
            key.SetValue(LanguageValue, value, RegistryValueKind.String);
        }
    }

    /// <summary>Skala roda terhadap ukuran desainnya (<see cref="Controls.RadialWheel.Size"/>).</summary>
    public static double WheelScale => WheelSize switch
    {
        WheelSize.Small => 0.65,
        WheelSize.Large => 1.0,
        _ => 0.8,
    };
}
