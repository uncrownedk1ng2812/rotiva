using System.IO.Pipes;
using System.Text;
using Avalonia.Threading;

namespace Rotiva.Desktop;

/// <summary>
/// Satu Rotiva per user: peluncuran berikutnya ("Open with Rotiva", baris perintah) meneruskan
/// filenya lewat named pipe (di macOS/Linux .NET memakai Unix domain socket) ke instance yang berjalan.
/// Pesannya satu path per baris, diakhiri baris kosong.
/// </summary>
static class InstanceChannel
{
    // Nama per user, supaya user lain di komputer yang sama tidak bisa saling kirim file.
    static readonly string PipeName = $"Rotiva.Desktop.{Environment.UserName}";

    /// <summary>Kirim ke instance yang sedang berjalan. False kalau belum ada yang berjalan.</summary>
    public static bool TrySend(IReadOnlyList<string> files)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            pipe.Connect(300);
            using var writer = new StreamWriter(pipe, new UTF8Encoding(false));
            foreach (string file in files)
                writer.WriteLine(file);
            writer.WriteLine();
            return true;
        }
        catch (Exception ex) when (ex is TimeoutException or IOException)
        {
            return false;
        }
    }

    /// <summary>Dengarkan di background; <paramref name="onFiles"/> dipanggil di UI thread.</summary>
    public static void Listen(Action<IReadOnlyList<string>> onFiles)
    {
        new Thread(() => ServerLoop(onFiles)) { IsBackground = true, Name = "Rotiva instance pipe" }.Start();
    }

    static void ServerLoop(Action<IReadOnlyList<string>> onFiles)
    {
        while (true)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1);
                pipe.WaitForConnection();
                using var reader = new StreamReader(pipe, Encoding.UTF8);
                var files = new List<string>();
                while (reader.ReadLine() is { Length: > 0 } line)
                    files.Add(line);
                if (files.Count > 0)
                    Dispatcher.UIThread.Post(() => onFiles(files));
            }
            catch (IOException)
            {
                // Klien putus di tengah jalan; tunggu klien berikutnya.
            }
        }
    }
}
