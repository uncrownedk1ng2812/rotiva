using Microsoft.Win32;
using Rotiva.Interop;
using Rotiva.Localization;
using Windows.ApplicationModel;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace Rotiva;

/// <summary>Ikon di system tray: satu-satunya UI saat aplikasi sedang menunggu.</summary>
internal static class TrayIcon
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string RunValue = "Rotiva";
    const string LicenseUrl = "https://github.com/uncrownedk1ng2812/rotiva/blob/main/NOTICE";

    static Forms.NotifyIcon? _icon;
    static Drawing.Icon? _plainIcon, _badgeIcon;
    static Action? _balloonClick;

    /// <param name="showWelcome">False kalau dijalankan dari menu klik kanan (notifikasi konversi sudah cukup).</param>
    public static Forms.NotifyIcon Create(Action onExit, bool showWelcome = true)
    {
        _plainIcon = Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? Drawing.SystemIcons.Application;
        var icon = new Forms.NotifyIcon
        {
            Icon = _plainIcon,
            Text = "Rotiva",
            Visible = true,
        };
        _icon = icon;
        icon.BalloonTipClicked += (_, _) => _balloonClick?.Invoke();
        icon.BalloonTipClosed += (_, _) => _balloonClick = null;
        Updater.AvailableChanged += ShowUpdateState;
        PinToTaskbarOnce();
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

    /// <summary>
    /// Windows 11 menyembunyikan ikon tray aplikasi baru di balik tombol "^". Sekali saja (sesudah instalasi)
    /// ikon Rotiva dimunculkan di taskbar; kalau pengguna menyembunyikannya lagi, pilihan itu dihormati.
    /// Entri NotifyIconSettings baru dibuat Windows sesudah ikon tampil, jadi dicoba beberapa kali.
    /// </summary>
    static void PinToTaskbarOnce()
    {
        if (AppSettings.TrayPinned)
            return;
        int attempts = 0;
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (_, _) =>
        {
            attempts++;
            bool done;
            try { done = PromoteTrayIcon(); }
            catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or System.IO.IOException) { done = false; }
            if (done)
                AppSettings.TrayPinned = true;
            if (done || attempts >= 10)
                timer.Stop();
        };
        timer.Start();
    }

    static bool PromoteTrayIcon()
    {
        using var root = Registry.CurrentUser.OpenSubKey(@"Control Panel\NotifyIconSettings");
        if (root is null)
            return true; // Windows 10: ikon tray tidak disembunyikan dengan cara ini.
        string exe = Environment.ProcessPath ?? "";
        foreach (var name in root.GetSubKeyNames())
        {
            using var key = root.OpenSubKey(name, writable: true);
            if (key?.GetValue("ExecutablePath") is string path && path.Equals(exe, StringComparison.OrdinalIgnoreCase))
            {
                key.SetValue("IsPromoted", 1, RegistryValueKind.DWord);
                return true;
            }
        }
        return false;
    }

    /// <summary>Notifikasi balon dari ikon tray; <paramref name="onClick"/> dipanggil kalau balon diklik.</summary>
    public static void Notify(string title, string text, Action? onClick = null)
    {
        if (_icon is null)
            return;
        _balloonClick = onClick;
        _icon.ShowBalloonTip(5000, title, text, Forms.ToolTipIcon.None);
    }

    /// <summary>Titik oranye di ikon tray + tooltip selama ada update yang belum dipasang.</summary>
    static void ShowUpdateState()
    {
        if (_icon is null || _plainIcon is null)
            return;
        if (Updater.Available is { } release)
        {
            _badgeIcon ??= WithBadge(_plainIcon);
            _icon.Icon = _badgeIcon;
            _icon.Text = L.F("Rotiva: version {0} is available", release.Version.ToString(3));
        }
        else
        {
            _icon.Icon = _plainIcon;
            _icon.Text = "Rotiva";
        }
    }

    static Drawing.Icon WithBadge(Drawing.Icon source)
    {
        int size = Forms.SystemInformation.SmallIconSize.Width;
        using var bitmap = new Drawing.Bitmap(size, size);
        using (var g = Drawing.Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var sized = new Drawing.Icon(source, size, size))
                g.DrawIcon(sized, new Drawing.Rectangle(0, 0, size, size));
            float dot = size * 0.42f, at = size - dot;
            using var ring = new Drawing.SolidBrush(Drawing.Color.FromArgb(0x16, 0x16, 0x1A));
            using var fill = new Drawing.SolidBrush(Drawing.Color.FromArgb(0xFF, 0x8A, 0x1F));
            g.FillEllipse(ring, at - 1, at - 1, dot + 1, dot + 1);
            g.FillEllipse(fill, at, at, dot - 1, dot - 1);
        }
        // Handle dari GetHicon tidak dibebaskan: ikon ini dipakai sampai aplikasi keluar.
        return Drawing.Icon.FromHandle(bitmap.GetHicon());
    }

    // Ringkasan lisensi (NOTICE) dari installer; kalau tidak ada (mis. build dev), buka versi online.
    public static void OpenLicense()
    {
        var local = System.IO.Path.Combine(AppContext.BaseDirectory, "NOTICE.txt");
        var target = System.IO.File.Exists(local) ? local : LicenseUrl;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target) { UseShellExecute = true });
    }

    // Versi installer: nilai yang sama juga ditulis oleh installer (opsi "jalankan otomatis").
    // Versi MSIX (Store): registry-nya divirtualisasi, jadi pakai StartupTask dari manifest paket.
    public static bool IsAutostartEnabled()
    {
        if (NativeMethods.IsPackaged)
            return StartupTask().State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(RunValue) is not null;
    }

    public static void SetAutostart(bool enabled)
    {
        if (NativeMethods.IsPackaged)
        {
            var task = StartupTask();
            if (enabled)
                task.RequestEnableAsync().AsTask().GetAwaiter().GetResult();
            else
                task.Disable();
            return;
        }
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
            key.SetValue(RunValue, $"\"{Environment.ProcessPath}\"");
        else
            key.DeleteValue(RunValue, throwOnMissingValue: false);
    }

    // TaskId sama dengan <desktop:StartupTask> di installer/msix/AppxManifest.xml.
    static StartupTask StartupTask() =>
        Windows.ApplicationModel.StartupTask.GetAsync("RotivaStartup").AsTask().GetAwaiter().GetResult();
}
