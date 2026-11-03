using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Rotiva.Controls;
using Rotiva.Conversion;
using Rotiva.Interop;
using Rotiva.Models;
using Rotiva.Sound;

namespace Rotiva;

/// <summary>
/// Jendela roda yang muncul di posisi kursor saat Shift ditekan sambil men-drag file.
/// Alurnya: Shift ditekan → <see cref="BeginProbe"/> (jendela tak terlihat menunggu DragEnter)
/// → kalau yang di-drag file yang didukung, roda dibuka → file di-drop ke segmen.
/// </summary>
public partial class OverlayWindow : Window
{
    static readonly Duration OpenDuration = TimeSpan.FromMilliseconds(220);
    static readonly Duration CloseDuration = TimeSpan.FromMilliseconds(140);
    static readonly Brush TabActiveBrush = Brushes.White;
    static readonly Brush TabInactiveBrush = new SolidColorBrush(Color.FromArgb(0x73, 0xFF, 0xFF, 0xFF));
    static readonly Brush AccentBrush = new SolidColorBrush(RadialWheel.AccentColor);
    // Ruang kosong di sekeliling roda (satuan desain): kursor boleh sedikit keluar dari roda
    // tanpa langsung keluar dari jendela.
    const double StagePadding = 40;
    // Tab dan petunjuk masuk ke margin tepi roda supaya jendela tetap ringkas.
    const double BarOverlap = 28;

    enum OverlayState { Hidden, Probing, Open, Closing }

    readonly ConversionQueue _conversions = new();
    readonly ToastWindow _toast = new();
    OverlayState _state;
    FileCategory? _category;
    string[] _files = [];
    // Titik tengah roda dalam koordinat jendela (DIP).
    Point _wheelCenter;
    double _scale = 1;
    int _dragVersion;

    public OverlayWindow()
    {
        InitializeComponent();
        BuildInstructions();
        Wheel.SelectionChanged += SoundFx.Tick;
        SourceInitialized += (_, _) =>
            NativeMethods.MakeOverlayWindow(new WindowInteropHelper(this).Handle, clickThrough: false);
        // Buat HWND sekarang supaya kemunculan pertama tidak tersendat.
        new WindowInteropHelper(this).EnsureHandle();
    }

    /// <summary>Shift ditekan saat drag: tampilkan jendela tak terlihat dan tunggu DragEnter.</summary>
    public void BeginProbe()
    {
        if (_state != OverlayState.Hidden)
            return;

        _category = null;
        PlaceAtCursor();
        _state = OverlayState.Probing;
        Show();
    }

    /// <summary>
    /// Susun roda, tab, dan petunjuk dalam ukuran desain, lalu taruh jendela supaya titik tengah
    /// roda tepat di kursor (digeser kalau terpotong pinggir monitor).
    /// </summary>
    void PlaceAtCursor()
    {
        _scale = AppSettings.WheelScale;
        StageScale.ScaleX = StageScale.ScaleY = _scale;

        RefreshTabs();
        var unbounded = new Size(double.PositiveInfinity, double.PositiveInfinity);
        TabsBar.Measure(unbounded);
        InstructionsBar.Measure(unbounded);
        var tabs = TabsBar.DesiredSize;
        var instructions = InstructionsBar.DesiredSize;

        double side = RadialWheel.Size;
        double stageWidth = Math.Max(side, Math.Max(tabs.Width, instructions.Width)) + StagePadding * 2;
        double wheelTop = StagePadding + tabs.Height - BarOverlap;
        double instructionsTop = wheelTop + side - BarOverlap;
        double stageHeight = instructionsTop + instructions.Height + StagePadding;
        double centerX = stageWidth / 2;

        Stage.Width = stageWidth;
        Stage.Height = stageHeight;
        Canvas.SetLeft(Wheel, centerX - side / 2);
        Canvas.SetTop(Wheel, wheelTop);
        Canvas.SetLeft(TabsBar, centerX - tabs.Width / 2);
        Canvas.SetTop(TabsBar, StagePadding);
        Canvas.SetLeft(InstructionsBar, centerX - instructions.Width / 2);
        Canvas.SetTop(InstructionsBar, instructionsTop);
        _wheelCenter = new Point(centerX * _scale, (wheelTop + side / 2) * _scale);

        double dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        var bounds = NativeMethods.GetMonitorBoundsAtCursor();
        NativeMethods.GetCursorPos(out var cursor);
        Width = stageWidth * _scale;
        Height = stageHeight * _scale;
        double left = bounds.X / dpi, top = bounds.Y / dpi;
        double right = (bounds.X + bounds.Width) / dpi, bottom = (bounds.Y + bounds.Height) / dpi;
        Left = Math.Clamp(cursor.X / dpi - _wheelCenter.X, left, Math.Max(left, right - Width));
        Top = Math.Clamp(cursor.Y / dpi - _wheelCenter.Y, top, Math.Max(top, bottom - Height));
    }

