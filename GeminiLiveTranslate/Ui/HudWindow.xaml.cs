using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using GeminiLiveTranslate.Settings;
using MediaColor = System.Windows.Media.Color;

namespace GeminiLiveTranslate.Ui;

public partial class HudWindow : Window
{
    private readonly AppSettings _settings;
    private readonly RollingTextTrack _translationTrack = new();
    private readonly RollingTextTrack _sourceTrack = new();

    public event Action? ToggleRequested;
    public event Action? SettingsRequested;
    public event Action? ExitRequested;

    public HudWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        ApplySettings();
    }

    public void ApplySettings()
    {
        Width = Math.Max(420, _settings.Hud.Width);
        Height = Math.Max(150, _settings.Hud.Height);
        Left = _settings.Hud.Left;
        Top = _settings.Hud.Top;
        OutputText.FontSize = _settings.FontSize;
        InputText.Visibility = _settings.ShowOriginal ? Visibility.Visible : Visibility.Collapsed;
        InputScroll.Visibility = _settings.ShowOriginal ? Visibility.Visible : Visibility.Collapsed;
        LaneDivider.Visibility = _settings.ShowOriginal ? Visibility.Visible : Visibility.Collapsed;
        var opacity = (byte)(Math.Clamp(_settings.BackgroundOpacity, 0.2, 0.95) * 255);
        RootPanel.Background = new SolidColorBrush(MediaColor.FromArgb(opacity, 17, 24, 39));
    }

    public void SavePlacement()
    {
        _settings.Hud.Left = Left;
        _settings.Hud.Top = Top;
        _settings.Hud.Width = Width;
        _settings.Hud.Height = Height;
    }

    public void SetRunning(bool running) => ToggleButton.Content = running ? "Stop" : "Start";

    public void SetStatus(string message, string kind)
    {
        StatusText.Text = message;
        StatusDot.Fill = new SolidColorBrush(kind switch
        {
            "connected" => MediaColor.FromRgb(34, 197, 94),
            "connecting" => MediaColor.FromRgb(250, 204, 21),
            "error" => MediaColor.FromRgb(248, 113, 113),
            "warning" => MediaColor.FromRgb(251, 146, 60),
            _ => MediaColor.FromRgb(100, 116, 139)
        });
    }

    public void ClearTranscripts()
    {
        _translationTrack.Clear();
        _sourceTrack.Clear();
        OutputText.Text = "Ready";
        InputText.Text = "";
        OutputScroll.ScrollToEnd();
        InputScroll.ScrollToEnd();
    }

    public void SetInput(string text)
    {
        InputText.Text = _sourceTrack.Update(text);
        InputScroll.ScrollToEnd();
    }

    public void SetOutput(string text)
    {
        OutputText.Text = _translationTrack.Update(text);
        OutputScroll.ScrollToEnd();
    }
    public void SetStats(int pending, int dropped) => StatsText.Text = $"Pending: {pending} / Dropped: {dropped}";

    private void ToggleButton_OnClick(object sender, RoutedEventArgs e) => ToggleRequested?.Invoke();
    private void SettingsButton_OnClick(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke();
    private void ExitButton_OnClick(object sender, RoutedEventArgs e) => ExitRequested?.Invoke();

    private void DragBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void RootPanel_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed || IsInteractiveElement(e.OriginalSource as DependencyObject)) return;
        DragMove();
    }

    private static bool IsInteractiveElement(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is System.Windows.Controls.Primitives.ButtonBase
                or System.Windows.Controls.Primitives.TextBoxBase
                or System.Windows.Controls.Primitives.ScrollBar
                or System.Windows.Controls.ComboBox
                or System.Windows.Controls.PasswordBox)
            {
                return true;
            }
            source = VisualTreeHelper.GetParent(source);
        }
        return false;
    }
}
