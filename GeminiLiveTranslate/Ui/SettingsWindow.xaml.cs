using System.Windows;
using System.Windows.Controls;
using GeminiLiveTranslate.Settings;

namespace GeminiLiveTranslate.Ui;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    public SettingsWindow(AppSettings settings)
    {
        _settings = settings;
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        ApiKeyBox.Password = _settings.ApiKey;
        ApiBaseBox.Text = _settings.ApiBase;
        ProxyBox.Text = _settings.ProxyUrl;
        ModelBox.Text = _settings.GeminiModel;
        SelectCombo(LanguageBox, _settings.TargetLanguage);
        SelectCombo(AudioSourceBox, _settings.AudioSource);
        DeviceBox.Text = _settings.AudioDeviceNumber.ToString();
        EchoBox.IsChecked = _settings.EchoTargetLanguage;
        ShowOriginalBox.IsChecked = _settings.ShowOriginal;
        VolumeBox.Text = _settings.PlaybackVolume.ToString("0.##");
        FontSizeBox.Text = _settings.FontSize.ToString();
        OpacityBox.Text = _settings.BackgroundOpacity.ToString("0.##");
        PromptBox.Text = _settings.SystemPrompt;
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        _settings.ApiKey = ApiKeyBox.Password;
        _settings.ApiBase = ApiBaseBox.Text;
        _settings.ProxyUrl = ProxyBox.Text;
        _settings.GeminiModel = ModelBox.Text;
        _settings.TargetLanguage = ((ComboBoxItem?)LanguageBox.SelectedItem)?.Content?.ToString() ?? "zh-CN";
        _settings.AudioSource = ((ComboBoxItem?)AudioSourceBox.SelectedItem)?.Content?.ToString() ?? "system";
        _settings.AudioDeviceNumber = int.TryParse(DeviceBox.Text, out var device) ? device : -1;
        _settings.EchoTargetLanguage = EchoBox.IsChecked == true;
        _settings.ShowOriginal = ShowOriginalBox.IsChecked == true;
        _settings.PlaybackVolume = double.TryParse(VolumeBox.Text, out var volume) ? volume : 0.8;
        _settings.FontSize = int.TryParse(FontSizeBox.Text, out var size) ? size : 15;
        _settings.BackgroundOpacity = double.TryParse(OpacityBox.Text, out var opacity) ? opacity : 0.72;
        _settings.SystemPrompt = PromptBox.Text;
        _settings.Normalize();
        DialogResult = true;
    }

    private static void SelectCombo(System.Windows.Controls.ComboBox combo, string value)
    {
        foreach (var item in combo.Items.OfType<ComboBoxItem>())
        {
            if (string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedItem = item;
                return;
            }
        }
        combo.SelectedIndex = 0;
    }
}
