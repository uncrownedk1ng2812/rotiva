using System.IO;
using System.Windows;
using Rotiva.Models;

namespace Rotiva.Conversion;

/// <summary>
/// Menjalankan konversi satu per satu (drop berikutnya menunggu giliran) dan
/// melaporkan progresnya lewat <see cref="ToastWindow"/>.
/// </summary>
internal sealed class ConversionQueue(ToastWindow toast)
{
    readonly SemaphoreSlim _gate = new(1, 1);

    /// <param name="area">Area monitor (DIP) tempat notifikasi ditampilkan.</param>
    public async void Enqueue(IReadOnlyList<string> files, ConvertTarget target, Rect area)
    {
        await _gate.WaitAsync();
        try
        {
            if (target.Action is TargetAction.MergePdf or TargetAction.Zip)
            {
                await CombineAsync(files, target, area);
                return;
            }

            string? lastOutput = null;
            var failures = new List<string>();
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string name = Path.GetFileName(file);
                string verb = target.Action switch
                {
                    TargetAction.Compress => "COMPRESSING",
                    TargetAction.ExtractText => "READING TEXT",
                    TargetAction.Extract => "EXTRACTING",
                    _ => "CONVERTING",
                };
                string header = files.Count > 1 ? $"{verb} {i + 1}/{files.Count}" : verb;
                toast.ShowProgress(header, $"{name}   →   {target.Label}", area);

                // Progress<T> dibuat di UI thread, jadi laporannya otomatis kembali ke UI thread.
                var progress = new Progress<double?>(toast.SetProgress);
                try
                {
                    lastOutput = await Task.Run(() => Converter.ConvertAsync(file, target, progress));
                }
                catch (Exception ex) when (ex is ConversionException or IOException or UnauthorizedAccessException
                                               or System.ComponentModel.Win32Exception)
                {
                    failures.Add($"{name}: {ex.Message}");
                }
            }

            int succeeded = files.Count - failures.Count;
            if (failures.Count == 0)
            {
                string message = files.Count == 1 && target.Action == TargetAction.ExtractText && TryCopyText(lastOutput!)
                    ? $"Text copied to clipboard · {Path.GetFileName(lastOutput)}"
                    : files.Count == 1
                    ? $"Saved as {Path.GetFileName(lastOutput)}"
                    : target.Action == TargetAction.Compress
                        ? $"{files.Count} files compressed"
                        : $"{files.Count} files converted to {target.Label}";
                toast.ShowResult(success: true, "DONE", message, area, revealPath: lastOutput);
            }
            else
            {
                string header = succeeded == 0 ? "FAILED" : $"{succeeded} SUCCEEDED, {failures.Count} FAILED";
                toast.ShowResult(success: false, header, failures[0], area);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    // Dipanggil di UI thread (clipboard butuh STA). Clipboard bisa sedang dikunci aplikasi lain.
    static bool TryCopyText(string textFile)
    {
        try
        {
            Clipboard.SetText(File.ReadAllText(textFile));
            return true;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or IOException)
        {
            return false;
        }
    }

    /// <summary>Semua file jadi satu hasil: gabung PDF atau ZIP.</summary>
    async Task CombineAsync(IReadOnlyList<string> files, ConvertTarget target, Rect area)
    {
        bool zip = target.Action == TargetAction.Zip;
        string items = files.Count == 1 ? Path.GetFileName(files[0].TrimEnd('\\', '/')) : $"{files.Count} items";
        toast.ShowProgress(zip ? "ZIPPING" : "MERGING", $"{items}   →   {target.Label}", area);
        var progress = new Progress<double?>(toast.SetProgress);
        try
        {
            string output = await Task.Run(() => zip
                ? Converter.ZipAsync(files, progress)
                : Converter.MergeToPdfAsync(files, progress));
            string message = zip
                ? $"Saved as {Path.GetFileName(output)}"
                : $"{files.Count} files merged into {Path.GetFileName(output)}";
            toast.ShowResult(success: true, "DONE", message, area, revealPath: output);
        }
        catch (Exception ex) when (ex is ConversionException or IOException or UnauthorizedAccessException
                                       or System.ComponentModel.Win32Exception)
        {
            toast.ShowResult(success: false, "FAILED", ex.Message, area);
        }
    }
}
