using LiquidGlassPoC.Controls;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Windows.Graphics;

namespace LiquidGlassPoC;

public sealed partial class MainWindow : Window
{
    private readonly LiquidGlassCard[] _animatedCards;
    private readonly Button[] _navigationButtons;
    private readonly AppWindow _appWindow;
    private bool _isReady;
    private bool _darkMode;
    private string _currentSection = "Overview";

    public MainWindow()
    {
        InitializeComponent();

        _animatedCards =
        [
            SidebarStatusCard,
            ToolbarCard,
            HeroCard,
            MetricsCard,
            SettingsCard
        ];

        _navigationButtons =
        [
            OverviewNavigationButton,
            MaterialsNavigationButton,
            MotionNavigationButton,
            AccessibilityNavigationButton
        ];

        var windowHandle = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.Resize(new SizeInt32(1240, 820));
        _appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        RootContent.ActualThemeChanged += (_, _) =>
        {
            UpdateCaptionButtonColors();
            ApplyMaterialThemeColors();
        };

        ApplyBackdrop("Mica");
        UpdateCaptionButtonColors();

        _isReady = true;
        ApplyMaterialThemeColors();
        ApplyMaterialSettings();
        SelectSection("Overview");
    }

    private AcrylicBrush? SurfaceBrush =>
        Application.Current.Resources["GlassSurfaceBrush"] as AcrylicBrush;

    private AcrylicBrush? DenseBrush =>
        Application.Current.Resources["GlassDenseBrush"] as AcrylicBrush;

    private void ApplyBackdrop(string kind)
    {
        SystemBackdrop = kind switch
        {
            "MicaAlt" => new MicaBackdrop { Kind = MicaKind.BaseAlt },
            "Acrylic" => new DesktopAcrylicBackdrop(),
            _ => new MicaBackdrop { Kind = MicaKind.Base }
        };

        StatusText.Text = kind switch
        {
            "MicaAlt" => "Mica Alt + Acrylic interne",
            "Acrylic" => "Desktop Acrylic + Acrylic interne",
            _ => "Mica + Acrylic interne"
        };
    }

