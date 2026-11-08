using System.IO;
using Rotiva.Localization;
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
    /// <param name="finished">Dipanggil sesudah konversi selesai, berhasil atau tidak.</param>
    public async void Enqueue(IReadOnlyList<string> files, ConvertTarget target, IConversionReporter reporter,
        Action? finished = null)
    {
        await _gate.WaitAsync();
        try
        {
            if (target.Action is TargetAction.MergePdf or TargetAction.Zip or TargetAction.BurnSubtitles
                or TargetAction.ZipPassword or TargetAction.ContactSheet)
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
                    TargetAction.Compress => L.T("COMPRESSING"),
                    TargetAction.ExtractText => L.T("READING TEXT"),
                    TargetAction.Extract => L.T("EXTRACTING"),
                    TargetAction.Edit => L.T("EDITING"),
                    _ => L.T("CONVERTING"),
                };
                string header = files.Count > 1 ? $"{verb} {i + 1}/{files.Count}" : verb;
                reporter.ShowProgress(header, $"{name}   →   {L.T(target.Label)}");

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
                    ? L.F("Text copied to clipboard · {0}", Path.GetFileName(lastOutput))
                    : files.Count == 1
                    ? L.F("Saved as {0}", Path.GetFileName(lastOutput))
                    : target.Action == TargetAction.Compress
                        ? L.F("{0} files compressed", files.Count)
                    : target.Action == TargetAction.Edit
                        ? L.F("{0} files edited", files.Count)
                        : L.F("{0} files converted to {1}", files.Count, L.T(target.Label));
                reporter.ShowResult(success: true, L.T("DONE"), message, revealPath: lastOutput);
            }
            else
            {
                string header = succeeded == 0 ? L.T("FAILED") : L.F("{0} SUCCEEDED, {1} FAILED", succeeded, failures.Count);
                reporter.ShowResult(success: false, header, failures[0]);
            }
        }
        finally
        {
            _gate.Release();
            finished?.Invoke();
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
        bool merge = target.Action == TargetAction.MergePdf;
        string header = target.Action switch
        {
            TargetAction.BurnSubtitles => L.T("ADDING SUBTITLES"),
            TargetAction.Zip or TargetAction.ZipPassword => L.T("ZIPPING"),
            TargetAction.ContactSheet => L.T("MAKING SHEET"),
            _ => L.T("MERGING"),
        };
        string items = files.Count == 1 ? Path.GetFileName(files[0].TrimEnd('\\', '/')) : L.F("{0} items", files.Count);
        reporter.ShowProgress(header, $"{items}   →   {L.T(target.Label)}");
        var progress = new Progress<double?>(reporter.SetProgress);
        try
        {
            string output = await Task.Run(() => target.Action switch
            {
                TargetAction.BurnSubtitles => Converter.BurnSubtitlesAsync(files, progress),
                TargetAction.Zip => Converter.ZipAsync(files, progress),
                TargetAction.ZipPassword => Converter.ZipWithPasswordAsync(files, target.Password!),
                TargetAction.ContactSheet => Converter.ContactSheetAsync(files),
                _ => Converter.MergeToPdfAsync(files, progress),
            });
            string message = !merge
                ? L.F("Saved as {0}", Path.GetFileName(output))
                : L.F("{0} files merged into {1}", files.Count, Path.GetFileName(output));
            reporter.ShowResult(success: true, L.T("DONE"), message, revealPath: output);
        }
        catch (Exception ex) when (ex is ConversionException or IOException or UnauthorizedAccessException
                                       or System.ComponentModel.Win32Exception)
        {
            reporter.ShowResult(success: false, L.T("FAILED"), ex.Message);
        }
    }
}
