using System.IO;
using Rotiva.Models;

namespace Rotiva.Conversion;

/// <summary>
/// Menjalankan konversi satu per satu (drop berikutnya menunggu giliran) dan
/// melaporkan progresnya lewat <see cref="IConversionReporter"/>.
/// </summary>
public sealed class ConversionQueue
{
    readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Dipanggil di UI thread: <see cref="Progress{T}"/> dibuat di sini, jadi laporan progres dan
    /// hasil kembali ke UI thread lewat SynchronizationContext-nya.
    /// </summary>
    public async void Enqueue(IReadOnlyList<string> files, ConvertTarget target, IConversionReporter reporter)
    {
        await _gate.WaitAsync();
        try
        {
            if (target.Action is TargetAction.MergePdf or TargetAction.Zip)
            {
                await CombineAsync(files, target, reporter);
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
                reporter.ShowProgress(header, $"{name}   →   {target.Label}");

                var progress = new Progress<double?>(reporter.SetProgress);
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
                string message = files.Count == 1 && target.Action == TargetAction.ExtractText && TryCopyText(reporter, lastOutput!)
                    ? $"Text copied to clipboard · {Path.GetFileName(lastOutput)}"
                    : files.Count == 1
                    ? $"Saved as {Path.GetFileName(lastOutput)}"
                    : target.Action == TargetAction.Compress
                        ? $"{files.Count} files compressed"
                        : $"{files.Count} files converted to {target.Label}";
                reporter.ShowResult(success: true, "DONE", message, revealPath: lastOutput);
            }
            else
            {
                string header = succeeded == 0 ? "FAILED" : $"{succeeded} SUCCEEDED, {failures.Count} FAILED";
                reporter.ShowResult(success: false, header, failures[0]);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    static bool TryCopyText(IConversionReporter reporter, string textFile)
    {
        try
        {
            return reporter.TryCopyText(File.ReadAllText(textFile));
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>Semua file jadi satu hasil: gabung PDF atau ZIP.</summary>
    static async Task CombineAsync(IReadOnlyList<string> files, ConvertTarget target, IConversionReporter reporter)
    {
        bool zip = target.Action == TargetAction.Zip;
        string items = files.Count == 1 ? Path.GetFileName(files[0].TrimEnd('\\', '/')) : $"{files.Count} items";
        reporter.ShowProgress(zip ? "ZIPPING" : "MERGING", $"{items}   →   {target.Label}");
        var progress = new Progress<double?>(reporter.SetProgress);
        try
        {
            string output = await Task.Run(() => zip
                ? Converter.ZipAsync(files, progress)
                : Converter.MergeToPdfAsync(files, progress));
            string message = zip
                ? $"Saved as {Path.GetFileName(output)}"
                : $"{files.Count} files merged into {Path.GetFileName(output)}";
            reporter.ShowResult(success: true, "DONE", message, revealPath: output);
        }
        catch (Exception ex) when (ex is ConversionException or IOException or UnauthorizedAccessException
                                       or System.ComponentModel.Win32Exception)
        {
            reporter.ShowResult(success: false, "FAILED", ex.Message);
        }
    }
}
