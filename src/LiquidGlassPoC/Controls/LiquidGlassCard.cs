using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace LiquidGlassPoC.Controls;

public sealed class LiquidGlassCard : ContentControl
{
    private FrameworkElement? _glowSurface;
    private RadialGradientBrush? _glowBrush;
    private Visual? _cardVisual;
    private Visual? _glowVisual;

    public static readonly DependencyProperty MotionEnabledProperty =
        DependencyProperty.Register(
            nameof(MotionEnabled),
            typeof(bool),
            typeof(LiquidGlassCard),
            new PropertyMetadata(true));

    public bool MotionEnabled
    {
        get => (bool)GetValue(MotionEnabledProperty);
        set => SetValue(MotionEnabledProperty, value);
    }

    public LiquidGlassCard()
    {
        DefaultStyleKey = typeof(LiquidGlassCard);

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        PointerEntered += OnPointerEntered;
        PointerMoved += OnPointerMoved;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
        PointerCanceled += OnPointerCanceled;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _glowSurface = GetTemplateChild("PART_GlowSurface") as FrameworkElement;
        _glowBrush = (_glowSurface as Border)?.Background as RadialGradientBrush;

        if (_glowSurface is not null)
        {
            _glowVisual = ElementCompositionPreview.GetElementVisual(_glowSurface);
            _glowVisual.Opacity = 0f;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _cardVisual = ElementCompositionPreview.GetElementVisual(this);
        UpdateCenterPoint();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateCenterPoint();

    private void UpdateCenterPoint()
    {
        if (_cardVisual is null)
        {
            return;
        }

        _cardVisual.CenterPoint = new Vector3(
            (float)(ActualWidth / 2.0),
            (float)(ActualHeight / 2.0),
            0f);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        UpdateGlow(e);
        AnimateGlow(1f);
        AnimateScale(MotionEnabled ? 1.012f : 1f, 150);
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e) => UpdateGlow(e);

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateGlow(0f);
        AnimateScale(1f, 180);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        AnimateScale(MotionEnabled ? 0.985f : 1f, 90);
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        AnimateScale(MotionEnabled ? 1.012f : 1f, 170);
    }

    private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        AnimateScale(1f, 140);
    }

    private void UpdateGlow(PointerRoutedEventArgs e)
    {
        if (_glowBrush is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var point = e.GetCurrentPoint(this).Position;
        var center = new Windows.Foundation.Point(
            Math.Clamp(point.X / ActualWidth, 0, 1),
            Math.Clamp(point.Y / ActualHeight, 0, 1));

        _glowBrush.Center = center;
        _glowBrush.GradientOrigin = center;
    }

    private void AnimateScale(float target, int durationMs)
    {
        if (_cardVisual is null)
        {
            return;
        }

        if (!MotionEnabled)
        {
            _cardVisual.Scale = Vector3.One;
            return;
        }

        var compositor = _cardVisual.Compositor;
        var animation = compositor.CreateVector3KeyFrameAnimation();
        var easing = compositor.CreateCubicBezierEasingFunction(
            new Vector2(0.2f, 0.8f),
            new Vector2(0.2f, 1.0f));

        animation.InsertKeyFrame(1f, new Vector3(target, target, 1f), easing);
        animation.Duration = TimeSpan.FromMilliseconds(durationMs);
        _cardVisual.StartAnimation(nameof(Visual.Scale), animation);
    }

    private void AnimateGlow(float target)
    {
        if (_glowVisual is null)
        {
            return;
        }

        if (!MotionEnabled)
        {
            _glowVisual.Opacity = target;
            return;
        }

        var animation = _glowVisual.Compositor.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(1f, target);
        animation.Duration = TimeSpan.FromMilliseconds(target > 0 ? 120 : 220);
        _glowVisual.StartAnimation(nameof(Visual.Opacity), animation);
    }
}
