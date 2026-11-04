using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Rotiva.Localization;
using Rotiva.Models;

namespace Rotiva.Controls;

/// <summary>
/// Roda pilihan format ala weapon wheel GTA V. Seleksi ditentukan oleh arah kursor
/// dari titik tengah roda, bukan hover tepat di atas segmen.
/// </summary>
public sealed class RadialWheel : Canvas
{
    public const double OuterRadius = 280;
    public const double InnerRadius = 122;
    const double GapPx = 6;
    const double PopDistance = 12;
    const double EdgeMargin = 44;
    const double SelectorRadius = OuterRadius + 12;
    const double RingRadius = OuterRadius + 30;
    const double CenterRadius = InnerRadius - 14;
    const int MinSlots = 4;

    static readonly Color SegmentInner = Color.FromArgb(0xF0, 0x12, 0x12, 0x16);
    static readonly Color SegmentOuter = Color.FromArgb(0xC8, 0x2C, 0x2C, 0x33);
    static readonly Color EmptyInner = Color.FromArgb(0x90, 0x12, 0x12, 0x16);
    static readonly Color EmptyOuter = Color.FromArgb(0x60, 0x2C, 0x2C, 0x33);
    static readonly Color HoverInner = Color.FromArgb(0xFA, 0xFF, 0xFF, 0xFF);
    static readonly Color HoverOuter = Color.FromArgb(0xF0, 0xCF, 0xCF, 0xD4);
    static readonly Color LabelColor = Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF);
    static readonly Color LabelHoverColor = Color.FromRgb(0x10, 0x10, 0x12);
    public static readonly Color AccentColor = Color.FromRgb(0xFF, 0x8A, 0x1F);

    static readonly Duration HoverDuration = TimeSpan.FromMilliseconds(110);
    public static readonly FontFamily LabelFont = new("Bahnschrift");
    public static readonly FontFamily IconFont = new("Segoe Fluent Icons, Segoe MDL2 Assets");

    readonly List<Segment> _segments = [];
    readonly Canvas _segmentLayer = new();
    readonly Canvas _selector;
    readonly Path _selectorGlow;
    readonly Path _selectorLine;
    readonly RotateTransform _selectorRotation = new(0, Center, Center);
    readonly Path _needle;
    readonly RotateTransform _needleRotation = new(0, Center, Center);
    readonly StackPanel _centerContent;
    readonly TranslateTransform _centerShift = new();
    readonly TextBlock _centerGlyph;
    readonly TextBlock _centerCategory;
    readonly TextBlock _centerTitle;
    readonly TextBlock _centerDescription;
    readonly TextBlock _sourceBadge;
    readonly TextBlock _targetBadge;
    readonly TextBlock _checkMark;
    readonly ScaleTransform _checkScale = new(1, 1);
    readonly Ellipse _confirmRing;
    readonly ScaleTransform _confirmRingScale = new(1, 1);
    FileCategory? _category;
    int _selectedIndex = -1;
    double _selectorAngle;
    bool _selectorShown;
    bool _needleShown;
    // Kursor sedang di lingkaran tengah (zona batal).
    bool _inCenter = true;

    public RadialWheel()
    {
        Width = Height = Size;
        RenderTransformOrigin = new Point(0.5, 0.5);
        RenderTransform = new ScaleTransform(1, 1);
        IsHitTestVisible = false;

        // Bayangan gelap lembut di belakang roda supaya tetap terbaca di latar apa pun.
        var halo = new Ellipse
        {
            Width = Size,
            Height = Size,
            Fill = FrozenRadial(
                (Color.FromArgb(0xC0, 0, 0, 0), 0),
                (Color.FromArgb(0x90, 0, 0, 0), 0.75),
                (Color.FromArgb(0x00, 0, 0, 0), 1)),
        };
        Children.Add(halo);

        // Cincin tick statis. Dulu berputar terus, tapi animasi tanpa henti membuat WPF
        // menggambar ulang seluruh jendela tiap frame selama roda terbuka.
        var ticks = new Ellipse
        {
            Width = RingRadius * 2,
            Height = RingRadius * 2,
            Stroke = Frozen(Color.FromArgb(0x55, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 3,
            StrokeDashArray = [0.35, 5],
        };
        Place(ticks, RingRadius);
        Children.Add(ticks);

        var outline = new Ellipse
        {
            Width = (OuterRadius + 21) * 2,
            Height = (OuterRadius + 21) * 2,
            Stroke = Frozen(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 1,
        };
        Place(outline, OuterRadius + 21);
        Children.Add(outline);

        Children.Add(_segmentLayer);

        // Pendar indikator: garis lebar transparan di bawah garis utama. Jauh lebih murah daripada
        // DropShadowEffect, yang harus di-blur ulang tiap frame animasi.
        _selectorGlow = new Path
        {
            Stroke = Frozen(Color.FromArgb(0x40, AccentColor.R, AccentColor.G, AccentColor.B)),
            StrokeThickness = 15,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            RenderTransform = _selectorRotation,
        };
        _selectorLine = new Path
        {
            Stroke = Frozen(AccentColor),
            StrokeThickness = 5,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            RenderTransform = _selectorRotation,
        };
        _selector = new Canvas { Opacity = 0 };
        _selector.Children.Add(_selectorGlow);
        _selector.Children.Add(_selectorLine);
        Children.Add(_selector);

        var innerRing = new Ellipse
        {
            Width = (InnerRadius - 6) * 2,
            Height = (InnerRadius - 6) * 2,
            Stroke = Frozen(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 1,
        };
        Place(innerRing, InnerRadius - 6);
        Children.Add(innerRing);

        var needleFigure = new PathFigure { StartPoint = new Point(Center, Center - (InnerRadius - 2)), IsClosed = true };
        needleFigure.Segments.Add(new LineSegment(new Point(Center + 8, Center - (InnerRadius - 14)), true));
        needleFigure.Segments.Add(new LineSegment(new Point(Center - 8, Center - (InnerRadius - 14)), true));
        _needle = new Path
        {
            Data = new PathGeometry([needleFigure]),
            Fill = Frozen(AccentColor),
            RenderTransform = _needleRotation,
            Opacity = 0,
        };
        Children.Add(_needle);

        // Lingkaran tengah: kategori, format terpilih, dan badge "HEIC → PNG".
        double discDiameter = CenterRadius * 2;
        var disc = new Ellipse
        {
            Fill = FrozenRadial(
                (Color.FromArgb(0xFA, 0x1C, 0x1C, 0x21), 0),
                (Color.FromArgb(0xFA, 0x0A, 0x0A, 0x0C), 1)),
            Stroke = Frozen(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
            StrokeThickness = 1.5,
        };
        var discRing = new Ellipse
        {
            Margin = new Thickness(8),
            Stroke = Frozen(Color.FromArgb(0x50, AccentColor.R, AccentColor.G, AccentColor.B)),
            StrokeThickness = 1,
            StrokeDashArray = [2, 3],
        };

        _centerGlyph = new TextBlock { FontFamily = IconFont, FontSize = 12, Foreground = Frozen(AccentColor), VerticalAlignment = VerticalAlignment.Center };
        _centerCategory = new TextBlock
        {
            FontFamily = LabelFont,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = Frozen(AccentColor),
            Margin = new Thickness(6, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        var header = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        header.Children.Add(_centerGlyph);
        header.Children.Add(_centerCategory);

        _centerTitle = CenterText(46, FontWeights.Bold, Colors.White);
        _centerTitle.FontStretch = FontStretches.Condensed;
        _centerTitle.Margin = new Thickness(0, 0, 0, -2);
        _centerDescription = CenterText(14, FontWeights.Normal, Color.FromArgb(0xC8, 0xFF, 0xFF, 0xFF));

        var badges = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
        badges.Children.Add(Badge(out _sourceBadge, Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF), Colors.White));
        badges.Children.Add(new TextBlock
        {
            Text = "",
            FontFamily = IconFont,
            FontSize = 11,
            Foreground = Frozen(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(7, 0, 7, 0),
        });
        badges.Children.Add(Badge(out _targetBadge, AccentColor, Color.FromRgb(0x11, 0x11, 0x11)));

        _centerContent = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16, 0, 16, 0),
            RenderTransform = _centerShift,
        };
        _centerContent.Children.Add(header);
        _centerContent.Children.Add(_centerTitle);
        _centerContent.Children.Add(_centerDescription);
        _centerContent.Children.Add(badges);

        // Konfirmasi saat file dilepas: centang muncul di tengah, cincin oranye memancar keluar.
        _confirmRing = new Ellipse
        {
            Stroke = Frozen(AccentColor),
            StrokeThickness = 4,
            Opacity = 0,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = _confirmRingScale,
        };
        _checkMark = new TextBlock
        {
            Text = "",
            FontFamily = IconFont,
            FontSize = 64,
            Foreground = Frozen(AccentColor),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = _checkScale,
        };

        var center = new Grid { Width = discDiameter, Height = discDiameter };
        center.Children.Add(disc);
        center.Children.Add(discRing);
        center.Children.Add(_centerContent);
        center.Children.Add(_checkMark);
        center.Children.Add(_confirmRing);
        Place(center, CenterRadius);
        Children.Add(center);
    }

    public event Action? SelectionChanged;

    public static double Size => (OuterRadius + EdgeMargin) * 2;

    static double Center => OuterRadius + EdgeMargin;

    /// <summary>Drag mode (default): pilihan dibuat dengan melepas file; false = dengan klik.</summary>
    public bool ChooseOnRelease { get; set; } = true;

    public ConvertTarget? SelectedTarget =>
        _category is null || _selectedIndex < 0 ? null : _category.Targets[_selectedIndex];

    public void Load(FileCategory category)
    {
        _segmentLayer.Children.Clear();
        _segments.Clear();
        _selectedIndex = -1;
        // Roda selalu dibuka dengan titik tengah di kursor.
        _inCenter = true;
        _category = category;

        // Pilihan sedikit (misal folder: hanya ZIP): sisanya slot kosong seperti di GTA V,
        // supaya roda tidak jadi satu cincin besar.
        int count = category.Targets.Count;
        int slots = Math.Max(count, MinSlots);
        double span = 360.0 / slots;
        for (int i = 0; i < slots; i++)
        {
            var segment = new Segment(i < count ? category.Targets[i] : null, i * span - span / 2, i * span + span / 2);
            _segments.Add(segment);
            _segmentLayer.Children.Add(segment.Root);
        }

        _selectorLine.Data = _selectorGlow.Data = CreateArc(-span / 2, span / 2, SelectorRadius, 2);
        _selector.BeginAnimation(OpacityProperty, null);
        _selector.Opacity = 0;
        _selectorShown = false;
        _needle.BeginAnimation(OpacityProperty, null);
        _needle.Opacity = 0;
        _needleShown = false;
        _centerContent.BeginAnimation(OpacityProperty, null);
        _centerContent.Opacity = 1;
        _checkMark.BeginAnimation(OpacityProperty, null);
        _checkMark.Opacity = 0;
        _confirmRing.BeginAnimation(OpacityProperty, null);
        _confirmRing.Opacity = 0;
        UpdateCenter(animate: false);
    }

    /// <summary>Segmen muncul satu per satu dari arah tengah.</summary>
    public void PlayEntrance()
    {
        for (int i = 0; i < _segments.Count; i++)
            _segments[i].PlayEntrance(TimeSpan.FromMilliseconds(i * 28));
    }

    /// <summary>Pilih segmen berdasarkan arah kursor relatif terhadap titik tengah roda.</summary>
    public void UpdatePointer(Vector fromCenter)
    {
        if (_segments.Count == 0)
            return;

        // Kursor di lingkaran tengah = batal: pilihan dilepas, drop di sini tidak mengonversi.
        bool active = fromCenter.Length >= CenterRadius;
        if (active != _needleShown)
        {
            _needleShown = active;
            _needle.BeginAnimation(OpacityProperty, new DoubleAnimation(active ? 1 : 0, HoverDuration));
        }
        if (!active)
        {
            bool changed = _selectedIndex >= 0 || !_inCenter;
            _inCenter = true;
            if (_selectedIndex >= 0)
                Select(-1);
            else if (changed)
                UpdateCenter(animate: true);
            return;
        }
        if (_inCenter)
        {
            _inCenter = false;
            if (_selectedIndex < 0)
                UpdateCenter(animate: true);
        }

        double angle = Math.Atan2(fromCenter.X, -fromCenter.Y) * 180 / Math.PI;
        if (angle < 0)
            angle += 360;
        _needleRotation.Angle = angle;

        double span = 360.0 / _segments.Count;
        Select((int)Math.Floor((angle + span / 2) / span) % _segments.Count);
    }

    public void FlashSelected()
    {
        if (_selectedIndex < 0)
            return;
        _segments[_selectedIndex].Flash();
        PlayConfirm();
    }

    void PlayConfirm()
    {
        var fast = TimeSpan.FromMilliseconds(120);
        _centerContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0, fast));
        _checkMark.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, fast));
        var pop = new DoubleAnimation(0.4, 1, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new BackEase { Amplitude = 0.6, EasingMode = EasingMode.EaseOut },
        };
        _checkScale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
        _checkScale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);

        var burst = TimeSpan.FromMilliseconds(340);
        var grow = new DoubleAnimation(1, 1.5, burst) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        _confirmRingScale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
        _confirmRingScale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
        _confirmRing.BeginAnimation(OpacityProperty, new DoubleAnimation(0.9, 0, burst));
    }

    void Select(int index)
    {
        if (index >= _category!.Targets.Count)
            index = -1; // slot kosong: tidak ada pilihan, drop dibatalkan
        if (index == _selectedIndex)
            return;
        if (_selectedIndex >= 0)
            _segments[_selectedIndex].SetHighlighted(false);
        _selectedIndex = index;
        if (index < 0)
        {
            _selectorShown = false;
            _selector.BeginAnimation(OpacityProperty, new DoubleAnimation(0, HoverDuration));
            UpdateCenter(animate: true);
            SelectionChanged?.Invoke();
            return;
        }
        _segments[index].SetHighlighted(true);
        MoveSelector(index * 360.0 / _segments.Count);
        UpdateCenter(animate: true);
        SelectionChanged?.Invoke();
    }

    // Indikator oranye meluncur lewat jalur terpendek ke segmen baru.
    void MoveSelector(double target)
    {
        if (!_selectorShown)
        {
            _selectorShown = true;
            _selectorAngle = target;
            _selectorRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            _selectorRotation.Angle = target;
            _selector.BeginAnimation(OpacityProperty, new DoubleAnimation(1, HoverDuration));
            return;
        }

        double delta = ((target - _selectorAngle) % 360 + 540) % 360 - 180;
        _selectorAngle += delta;
        _selectorRotation.BeginAnimation(RotateTransform.AngleProperty,
            new DoubleAnimation(_selectorAngle, TimeSpan.FromMilliseconds(150))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            });
    }

    void UpdateCenter(bool animate)
    {
        if (_category is null)
            return;

        _centerGlyph.Text = _category.Glyph;
        _centerCategory.Text = L.T(_category.Name).ToUpperInvariant();
        // Folder dan file tanpa ekstensi: tampilkan "FOLDER"/"FILE".
        string extension = System.IO.Path.GetExtension(_category.FileName).TrimStart('.');
        _sourceBadge.Text = (extension.Length > 0 ? extension : L.T(_category.Name)).ToUpperInvariant();

        if (SelectedTarget is { } target)
        {
            _centerTitle.Text = L.T(target.Label);
            _centerDescription.Text = L.T(target.Description);
            _targetBadge.Text = L.T(target.Label);
        }
        else if (_inCenter)
        {
            _centerTitle.Text = L.T("CANCEL");
            _centerDescription.Text = L.T(ChooseOnRelease ? "Release here to cancel" : "Click here to cancel");
            _targetBadge.Text = "✕";
        }
        else
        {
            _centerTitle.Text = L.T("PICK");
            _centerDescription.Text = L.T("Point at a format");
            _targetBadge.Text = "?";
        }

        if (!animate)
            return;
        var duration = TimeSpan.FromMilliseconds(150);
        _centerShift.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(8, 0, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        _centerContent.BeginAnimation(OpacityProperty, new DoubleAnimation(0.2, 1, duration));
    }

    static TextBlock CenterText(double size, FontWeight weight, Color color) => new()
    {
        FontFamily = LabelFont,
        FontSize = size,
        FontWeight = weight,
        Foreground = Frozen(color),
        HorizontalAlignment = HorizontalAlignment.Center,
        TextAlignment = TextAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis,
    };

    static Border Badge(out TextBlock text, Color background, Color foreground)
    {
        text = new TextBlock
        {
            FontFamily = LabelFont,
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = Frozen(foreground),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        return new Border
        {
            Background = Frozen(background),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(7, 2, 7, 2),
            MinWidth = 30,
            Child = text,
        };
    }

    static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    static RadialGradientBrush FrozenRadial(params (Color Color, double Offset)[] stops)
    {
        var brush = new RadialGradientBrush();
        foreach (var (color, offset) in stops)
            brush.GradientStops.Add(new GradientStop(color, offset));
        brush.Freeze();
        return brush;
    }

    static void Place(FrameworkElement element, double radius)
    {
        SetLeft(element, Center - radius);
        SetTop(element, Center - radius);
    }

    // 0° = atas, bertambah searah jarum jam.
    static Point Polar(double angleDeg, double radius)
    {
        double a = angleDeg * Math.PI / 180;
        return new Point(Center + radius * Math.Sin(a), Center - radius * Math.Cos(a));
    }

    // Sudut yang menghasilkan celah selebar GapPx (konstan dalam piksel) pada radius tertentu.
    static double GapAngle(double radius) => Math.Asin(GapPx / 2 / radius) * 180 / Math.PI;

    static Geometry CreateSector(double start, double end)
    {
        double outerGap = GapAngle(OuterRadius);
        double innerGap = GapAngle(InnerRadius);
        bool isLargeArc = end - start > 180;

        var figure = new PathFigure { StartPoint = Polar(start + outerGap, OuterRadius), IsClosed = true };
        figure.Segments.Add(new ArcSegment(Polar(end - outerGap, OuterRadius), new Size(OuterRadius, OuterRadius),
            0, isLargeArc, SweepDirection.Clockwise, true));
        figure.Segments.Add(new LineSegment(Polar(end - innerGap, InnerRadius), true));
        figure.Segments.Add(new ArcSegment(Polar(start + innerGap, InnerRadius), new Size(InnerRadius, InnerRadius),
            0, isLargeArc, SweepDirection.Counterclockwise, true));

        var geometry = new PathGeometry([figure]);
        geometry.Freeze();
        return geometry;
    }

    static Geometry CreateArc(double start, double end, double radius, double extraInset)
    {
        double inset = GapAngle(radius) + extraInset;
        var figure = new PathFigure { StartPoint = Polar(start + inset, radius), IsFilled = false };
        figure.Segments.Add(new ArcSegment(Polar(end - inset, radius), new Size(radius, radius),
            0, end - start > 180, SweepDirection.Clockwise, true));

        var geometry = new PathGeometry([figure]);
        geometry.Freeze();
        return geometry;
    }

    sealed class Segment
    {
        readonly TranslateTransform _entrance = new();
        readonly TranslateTransform _hover = new();
        readonly ScaleTransform _contentScale = new(1, 1);
        readonly GradientStop _innerStop = new(SegmentInner, InnerRadius / OuterRadius);
        readonly GradientStop _outerStop = new(SegmentOuter, 1);
        readonly SolidColorBrush _label = new(LabelColor);
        readonly Vector _direction;

        /// <param name="target">Null = slot kosong (redup, tanpa label, tidak bisa dipilih).</param>
        public Segment(ConvertTarget? target, double start, double end)
        {
            if (target is null)
            {
                _innerStop.Color = EmptyInner;
                _outerStop.Color = EmptyOuter;
            }
            double mid = (start + end) / 2;
            _direction = Polar(mid, 1) - Polar(mid, 0);
            Root.RenderTransform = new TransformGroup { Children = { _entrance, _hover } };
            Root.Opacity = 0;

            // Gradasi radial dari pusat roda: gelap di dalam, lebih terang di luar.
            var fill = new RadialGradientBrush
            {
                MappingMode = BrushMappingMode.Absolute,
                Center = new Point(Center, Center),
                GradientOrigin = new Point(Center, Center),
                RadiusX = OuterRadius,
                RadiusY = OuterRadius,
            };
            fill.GradientStops.Add(_innerStop);
            fill.GradientStops.Add(_outerStop);

            var shape = new Path
            {
                Data = CreateSector(start, end),
                Fill = fill,
                Stroke = Frozen(Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF)),
                StrokeThickness = 1,
            };

            if (target is null)
            {
                shape.Stroke = Frozen(Color.FromArgb(0x1C, 0xFF, 0xFF, 0xFF));
                Root.Children.Add(shape);
                return;
            }

            // Garis terang tipis di tepi luar segmen (rim light).
            var rim = new Path
            {
                Data = CreateArc(start, end, OuterRadius - 2.5, 0.6),
                Stroke = Frozen(Color.FromArgb(0x50, 0xFF, 0xFF, 0xFF)),
                StrokeThickness = 2,
            };

            var content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _contentScale,
            };
            content.Children.Add(new TextBlock
            {
                Text = target.Glyph,
                FontFamily = IconFont,
                FontSize = 30,
                Foreground = _label,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6),
            });
            content.Children.Add(new TextBlock
            {
                Text = L.T(target.Label),
                FontFamily = LabelFont,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                FontStretch = FontStretches.Condensed,
                Foreground = _label,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            const double boxWidth = 120, boxHeight = 84;
            var box = new Grid { Width = boxWidth, Height = boxHeight };
            box.Children.Add(content);
            var labelPoint = Polar(mid, (InnerRadius + OuterRadius) / 2);
            SetLeft(box, labelPoint.X - boxWidth / 2);
            SetTop(box, labelPoint.Y - boxHeight / 2);

            Root.Children.Add(shape);
            Root.Children.Add(rim);
            Root.Children.Add(box);
        }

        public Canvas Root { get; } = new();

        public void PlayEntrance(TimeSpan delay)
        {
            var duration = TimeSpan.FromMilliseconds(280);
            var ease = new BackEase { Amplitude = 0.3, EasingMode = EasingMode.EaseOut };
            Root.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)) { BeginTime = delay });
            _entrance.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(-_direction.X * 40, 0, duration) { BeginTime = delay, EasingFunction = ease });
            _entrance.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(-_direction.Y * 40, 0, duration) { BeginTime = delay, EasingFunction = ease });
        }

        public void SetHighlighted(bool highlighted)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            _innerStop.BeginAnimation(GradientStop.ColorProperty,
                new ColorAnimation(highlighted ? HoverInner : SegmentInner, HoverDuration) { EasingFunction = ease });
            _outerStop.BeginAnimation(GradientStop.ColorProperty,
                new ColorAnimation(highlighted ? HoverOuter : SegmentOuter, HoverDuration) { EasingFunction = ease });
            _label.BeginAnimation(SolidColorBrush.ColorProperty,
                new ColorAnimation(highlighted ? LabelHoverColor : LabelColor, HoverDuration) { EasingFunction = ease });
            _hover.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(highlighted ? _direction.X * PopDistance : 0, HoverDuration) { EasingFunction = ease });
            _hover.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(highlighted ? _direction.Y * PopDistance : 0, HoverDuration) { EasingFunction = ease });
            var scale = new DoubleAnimation(highlighted ? 1.12 : 1, HoverDuration) { EasingFunction = ease };
            _contentScale.BeginAnimation(ScaleTransform.ScaleXProperty, scale);
            _contentScale.BeginAnimation(ScaleTransform.ScaleYProperty, scale);
        }

        public void Flash()
        {
            var duration = TimeSpan.FromMilliseconds(350);
            _innerStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation(AccentColor, HoverInner, duration));
            _outerStop.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation(AccentColor, HoverOuter, duration));
        }
    }
}
