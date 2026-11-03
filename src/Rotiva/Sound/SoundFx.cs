using System.IO;
using System.Media;
using System.Text;

namespace Rotiva.Sound;

/// <summary>
/// Efek suara kecil yang disintesis saat runtime, jadi tidak perlu file audio.
/// </summary>
internal static class SoundFx
{
    const int SampleRate = 44100;

    static readonly SoundPlayer TickPlayer = Create(30, t =>
        Math.Sin(2 * Math.PI * 2200 * t) * Math.Exp(-t * 350) * 0.3);

    static readonly SoundPlayer ConfirmPlayer = Create(180, t =>
        (Math.Sin(2 * Math.PI * 520 * t) + 0.5 * Math.Sin(2 * Math.PI * 1040 * t)) * Math.Exp(-t * 26) * 0.3);

    static readonly SoundPlayer OpenPlayer = CreateWhoosh(260);

    public static void Tick() => Play(TickPlayer);

    public static void Confirm() => Play(ConfirmPlayer);

    public static void Open() => Play(OpenPlayer);

    static void Play(SoundPlayer player)
    {
        try
        {
            player.Play();
        }
        catch (InvalidOperationException)
        {
            // Tidak ada perangkat audio: abaikan saja.
        }
    }

    // Noise yang di-low-pass dengan cutoff naik: bunyi "whoosh" pendek.
    static SoundPlayer CreateWhoosh(int milliseconds)
    {
        var random = new Random(7);
        double length = milliseconds / 1000.0;
        double filtered = 0;
        return Create(milliseconds, t =>
        {
            double noise = random.NextDouble() * 2 - 1;
            filtered += (0.03 + 0.25 * t / length) * (noise - filtered);
            return filtered * Math.Sin(Math.PI * t / length) * 0.9;
        });
    }

    static SoundPlayer Create(int milliseconds, Func<double, double> sample)
    {
        int count = SampleRate * milliseconds / 1000;
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("RIFF"u8);
            writer.Write(36 + count * 2);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)1); // mono
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(count * 2);
            for (int i = 0; i < count; i++)
                writer.Write((short)(Math.Clamp(sample((double)i / SampleRate), -1, 1) * short.MaxValue));
        }

        stream.Position = 0;
        var player = new SoundPlayer(stream);
        player.Load();
        return player;
    }
}
