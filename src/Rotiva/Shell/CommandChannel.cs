using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Rotiva.Shell;

/// <summary>
/// Perintah dari menu klik kanan ("--target png file.jpg") diteruskan ke instance Rotiva yang
/// sedang berjalan lewat named pipe. Explorer menjalankan satu proses per file yang dipilih, jadi
/// perintah yang datang berdekatan dikumpulkan dulu: 20 file → satu antrean (dan satu "Gabung").
/// </summary>
internal static class CommandChannel
{
    // Nama pipe per user, supaya user lain di PC yang sama tidak bisa saling kirim perintah.
    static readonly string PipeName = $"Rotiva.Commands.{Environment.UserName}";
    static readonly TimeSpan BatchWindow = TimeSpan.FromMilliseconds(600);

    /// <summary>Kirim ke instance yang sedang berjalan. False kalau tidak tersambung.</summary>
    public static bool TrySend(string targetId, string file)
    {
        // Saat banyak file dipilih, instance pertama mungkin baru mulai; coba beberapa kali.
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                pipe.Connect(1000);
                using var writer = new StreamWriter(pipe, new UTF8Encoding(false));
                writer.WriteLine(targetId);
                writer.WriteLine(file);
                return true;
            }
            catch (Exception ex) when (ex is TimeoutException or IOException)
            {
                Thread.Sleep(100);
            }
        }
        return false;
    }

    /// <summary>
    /// Dengarkan perintah di background. <paramref name="onBatch"/> dipanggil (di thread pemanggil
    /// <see cref="Listen"/>, yaitu UI thread) per kode tujuan, dengan semua file yang terkumpul.
    /// </summary>
    public static void Listen(Action<string, IReadOnlyList<string>> onBatch)
    {
        var context = SynchronizationContext.Current!;
        var pending = new Dictionary<string, List<string>>();
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = BatchWindow };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var batches = pending.ToList();
            pending.Clear();
            foreach (var (targetId, files) in batches)
                onBatch(targetId, files);
        };

        // Dipakai juga untuk perintah milik proses ini sendiri (kalau dijalankan langsung dari menu).
        Received = (targetId, file) => context.Post(_ =>
        {
            if (!pending.TryGetValue(targetId, out var files))
                pending[targetId] = files = [];
            if (!files.Contains(file, StringComparer.OrdinalIgnoreCase))
                files.Add(file);
            timer.Stop();
            timer.Start();
        }, null);

        new Thread(ServerLoop) { IsBackground = true, Name = "Rotiva command pipe" }.Start();
    }

    /// <summary>Tambahkan perintah ke kumpulan yang sedang menunggu (dari thread mana pun).</summary>
    public static Action<string, string> Received { get; private set; } = (_, _) => { };

    static void ServerLoop()
    {
        while (true)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
                pipe.WaitForConnection();
                using var reader = new StreamReader(pipe, Encoding.UTF8);
                string? targetId = reader.ReadLine();
                string? file = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(targetId) && !string.IsNullOrWhiteSpace(file))
                    Received(targetId, file);
            }
            catch (IOException)
            {
                // Klien putus di tengah jalan; tunggu klien berikutnya.
            }
        }
    }
}
