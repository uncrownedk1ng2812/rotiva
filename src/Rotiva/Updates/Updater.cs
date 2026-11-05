using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Threading;
using Rotiva.Interop;
using Rotiva.Updates;

namespace Rotiva;

/// <summary>
/// Update otomatis versi installer: cek rilis terbaru di GitHub, unduh installer-nya, cocokkan
/// SHA-256 dengan SHA256SUMS.txt, lalu jalankan installer secara senyap (Rotiva dibuka lagi sesudahnya).
/// Versi Microsoft Store (MSIX) tidak memakai ini: Store yang meng-update.
/// </summary>
internal static class Updater
{
    static readonly TimeSpan FirstCheck = TimeSpan.FromSeconds(15);
    static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    // Harus di atas Http: CreateClient memakai Current untuk User-Agent.
    public static Version Current { get; } =
        ReleaseInfo.Normalize(Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0));

    static readonly HttpClient Http = CreateClient();
    static DispatcherTimer? _timer;
    static bool _checking;

    public static bool IsSupported => !NativeMethods.IsPackaged;

    /// <summary>Rilis yang lebih baru dari versi terpasang (null = sudah terbaru / belum dicek).</summary>
    public static ReleaseInfo? Available { get; private set; }

    /// <summary>Dipanggil (di thread UI) saat <see cref="Available"/> berubah.</summary>
    public static event Action? AvailableChanged;

    /// <summary>Mulai cek berkala; rilis baru yang belum dilewati langsung dimunculkan.</summary>
    public static void Start()
    {
        if (!IsSupported || _timer is not null)
            return;
        _timer = new DispatcherTimer { Interval = FirstCheck };
        _timer.Tick += async (_, _) =>
        {
            _timer.Interval = CheckInterval;
            if (!AppSettings.CheckForUpdates)
                return;
            try
            {
                var release = await CheckAsync();
                if (release is not null && release.Tag != AppSettings.SkippedVersion)
                    UpdateWindow.ShowFor(release, activate: false);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                // Offline atau GitHub sedang tidak bisa dihubungi: coba lagi di jadwal berikutnya.
            }
        };
        _timer.Start();
    }

    /// <summary>Cek sekarang. Mengembalikan rilis baru, atau null kalau versi ini sudah terbaru.</summary>
    public static async Task<ReleaseInfo?> CheckAsync()
    {
        if (_checking)
            return Available;
        _checking = true;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, ReleaseInfo.LatestApiUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            using var response = await Http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            var release = ReleaseInfo.Parse(json);
            var newer = release is not null && release.IsNewerThan(Current) && release.WindowsInstaller is not null
                ? release
                : null;
            if (newer?.Tag != Available?.Tag)
            {
                Available = newer;
                AvailableChanged?.Invoke();
            }
            return newer;
        }
        finally
        {
            _checking = false;
        }
    }

    /// <summary>"Lewati versi ini": tanda di tray hilang sampai ada versi yang lebih baru lagi.</summary>
    public static void Skip(ReleaseInfo release)
    {
        AppSettings.SkippedVersion = release.Tag;
        if (Available?.Tag == release.Tag)
        {
            Available = null;
            AvailableChanged?.Invoke();
        }
    }

    /// <summary>Unduh installer ke folder sementara dan periksa SHA-256-nya.</summary>
    public static async Task<string> DownloadAsync(ReleaseInfo release, IProgress<(long Done, long Total)> progress,
        CancellationToken cancel)
    {
        var asset = release.WindowsInstaller ?? throw new InvalidOperationException("No Windows installer in this release.");
        string? expected = null;
        if (release.Checksums is { } sums)
            expected = ReleaseInfo.ChecksumFor(await Http.GetStringAsync(sums.DownloadUrl, cancel), asset.Name);

        string folder = Path.Combine(Path.GetTempPath(), "Rotiva-Update");
        Directory.CreateDirectory(folder);
        foreach (var old in Directory.EnumerateFiles(folder))
            TryDelete(old);
        string path = Path.Combine(folder, asset.Name);
        string partial = path + ".part";

        using (var response = await Http.GetAsync(asset.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancel))
        {
            response.EnsureSuccessStatusCode();
            long total = response.Content.Headers.ContentLength ?? asset.Size;
            await using var input = await response.Content.ReadAsStreamAsync(cancel);
            await using var output = File.Create(partial);
            using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[1 << 16];
            long done = 0;
            int read;
            while ((read = await input.ReadAsync(buffer, cancel)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read), cancel);
                sha.AppendData(buffer, 0, read);
                done += read;
                progress.Report((done, total));
            }

            if (asset.Size > 0 && done != asset.Size)
                throw new InvalidDataException($"Download incomplete ({done} of {asset.Size} bytes).");
            string actual = Convert.ToHexString(sha.GetHashAndReset()).ToLowerInvariant();
            if (expected is not null && actual != expected)
                throw new InvalidDataException("The download doesn't match its SHA-256 checksum.");
        }
        File.Move(partial, path, overwrite: true);
        return path;
    }

    /// <summary>
    /// Jalankan installer secara senyap. Mutex instance dilepas dulu (installer menunggu Rotiva tertutup),
    /// lalu aplikasi keluar; installer membuka Rotiva lagi lewat <c>/relaunch=1</c>.
    /// </summary>
    public static void Install(string installer)
    {
        ((App)System.Windows.Application.Current).ReleaseForUpdate();
        Process.Start(new ProcessStartInfo(installer)
        {
            Arguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART /SP- /relaunch=1",
            UseShellExecute = true,
        });
        System.Windows.Application.Current.Shutdown();
    }

    static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        // GitHub API menolak permintaan tanpa User-Agent.
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Rotiva", Current.ToString(3)));
        return client;
    }

    static void TryDelete(string path)
    {
        try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
}