    /// <summary>Tombol mouse dilepas di mana pun.</summary>
    public async void OnGlobalDragEnded()
    {
        if (_state == OverlayState.Probing)
        {
            HideOverlay();
            return;
        }
        if (_state != OverlayState.Open)
            return;

        // Drop diproses sesudah tombol mouse dilepas; tunggu sebentar sebelum menutup.
        int version = _dragVersion;
        await Task.Delay(400);
        if (_state == OverlayState.Open && version == _dragVersion)
            CloseWheel();
    }

    void OnDragEnter(object sender, DragEventArgs e)
    {
        _dragVersion++;
        if (_state == OverlayState.Probing)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files
                && FormatCatalog.ForFile(files[0], multipleFiles: files.Length > 1) is { } category)
            {
                _files = files;
                _category = category;
                Open();
            }
            else
            {
                // Bukan file atau folder (misal teks dari browser).
                Dispatcher.BeginInvoke(HideOverlay);
            }
        }
        e.Effects = CurrentEffect();
        e.Handled = true;
    }

    void OnDragOver(object sender, DragEventArgs e)
    {
        _dragVersion++;
        if (_state == OverlayState.Open)
            Wheel.UpdatePointer((e.GetPosition(this) - _wheelCenter) / _scale);
        e.Effects = CurrentEffect();
        e.Handled = true;
    }

    async void OnDragLeave(object sender, DragEventArgs e)
    {
        // DragLeave juga terpanggil saat drag dibatalkan (Esc). Tunggu sebentar untuk
        // memastikan kursor tidak masuk lagi sebelum menutup roda.
        int version = ++_dragVersion;
        await Task.Delay(80);
        if (version == _dragVersion)
            CloseWheel();
    }

    async void OnDrop(object sender, DragEventArgs e)
    {
        _dragVersion++;
        e.Handled = true;
        if (_state != OverlayState.Open || _category is null || Wheel.SelectedTarget is not { } target)
        {
            e.Effects = DragDropEffects.None;
            CloseWheel();
            return;
        }

        // Selalu Copy: jangan sampai Explorer menganggapnya "Move" lalu menghapus file asli.
        e.Effects = DragDropEffects.Copy;
        _state = OverlayState.Closing;
        Wheel.FlashSelected();
        SoundFx.Confirm();

        // Gabung PDF menerima campuran gambar/PDF/dokumen, diurutkan seperti di Explorer; ZIP menerima
        // semuanya, termasuk folder. Selain itu hanya file sejenis yang bisa dikonversi ke format ini.
        var files = target.Action switch
        {
            TargetAction.MergePdf => FilesToMerge(_files),
            TargetAction.Zip => _files.ToList(),
            _ => _files
                .Where(file => FormatCatalog.ForFile(file) is var category
                    && category.Name == _category.Name && category.Targets.Contains(target))
                .ToList(),
        };
        var area = CurrentMonitorArea();
        await Task.Delay(200);
        CloseWheel();
        if (files.Count > 0)
            Convert(files, target, area);
    }

    /// <summary>Antrekan konversi; progres dan hasilnya tampil sebagai toast di <paramref name="area"/>.</summary>
    void Convert(IReadOnlyList<string> files, ConvertTarget target, Rect area) =>
        _conversions.Enqueue(files, target, new ToastReporter(_toast, area));

    static List<string> FilesToMerge(IEnumerable<string> files) =>
        files.Where(FormatCatalog.CanMergeToPdf)
            .OrderBy(Path.GetFileName, Comparer<string?>.Create((a, b) => NativeMethods.StrCmpLogicalW(a!, b!)))
            .ToList();

    /// <summary>
    /// Perintah dari menu klik kanan Explorer (sudah dikumpulkan per kode tujuan). File dari jenis
    /// berbeda bisa punya kode yang sama ("png" untuk gambar maupun PDF), jadi diproses per kategori.
    /// </summary>
    public void ConvertFromShell(string targetId, IReadOnlyList<string> files)
    {
        var area = CurrentMonitorArea();
        if (targetId == FormatCatalog.ZipId)
        {
            var items = files.Where(path => File.Exists(path) || Directory.Exists(path)).ToList();
            if (items.Count > 0)
                Convert(items, FormatCatalog.ForFile(items[0], multipleFiles: true).Targets
                    .First(t => t.Action == TargetAction.Zip), area);
            return;
        }

        var existing = files.Where(File.Exists).ToList();
        if (existing.Count == 0)
            return;

        if (targetId == FormatCatalog.MergeId)
        {
            var mergeable = FilesToMerge(existing);
            var merge = mergeable.Count > 0
                ? FormatCatalog.ForFile(mergeable[0], multipleFiles: true).Targets.First(t => t.Action == TargetAction.MergePdf)
                : null;
            if (merge is not null)
                Convert(mergeable, merge, area);
            return;
        }

        foreach (var group in existing.GroupBy(file => FormatCatalog.ForFile(file).Name))
        {
            var target = FormatCatalog.ForFile(group.First()).Targets.FirstOrDefault(t => FormatCatalog.IdOf(t) == targetId);
            // Format tujuan sama dengan format asli (misal "Ke PNG" untuk file .png): dilewati.
            var sameTarget = group.Where(file => FormatCatalog.ForFile(file).Targets.Contains(target)).ToList();
            if (target is not null && sameTarget.Count > 0)
                Convert(sameTarget, target, area);
        }
    }

    /// <summary>Area monitor tempat kursor berada, dalam satuan WPF (DIP), untuk posisi notifikasi.</summary>
    Rect CurrentMonitorArea()
    {
        var bounds = NativeMethods.GetMonitorBoundsAtCursor();
        double scale = VisualTreeHelper.GetDpi(this).DpiScaleX;
        return new Rect(bounds.X / scale, bounds.Y / scale, bounds.Width / scale, bounds.Height / scale);
    }

    DragDropEffects CurrentEffect() =>
        _state == OverlayState.Open && Wheel.SelectedTarget is not null ? DragDropEffects.Copy : DragDropEffects.None;

    void Open()
    {
        Wheel.Load(_category!);
        RefreshTabs();
        // Tab aktif ditulis tebal, jadi lebarnya sedikit berubah: tengahkan lagi.
        TabsBar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(TabsBar, Stage.Width / 2 - TabsBar.DesiredSize.Width / 2);
        _state = OverlayState.Open;
        Animate(open: true);
        Wheel.PlayEntrance();
        SoundFx.Open();
    }

    async void CloseWheel()
    {
        if (_state == OverlayState.Probing)
            HideOverlay();
        if (_state is OverlayState.Hidden or OverlayState.Probing)
            return;

        _state = OverlayState.Closing;
        Animate(open: false);
        await Task.Delay(CloseDuration.TimeSpan);
        if (_state == OverlayState.Closing)
            HideOverlay();
    }

    void HideOverlay()
    {
        Hide();
        _state = OverlayState.Hidden;
    }

    void Animate(bool open)
    {
        var duration = open ? OpenDuration : CloseDuration;
        IEasingFunction ease = open
            ? new CubicEase { EasingMode = EasingMode.EaseOut }
            : new QuadraticEase { EasingMode = EasingMode.EaseIn };

        var scale = (ScaleTransform)Wheel.RenderTransform;
        var scaleAnimation = open
            ? new DoubleAnimation(0.88, 1, duration) { EasingFunction = ease }
            : new DoubleAnimation(0.9, duration) { EasingFunction = ease };
        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        Wheel.BeginAnimation(OpacityProperty, new DoubleAnimation(open ? 1 : 0, duration));
        TabsBar.BeginAnimation(OpacityProperty, new DoubleAnimation(open ? 1 : 0, duration));
        InstructionsBar.BeginAnimation(OpacityProperty, new DoubleAnimation(open ? 1 : 0, duration));
    }

    void RefreshTabs()
    {
        Tabs.Children.Clear();
        foreach (var category in FormatCatalog.Categories)
        {
            bool active = category.Name == _category?.Name;
            Tabs.Children.Add(new Border
            {
                Margin = new Thickness(12, 0, 12, 0),
                Padding = new Thickness(0, 4, 0, 4),
                BorderBrush = AccentBrush,
                BorderThickness = new Thickness(0, 0, 0, active ? 3 : 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = category.Name.ToUpperInvariant(),
                    FontFamily = RadialWheel.LabelFont,
                    FontSize = 16,
                    FontWeight = active ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = active ? TabActiveBrush : TabInactiveBrush,
                },
            });
        }
    }

    void BuildInstructions()
    {
        (string Label, string Key)[] items =
        [
            ("Convert", "Release"),
            ("Cancel", "Esc"),
        ];
        foreach (var (label, key) in items)
        {
            Instructions.Children.Add(new TextBlock
            {
                Text = label,
                FontFamily = RadialWheel.LabelFont,
                FontSize = 14,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 8, 0),
            });
            Instructions.Children.Add(KeyChip(key));
        }
    }

    static Border KeyChip(string key) => new()
    {
        Background = Brushes.White,
        CornerRadius = new CornerRadius(3),
        Padding = new Thickness(7, 2, 7, 2),
        MinWidth = 24,
        VerticalAlignment = VerticalAlignment.Center,
        Child = new TextBlock
        {
            Text = key,
            FontFamily = RadialWheel.LabelFont,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11)),
            HorizontalAlignment = HorizontalAlignment.Center,
        },
    };
}
