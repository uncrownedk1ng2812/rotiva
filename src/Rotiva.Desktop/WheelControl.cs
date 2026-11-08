using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using Rotiva.Localization;
using Rotiva.Models;

namespace Rotiva.Desktop;

/// <summary>
/// Roda format Rotiva, digambar sendiri dalam satu control (tanpa elemen per segmen) supaya ringan.
/// Ukuran dan warna sama dengan roda versi Windows. Seleksi mengikuti arah kursor dari titik tengah;
/// lingkaran tengah = batal.
/// </summary>
sealed class WheelControl : Control, ICustomHitTest
{
    public const double OuterRadius = 280;
    public const double InnerRadius = 122;
    public const double CenterRadius = 108;
    const double GapPx = 6;
    const double PopDistance = 12;
    const double EdgeMargin = 44;
    const double SelectorRadius = OuterRadius + 12;
    const double RingRadius = OuterRadius + 30;
    const int MinSlots = 4;

    // Kecepatan animasi: berapa bagian dari 0..1 per detik.
    const double OpenSpeed = 1 / 0.22;
    const double HoverSpeed = 1 / 0.11;
    const double SelectorSpeed = 1 / 0.15;

    static readonly Color SegmentColor = Color.FromArgb(0xF0, 0x1E, 0x1E, 0x24);
    static readonly Color EmptyColor = Color.FromArgb(0x80, 0x16, 0x16, 0x1A);
    static readonly Color HoverColor = Color.FromArgb(0xFA, 0xF4, 0xF4, 0xF6);
    static readonly Color LabelColor = Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF);
    static readonly Color LabelHoverColor = Color.FromRgb(0x10, 0x10, 0x12);
    public static readonly Color AccentColor = Color.FromRgb(0xFF, 0x8A, 0x1F);

    public static readonly FontFamily Font = new("fonts:Inter#Inter, $Default");
    static readonly Typeface Bold = new(Font, FontStyle.Normal, FontWeight.Bold);
    static readonly Typeface Regular = new(Font);

    static readonly IBrush Halo = new RadialGradientBrush
    {
        GradientStops =
        {
            new GradientStop(Color.FromArgb(0xC0, 0, 0, 0), 0),
            new GradientStop(Color.FromArgb(0x90, 0, 0, 0), 0.75),
            new GradientStop(Color.FromArgb(0x00, 0, 0, 0), 1),
        },
    };
    static readonly IPen TickPen = new Pen(new SolidColorBrush(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF)), 3, new DashStyle([0.35, 5], 0));
    static readonly IPen SegmentPen = new Pen(new SolidColorBrush(Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF)), 1);
    static readonly IPen SelectorPen = new Pen(new SolidColorBrush(AccentColor), 5, lineCap: PenLineCap.Round);
    static readonly IPen SelectorGlowPen = new Pen(new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0x8A, 0x1F)), 15, lineCap: PenLineCap.Round);
    static readonly IBrush DiscBrush = new SolidColorBrush(Color.FromArgb(0xFA, 0x14, 0x14, 0x18));
    static readonly IPen DiscPen = new Pen(new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)), 1.5);
    static readonly IBrush AccentBrush = new SolidColorBrush(AccentColor);
    static readonly IBrush MutedBrush = new SolidColorBrush(Color.FromArgb(0xC8, 0xFF, 0xFF, 0xFF));

    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(15) };
    readonly Stopwatch _clock = new();
    FileCategory? _category;
    IReadOnlyList<ConvertTarget> _targets = [];
    Geometry[] _wedges = [];
    Geometry? _selectorArc;
    double[] _lit = [];
    double _open;
    int _selected = -1;
    bool _inCenter = true;
    double _pointerAngle;
    double _selectorAngle;
    double _selectorOpacity;
    double _scale = 1;

    public WheelControl()
    {
        _timer.Tick += (_, _) => Step();
        _dwell.Tick += (_, _) =>
        {
            _dwell.Stop();
            if (Highlighted is { Action: TargetAction.Group })
                OpenHighlightedGroup();
        };
        Focusable = true;
    }

    /// <summary>Sisi roda dalam satuan desain (sebelum diskalakan).</summary>
    public static double DesignSize => (OuterRadius + EdgeMargin) * 2;

    /// <summary>Segmen dipilih (klik atau Enter).</summary>
    public event Action<ConvertTarget>? TargetChosen;

    /// <summary>Klik di tengah atau di slot kosong.</summary>
    public event Action? Cancelled;

    /// <summary>Pilihan yang akan dikerjakan; null untuk irisan yang hanya membuka halaman (EDIT, BACK).</summary>
    public ConvertTarget? Selected => Highlighted is { Action: not TargetAction.Group } target ? target : null;

    ConvertTarget? Highlighted => _selected >= 0 ? _targets[_selected] : null;

    // Kursor berhenti sebentar di irisan EDIT/BACK: halamannya berganti (cara ini jalan juga selama drag).
    readonly DispatcherTimer _dwell = new() { Interval = TimeSpan.FromMilliseconds(200) };
    IReadOnlyList<ConvertTarget> _rootTargets = [];

    /// <summary>Drag mode: pilihan dibuat dengan melepas file, bukan dengan klik.</summary>
    public bool ChooseOnRelease { get; set; }

    double Center => DesignSize / 2;

    int SlotCount => _wedges.Length;

    /// <summary>
    /// Drag mode sebelum file masuk: ukuran roda sudah dipesan (jendela diletakkan di kursor),
    /// tapi belum ada yang digambar.
    /// </summary>
    public void Prepare(double scale)
    {
        _category = null;
        _targets = [];
        _wedges = [];
        _scale = scale;
        Width = Height = DesignSize * scale;
        InvalidateVisual();
    }

    /// <summary>Arahkan ke titik di control ini (dipakai selama drag: drag tidak memicu PointerMoved).</summary>
    public void PointAt(Point position) =>
        UpdatePointer((position - new Point(Bounds.Width / 2, Bounds.Height / 2)) / _scale);

    public void Load(FileCategory category, IReadOnlyList<ConvertTarget> targets, double scale)
    {
        _category = category;
        _rootTargets = targets;
        _scale = scale;
        Width = Height = DesignSize * scale;
        _inCenter = true;
        ShowPage(targets);
        Animate();
    }

    void ShowPage(IReadOnlyList<ConvertTarget> targets)
    {
        _dwell.Stop();
        _targets = targets;

        // Pilihan sedikit: sisanya slot kosong, supaya roda tidak jadi satu cincin besar.
        int slots = Math.Max(targets.Count, MinSlots);
        double span = 360.0 / slots;
        _wedges = Enumerable.Range(0, slots).Select(i => CreateWedge(i * span - span / 2, i * span + span / 2)).ToArray();
        _selectorArc = CreateArc(-span / 2, span / 2, SelectorRadius, 2);
        _lit = new double[slots];
        _open = 0;
        _selected = -1;
        _selectorOpacity = 0;
    }

    void OpenHighlightedGroup()
    {
        var group = Highlighted!;
        double pointer = _selected * 360.0 / SlotCount;
        int selected;
        if (group.Children is null)
        {
            selected = _rootTargets.ToList().FindIndex(t => t.Action == TargetAction.Group);
            ShowPage(_rootTargets);
        }
        else
        {
            int count = group.Children.Count + 1;
            selected = (int)Math.Round(pointer / (360.0 / count)) % count;
            ShowPage(FormatCatalog.PageOf(group, selected));
        }
        // Tanpa memulai jeda lagi: kursor masih diam di sini, jangan sampai halaman bolak-balik sendiri.
        Select(selected, startDwell: false);
        Animate();
    }

    /// <summary>Pilih berdasarkan arah dari titik tengah (satuan desain).</summary>
    void UpdatePointer(Vector fromCenter)
    {
        if (SlotCount == 0)
            return;
        _inCenter = fromCenter.Length < CenterRadius;
        if (_inCenter)
        {
            Select(-1);
            return;
        }
        double angle = Math.Atan2(fromCenter.X, -fromCenter.Y) * 180 / Math.PI;
        _pointerAngle = angle < 0 ? angle + 360 : angle;
        double span = 360.0 / SlotCount;
        int index = (int)Math.Floor((_pointerAngle + span / 2) / span) % SlotCount;
        Select(index < _targets.Count ? index : -1);
    }

    /// <summary>Panah kiri/kanan: pindah ke segmen sebelah (keyboard).</summary>
    public void StepSelection(int direction)
    {
        if (_targets.Count == 0)
            return;
        int next = _selected < 0 ? 0 : (_selected + direction + _targets.Count) % _targets.Count;
        _inCenter = false;
        _pointerAngle = next * 360.0 / SlotCount;
        Select(next);
    }

    public void ChooseSelected()
    {
        if (Highlighted is { Action: TargetAction.Group })
            OpenHighlightedGroup();
        else if (Selected is { } target)
            TargetChosen?.Invoke(target);
        else
            Cancelled?.Invoke();
    }

    void Select(int index, bool startDwell = true)
    {
        if (index == _selected)
        {
            InvalidateVisual();
            return;
        }
        _dwell.Stop();
        if (startDwell && index >= 0 && _targets[index].Action == TargetAction.Group)
            _dwell.Start();
        if (index >= 0 && _selected < 0)
            _selectorAngle = index * 360.0 / SlotCount;
        _selected = index;
        Animate();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        UpdatePointer((e.GetPosition(this) - new Point(Bounds.Width / 2, Bounds.Height / 2)) / _scale);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        UpdatePointer((e.GetPosition(this) - new Point(Bounds.Width / 2, Bounds.Height / 2)) / _scale);
        ChooseSelected();
        e.Handled = true;
    }

    public bool HitTest(Point point) => true;

    void Animate()
    {
        if (!_timer.IsEnabled)
        {
            _clock.Restart();
            _timer.Start();
        }
        InvalidateVisual();
    }

    void Step()
    {
        // Langkah dibatasi: frame yang telat tidak membuat animasi melompat jauh.
        double dt = Math.Min(_clock.Elapsed.TotalSeconds, 0.05);
        _clock.Restart();
        bool moving = false;

        _open = Approach(_open, 1, OpenSpeed * dt, ref moving);
        for (int i = 0; i < _lit.Length; i++)
            _lit[i] = Approach(_lit[i], i == _selected ? 1 : 0, HoverSpeed * dt, ref moving);
        _selectorOpacity = Approach(_selectorOpacity, _selected >= 0 ? 1 : 0, HoverSpeed * dt, ref moving);
        if (_selected >= 0)
        {
            // Indikator meluncur lewat jalur terpendek.
            double target = _selected * 360.0 / SlotCount;
            double delta = ((target - _selectorAngle) % 360 + 540) % 360 - 180;
            double step = delta * Math.Min(1, SelectorSpeed * 3 * dt);
            _selectorAngle = Math.Abs(delta) < 0.2 ? target : _selectorAngle + step;
            moving |= Math.Abs(delta) >= 0.2;
        }

        InvalidateVisual();
        if (!moving)
            _timer.Stop();
    }

    static double Approach(double value, double target, double step, ref bool moving)
    {
        if (Math.Abs(target - value) <= step)
            return target;
        moving = true;
        return value + Math.Sign(target - value) * step;
    }

    public override void Render(DrawingContext context)
    {
        if (_category is null)
            return;

        // Buka: membesar dari 88% sambil muncul (ease-out).
        double eased = 1 - Math.Pow(1 - _open, 3);
        double zoom = 0.88 + 0.12 * eased;
        var transform = Matrix.CreateScale(zoom, zoom) * Matrix.CreateTranslation(Center, Center) * Matrix.CreateScale(_scale, _scale);
        using var opacity = context.PushOpacity(eased);
        using var placed = context.PushTransform(transform);

        context.DrawEllipse(Halo, null, new Point(0, 0), Center, Center);
        context.DrawEllipse(null, TickPen, new Point(0, 0), RingRadius, RingRadius);

        double span = 360.0 / SlotCount;
        for (int i = 0; i < SlotCount; i++)
            DrawSegment(context, i, span);

        if (_selectorArc is not null && _selectorOpacity > 0)
        {
            using var fade = context.PushOpacity(_selectorOpacity);
            using var rotate = context.PushTransform(Matrix.CreateRotation(_selectorAngle * Math.PI / 180));
            context.DrawGeometry(null, SelectorGlowPen, _selectorArc);
            context.DrawGeometry(null, SelectorPen, _selectorArc);
        }

        DrawCenter(context);
    }

    void DrawSegment(DrawingContext context, int i, double span)
    {
        double mid = i * span;
        double lit = _lit[i];
        var direction = Polar(mid, 1);
        bool empty = i >= _targets.Count;
        using var pop = context.PushTransform(Matrix.CreateTranslation(direction.X * PopDistance * lit, direction.Y * PopDistance * lit));

        var fill = empty ? EmptyColor : Lerp(SegmentColor, HoverColor, lit);
        context.DrawGeometry(new SolidColorBrush(fill), SegmentPen, _wedges[i]);
        if (empty)
            return;

        var label = Text(L.T(_targets[i].Label), Bold, 22 * (1 + 0.1 * lit), new SolidColorBrush(Lerp(LabelColor, LabelHoverColor, lit)));
        var at = Polar(mid, (InnerRadius + OuterRadius) / 2);
        context.DrawText(label, new Point(at.X - label.Width / 2, at.Y - label.Height / 2));
    }

    void DrawCenter(DrawingContext context)
    {
        context.DrawEllipse(DiscBrush, DiscPen, new Point(0, 0), CenterRadius, CenterRadius);

        // Jarum kecil di tepi lingkaran tengah, mengarah ke kursor.
        if (!_inCenter)
        {
            using var rotate = context.PushTransform(Matrix.CreateRotation(_pointerAngle * Math.PI / 180));
            var needle = new StreamGeometry();
            using (var g = needle.Open())
            {
                g.BeginFigure(new Point(0, -(InnerRadius - 2)), true);
                g.LineTo(new Point(8, -(InnerRadius - 14)));
                g.LineTo(new Point(-8, -(InnerRadius - 14)));
                g.EndFigure(true);
            }
            context.DrawGeometry(AccentBrush, null, needle);
        }

        string title, description;
        if (Selected is { } target)
            (title, description) = (L.T(target.Label), L.T(target.Description));
        else if (_inCenter)
            (title, description) = (L.T("CANCEL"), L.T(ChooseOnRelease ? "Release here to cancel" : "Click here to cancel"));
        else
            (title, description) = (L.T("PICK"), L.T("Point at a format"));

        var category = Text(L.T(_category!.Name).ToUpperInvariant(), Bold, 12, AccentBrush);
        var big = Text(title, Bold, title.Length > 7 ? 32 : 42, Brushes.White);
        var small = Text(description, Regular, 13, MutedBrush);
        double top = -(category.Height + big.Height + small.Height) / 2;
        context.DrawText(category, new Point(-category.Width / 2, top));
        context.DrawText(big, new Point(-big.Width / 2, top + category.Height));
        context.DrawText(small, new Point(-small.Width / 2, top + category.Height + big.Height));
    }

    static FormattedText Text(string text, Typeface typeface, double size, IBrush brush) =>
        new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, brush);

    static Color Lerp(Color a, Color b, double t) => Color.FromArgb(
        (byte)(a.A + (b.A - a.A) * t), (byte)(a.R + (b.R - a.R) * t),
        (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));

    // 0° = atas, bertambah searah jarum jam; titik tengah roda di (0, 0).
    static Point Polar(double angleDeg, double radius)
    {
        double a = angleDeg * Math.PI / 180;
        return new Point(radius * Math.Sin(a), -radius * Math.Cos(a));
    }

    // Sudut yang menghasilkan celah selebar GapPx (konstan dalam piksel) pada radius tertentu.
    static double GapAngle(double radius) => Math.Asin(GapPx / 2 / radius) * 180 / Math.PI;

    static Geometry CreateWedge(double start, double end)
    {
        double outerGap = GapAngle(OuterRadius), innerGap = GapAngle(InnerRadius);
        bool large = end - start > 180;
        var geometry = new StreamGeometry();
        using var g = geometry.Open();
        g.BeginFigure(Polar(start + outerGap, OuterRadius), true);
        g.ArcTo(Polar(end - outerGap, OuterRadius), new Size(OuterRadius, OuterRadius), 0, large, SweepDirection.Clockwise);
        g.LineTo(Polar(end - innerGap, InnerRadius));
        g.ArcTo(Polar(start + innerGap, InnerRadius), new Size(InnerRadius, InnerRadius), 0, large, SweepDirection.CounterClockwise);
        g.EndFigure(true);
        return geometry;
    }

    static Geometry CreateArc(double start, double end, double radius, double extraInset)
    {
        double inset = GapAngle(radius) + extraInset;
        var geometry = new StreamGeometry();
        using var g = geometry.Open();
        g.BeginFigure(Polar(start + inset, radius), false);
        g.ArcTo(Polar(end - inset, radius), new Size(radius, radius), 0, end - start > 180, SweepDirection.Clockwise);
        g.EndFigure(false);
        return geometry;
    }
}
