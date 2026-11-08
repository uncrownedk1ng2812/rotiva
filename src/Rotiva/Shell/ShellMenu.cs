using Microsoft.Win32;
using Rotiva.Interop;
using Rotiva.Models;

namespace Rotiva.Shell;

/// <summary>
/// Menu klik kanan "Rotiva ▸" di Explorer, per ekstensi yang didukung, berisi format tujuan yang
/// sama dengan roda. Ditulis di HKCU (tanpa admin) lewat SystemFileAssociations, jadi tidak mengubah
/// aplikasi default untuk file tersebut. Di Windows 11 menu klasik ini ada di "Show more options"
/// (Shift+F10); menu baru Windows 11 hanya bisa diisi aplikasi berpaket (MSIX).
/// </summary>
internal static class ShellMenu
{
    const string AssociationsKey = @"Software\Classes\SystemFileAssociations";
    const string VerbName = "Rotiva";
    // Menu klik kanan folder (hanya "Kompres jadi ZIP").
    const string FolderVerbKey = @"Software\Classes\Directory\shell\" + VerbName;
    const string SettingsKey = @"Software\Rotiva";
    const string EnabledValue = "ShellMenu";

    /// <summary>Pilihan di menu tray; default menyala.</summary>
    public static bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(SettingsKey);
            return key?.GetValue(EnabledValue) is not 0;
        }
        set
        {
            using (var key = Registry.CurrentUser.CreateSubKey(SettingsKey))
                key.SetValue(EnabledValue, value ? 1 : 0, RegistryValueKind.DWord);
            if (value)
                Register();
            else
                Unregister();
        }
    }

    /// <summary>
    /// Tulis ulang semua menu. Dipanggil tiap aplikasi start, supaya path exe dan daftar format
    /// selalu sesuai versi yang terpasang.
    /// </summary>
    public static void Register()
    {
        string exe = Environment.ProcessPath!;
        Unregister(notify: false);
        foreach (var category in FormatCatalog.Categories)
        {
            foreach (string extension in category.Extensions.Concat(AliasesOf(category.Extensions)))
            {
                // multipleFiles: true supaya "Gabung jadi 1 PDF" dan "ZIP" ikut muncul (menu tidak tahu berapa file dipilih).
                var targets = FormatCatalog.Leaves(FormatCatalog.ForFile("x" + extension, multipleFiles: true)).ToList();
                WriteMenu($@"{AssociationsKey}\{extension}\shell\{VerbName}", targets, exe);
            }
        }
        WriteMenu(FolderVerbKey, FormatCatalog.FolderTargets, exe);
        NativeMethods.NotifyAssociationsChanged();
    }

    static void WriteMenu(string verbKey, IReadOnlyList<ConvertTarget> targets, string exe)
    {
        using var verb = Registry.CurrentUser.CreateSubKey(verbKey);
        verb.SetValue("MUIVerb", "Rotiva");
        verb.SetValue("Icon", $"\"{exe}\",0");
        verb.SetValue("SubCommands", "");
        // Tanpa ini menu hilang kalau lebih dari 15 file dipilih.
        verb.SetValue("MultiSelectModel", "Player");
        for (int i = 0; i < targets.Count; i++)
        {
            // Subkey diurutkan berdasarkan nama, jadi diberi nomor supaya urutannya sama dengan roda.
            using var item = verb.CreateSubKey($@"shell\{i:00}");
            item.SetValue("MUIVerb", FormatCatalog.MenuTextOf(targets[i]));
            item.SetValue("MultiSelectModel", "Player");
            // Garis pemisah sebelum pilihan untuk banyak file (GABUNG, lalu ZIP kalau GABUNG tidak ada).
            bool firstMultiple = targets[i].Action == TargetAction.MergePdf
                || (targets[i].Action == TargetAction.Zip && i > 0 && targets[i - 1].Action != TargetAction.MergePdf);
            if (firstMultiple)
                item.SetValue("CommandFlags", 0x20, RegistryValueKind.DWord); // ECF_SEPARATORBEFORE
            using var command = item.CreateSubKey("command");
            command.SetValue("", $"\"{exe}\" --target {FormatCatalog.IdOf(targets[i])} \"%1\"");
        }
    }

    public static void Unregister() => Unregister(notify: true);

    static void Unregister(bool notify)
    {
        Registry.CurrentUser.DeleteSubKeyTree(FolderVerbKey, throwOnMissingSubKey: false);
        using var associations = Registry.CurrentUser.OpenSubKey(AssociationsKey, writable: true);
        if (associations is null)
        {
            if (notify)
                NativeMethods.NotifyAssociationsChanged();
            return;
        }
        // Semua ekstensi, bukan hanya yang ada di katalog sekarang: bisa jadi versi lama mendaftarkan format lain.
        foreach (string extension in associations.GetSubKeyNames())
        {
            using var shell = associations.OpenSubKey($@"{extension}\shell", writable: true);
            if (shell?.OpenSubKey(VerbName) is { } existing)
            {
                existing.Dispose();
                shell.DeleteSubKeyTree(VerbName, throwOnMissingSubKey: false);
            }
        }
        if (notify)
            NativeMethods.NotifyAssociationsChanged();
    }

    // Ekstensi lain yang dinormalisasi ke ekstensi katalog (".jpeg" → ".jpg", dst).
    static IEnumerable<string> AliasesOf(IReadOnlyList<string> extensions) =>
        FormatCatalog.AliasExtensions.Where(alias => extensions.Contains(FormatCatalog.Normalize(alias)));
}