    private void BackdropSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isReady || BackdropSelector.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        ApplyBackdrop(item.Tag?.ToString() ?? "Mica");
    }

    private void MaterialSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_isReady)
        {
            ApplyMaterialSettings();
        }
    }

    private void ApplyMaterialSettings()
    {
        if (SurfaceBrush is { } surface)
        {
            surface.TintOpacity = TintSlider.Value;
            surface.TintLuminosityOpacity = LuminositySlider.Value;
            surface.AlwaysUseFallback = OpaqueToggle.IsOn;
        }

        if (DenseBrush is { } dense)
        {
            dense.TintOpacity = Math.Min(0.94, TintSlider.Value + 0.20);
            dense.TintLuminosityOpacity = Math.Min(1.0, LuminositySlider.Value + 0.10);
            dense.AlwaysUseFallback = OpaqueToggle.IsOn;
        }

        if (_currentSection is "Overview" or "Materials")
        {
            MetricOneValueText.Text = $"{TintSlider.Value:P0}";
            MetricTwoValueText.Text = $"{LuminositySlider.Value:P0}";
        }
    }

    private void ApplyMaterialThemeColors()
    {
        var isDark = RootContent.ActualTheme == ElementTheme.Dark;

        if (SurfaceBrush is { } surface)
        {
            surface.TintColor = isDark
                ? ColorHelper.FromArgb(0xFF, 0x15, 0x22, 0x3A)
                : ColorHelper.FromArgb(0xFF, 0xF5, 0xF8, 0xFF);
            surface.FallbackColor = isDark
                ? ColorHelper.FromArgb(0xFF, 0x17, 0x1C, 0x27)
                : ColorHelper.FromArgb(0xFF, 0xF1, 0xF3, 0xF8);
        }

        if (DenseBrush is { } dense)
        {
            dense.TintColor = isDark
                ? ColorHelper.FromArgb(0xFF, 0x10, 0x1B, 0x30)
                : ColorHelper.FromArgb(0xFF, 0xEE, 0xF3, 0xFF);
            dense.FallbackColor = isDark
                ? ColorHelper.FromArgb(0xFF, 0x12, 0x17, 0x22)
                : ColorHelper.FromArgb(0xFF, 0xE9, 0xED, 0xF5);
        }
    }

    private void MotionToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (!_isReady)
        {
            return;
        }

        foreach (var card in _animatedCards)
        {
            card.MotionEnabled = MotionToggle.IsOn;
        }
    }

    private void OpaqueToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isReady)
        {
            ApplyMaterialSettings();
            StatusText.Text = OpaqueToggle.IsOn
                ? "Repli opaque forcé"
                : "Transparence système active";
        }
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string section })
        {
            SelectSection(section);
        }
    }

    private void SelectSection(string section)
    {
        _currentSection = section;

        var content = section switch
        {
            "Materials" => new NavigationContent(
                "Matériaux",
                "Compose Mica, Acrylic, teinte et luminosité et observe immédiatement le résultat.",
                "COMPOSITION",
                "Construis le matériau couche par couche",
                "Choisis le backdrop, puis règle l’opacité et la luminosité de l’Acrylic interne.",
                "Changer de backdrop",
                "Recette du matériau",
                "Valeurs appliquées en direct",
                $"{TintSlider.Value:P0}", "Teinte",
                $"{LuminositySlider.Value:P0}", "Luminosité",
                "1 px", "Contour",
                "Réglages du matériau"),
            "Motion" => new NavigationContent(
                "Mouvement",
                "Teste les animations Composition au survol, à la pression et lors des changements d’état.",
                "COMPOSITION",
                "Le reflet et l’échelle suivent l’interaction",
                "Le halo radial suit le pointeur tandis que l’échelle utilise une courbe amortie courte.",
                "Activer/désactiver",
                "Profil d’animation",
                "Paramètres du comportement interactif",
                "150 ms", "Survol",
                "1,012×", "Échelle",
                "220 ms", "Sortie",
                "Contrôles du mouvement"),
            "Accessibility" => new NavigationContent(
                "Accessibilité",
                "Vérifie la lisibilité lorsque les transparences ou les animations sont désactivées.",
                "REPLI ADAPTATIF",
                "Le contenu reste lisible sans transparence",
                "Le fallback opaque, le contraste élevé et la désactivation du mouvement conservent la hiérarchie visuelle.",
                "Basculer le repli opaque",
                "Garanties d’accessibilité",
                "Comportements prévus par le PoC",
                "100 %", "Fallback",
                "HC", "Contraste",
                "0 ms", "Sans mouvement",
                "Options d’accessibilité"),
            _ => new NavigationContent(
                "Liquid Glass, version Windows",
                "Un matériau composé de Mica, d’Acrylic, de contours spéculaires et d’interactions pilotées par le pointeur.",
                "SURFACE INTERACTIVE",
                "Le reflet suit le pointeur",
                "Survole cette carte : son éclairage se déplace et sa surface réagit avec une animation amortie.",
                "Tester l’action",
                "Recette du matériau",
                "Composition actuelle",
                $"{TintSlider.Value:P0}", "Teinte",
                $"{LuminositySlider.Value:P0}", "Luminosité",
                "1 px", "Contour",
                "Laboratoire")
        };

        PageTitleText.Text = content.PageTitle;
        PageSubtitleText.Text = content.PageSubtitle;
        HeroBadgeText.Text = content.HeroBadge;
        HeroTitleText.Text = content.HeroTitle;
        HeroBodyText.Text = content.HeroBody;
        HeroActionButton.Content = content.ActionLabel;
        MetricsTitleText.Text = content.MetricsTitle;
        MetricsSubtitleText.Text = content.MetricsSubtitle;
        MetricOneValueText.Text = content.MetricOneValue;
        MetricOneLabelText.Text = content.MetricOneLabel;
        MetricTwoValueText.Text = content.MetricTwoValue;
        MetricTwoLabelText.Text = content.MetricTwoLabel;
        MetricThreeValueText.Text = content.MetricThreeValue;
        MetricThreeLabelText.Text = content.MetricThreeLabel;
        SettingsTitleText.Text = content.SettingsTitle;

        foreach (var button in _navigationButtons)
        {
            var isSelected = string.Equals(button.Tag?.ToString(), section, StringComparison.Ordinal);
            button.Background = new SolidColorBrush(isSelected
                ? ColorHelper.FromArgb(0x32, 0x4A, 0x8C, 0xFF)
                : Colors.Transparent);
            button.BorderBrush = isSelected
                ? Application.Current.Resources["AppAccentBrush"] as Brush
                : new SolidColorBrush(Colors.Transparent);
            button.BorderThickness = isSelected ? new Thickness(1) : new Thickness(0);
        }

        StatusText.Text = section switch
        {
            "Materials" => "Section Matériaux active",
            "Motion" => "Section Mouvement active",
            "Accessibility" => "Section Accessibilité active",
            _ => "Vue d’ensemble active"
        };
    }

    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        _darkMode = !_darkMode;
        RootContent.RequestedTheme = _darkMode ? ElementTheme.Dark : ElementTheme.Light;
        ApplyMaterialThemeColors();
        UpdateCaptionButtonColors();
    }

    private void PulseButton_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Interaction Composition déclenchée";
        TintSlider.Value = TintSlider.Value > 0.55 ? 0.38 : 0.64;
    }

    private void ActionButton_Click(object sender, RoutedEventArgs e)
    {
        switch (_currentSection)
        {
            case "Materials":
                BackdropSelector.SelectedIndex = (BackdropSelector.SelectedIndex + 1) % 3;
                break;
            case "Motion":
                MotionToggle.IsOn = !MotionToggle.IsOn;
                break;
            case "Accessibility":
                OpaqueToggle.IsOn = !OpaqueToggle.IsOn;
                break;
            default:
                StatusText.Text = "Action reçue — le verre reste interactif";
                break;
        }
    }

    private void UpdateCaptionButtonColors()
    {
        if (!AppWindowTitleBar.IsCustomizationSupported())
        {
            return;
        }

        var isDark = RootContent.ActualTheme == ElementTheme.Dark;
        var titleBar = _appWindow.TitleBar;

        titleBar.ButtonForegroundColor = isDark ? Colors.White : Colors.Black;
        titleBar.ButtonInactiveForegroundColor = isDark
            ? ColorHelper.FromArgb(0x88, 0xFF, 0xFF, 0xFF)
            : ColorHelper.FromArgb(0x88, 0x00, 0x00, 0x00);
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonHoverBackgroundColor = isDark
            ? ColorHelper.FromArgb(0x24, 0xFF, 0xFF, 0xFF)
            : ColorHelper.FromArgb(0x14, 0x00, 0x00, 0x00);
        titleBar.ButtonPressedBackgroundColor = isDark
            ? ColorHelper.FromArgb(0x36, 0xFF, 0xFF, 0xFF)
            : ColorHelper.FromArgb(0x24, 0x00, 0x00, 0x00);
    }

    private sealed record NavigationContent(
        string PageTitle,
        string PageSubtitle,
        string HeroBadge,
        string HeroTitle,
        string HeroBody,
        string ActionLabel,
        string MetricsTitle,
        string MetricsSubtitle,
        string MetricOneValue,
        string MetricOneLabel,
        string MetricTwoValue,
        string MetricTwoLabel,
        string MetricThreeValue,
        string MetricThreeLabel,
        string SettingsTitle);
}
