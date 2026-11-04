using Microsoft.Win32;
using Rotiva.Localization;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Rotiva;

/// <summary>Ikon di system tray: satu-satunya UI saat aplikasi sedang menunggu.</summary>
internal static class TrayIcon
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string RunValue = "Rotiva";
    const string LicenseUrl = "https://github.com/uncrownedk1ng2812/rotiva/blob/main/NOTICE";

    /// <param name="showWelcome">False kalau dijalankan dari menu klik kanan (notifikasi konversi sudah cukup).</param>
    public static Forms.NotifyIcon Create(Action onExit, bool showWelcome = true)
    {
        var icon = new Forms.NotifyIcon
        {
            Icon = Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? Drawing.SystemIcons.Application,
            Text = "Rotiva",
            Visible = true,
        };
        // Klik kiri maupun kanan membuka menu Rotiva (TrayMenuWindow), bukan ContextMenuStrip bawaan.
        icon.MouseUp += (_, e) =>
        {
            if (e.Button is Forms.MouseButtons.Left or Forms.MouseButtons.Right)
                TrayMenuWindow.ShowAtCursor(onExit);
        };
        if (showWelcome)
        {
            icon.ShowBalloonTip(3000, L.T("Rotiva is running"),
                L.T("Drag a file, then press Shift to open the conversion wheel."), Forms.ToolTipIcon.None);
        }
        return icon;
    }

    // Ringkasan lisensi (NOTICE) dari installer; kalau tidak ada (mis. build dev), buka versi online.
    public static void OpenLicense()
    {
        var local = System.IO.Path.Combine(AppContext.BaseDirectory, "NOTICE.txt");
        var target = System.IO.File.Exists(local) ? local : LicenseUrl;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target) { UseShellExecute = true });
    }

    // Nilai yang sama juga ditulis oleh installer (opsi "jalankan otomatis").
    public static bool IsAutostartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(RunValue) is not null;
    }

    public static void SetAutostart(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
            key.SetValue(RunValue, $"\"{Environment.ProcessPath}\"");
        else
            key.DeleteValue(RunValue, throwOnMissingValue: false);
    }
}
