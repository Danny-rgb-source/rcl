using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace RCL.Win
{
    public partial class SettingsWindow : Window
    {
        private readonly string _settingsPath;

        public SettingsWindow(string settingsPath)
        {
            InitializeComponent();
            // __IDEMPOTENT_SETTINGS_ATTACH__
            try { if (BtnSave != null) { try { BtnSave.Click -= BtnSave_Click; } catch {} BtnSave.Click += BtnSave_Click; } } catch {}
            _settingsPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));

            // Wire handlers defensively (controls may or may not exist depending on XAML)
            TryWire("BtnSave", (s, e) => BtnSave_Click(s, e));
            TryWire("BtnCancel", (s, e) => BtnCancel_Click(s, e));
            TryWire("BtnLight", (s, e) => BtnLight_Click(s, e));
            TryWire("BtnDark", (s, e) => BtnDark_Click(s, e));

            TryWireSlider("SliderFontSize", (s, ev) => SliderFontSize_ValueChanged(s, ev));

            // Load current persistent settings into the UI (if any)
            LoadSettingsIntoUi();
        }

        private void TryWire(string name, RoutedEventHandler handler)
        {
            try
            {
                var ctrl = this.FindName(name) as Button;
                if (ctrl != null)
                {
                    try { ctrl.Click -= handler; } catch { }
                    ctrl.Click += handler;
                }
            }
            catch { }
        }

        private void TryWireSlider(string name, RoutedPropertyChangedEventHandler<double> handler)
        {
            try
            {
                var s = this.FindName(name) as System.Windows.Controls.Slider;
                if (s != null)
                {
                    try { s.ValueChanged -= handler; } catch { }
                    s.ValueChanged += handler;
                }
            }
            catch { }
        }

        private void LoadSettingsIntoUi()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return;
                var txt = File.ReadAllText(_settingsPath);
                if (string.IsNullOrWhiteSpace(txt)) return;

                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var s = JsonSerializer.Deserialize<UserUiSettings>(txt, opts);
                if (s == null) return;

                // Theme (ComboBox or other)
                if (this.FindName("ThemeCombo") is ComboBox cb)
                {
                    foreach (var it in cb.Items)
                    {
                        if (it is ComboBoxItem cbi && string.Equals(cbi.Content?.ToString(), s.Theme.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            cb.SelectedItem = cbi;
                            break;
                        }
                    }
                }

                if (this.FindName("SliderFontSize") is System.Windows.Controls.Slider slider)
                {
                    slider.Value = s.FontSize;
                }

                if (this.FindName("TxtFirebaseProjectId") is TextBox tf && !string.IsNullOrEmpty(s is not null ? s.Theme.ToString() : null))
                {
                    // nothing to map here by default; keep for expansion
                }
            }
            catch
            {
                // ignore malformed settings
            }
        }

        private void BtnLight_Click(object sender, RoutedEventArgs e)
        {
            if (this.FindName("ThemeCombo") is ComboBox cb)
            {
                foreach (var it in cb.Items)
                {
                    if (it is ComboBoxItem cbi && string.Equals(cbi.Content?.ToString(), "Light", StringComparison.OrdinalIgnoreCase))
                    {
                        cb.SelectedItem = cbi;
                        break;
                    }
                }
            }
        }

        private void BtnDark_Click(object sender, RoutedEventArgs e)
        {
            if (this.FindName("ThemeCombo") is ComboBox cb)
            {
                foreach (var it in cb.Items)
                {
                    if (it is ComboBoxItem cbi && string.Equals(cbi.Content?.ToString(), "Dark", StringComparison.OrdinalIgnoreCase))
                    {
                        cb.SelectedItem = cbi;
                        break;
                    }
                }
            }
        }

        private void SliderFontSize_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            // Optional preview update if SampleFontPreview exists
            if (this.FindName("SampleFontPreview") is TextBlock tb)
                tb.FontSize = e.NewValue;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = new UserUiSettings();

                // Theme
                if (this.FindName("ThemeCombo") is ComboBox cb && cb.SelectedItem is ComboBoxItem cbi)
                {
                    var txt = (cbi.Content ?? string.Empty).ToString();
                    if (Enum.TryParse<UiTheme>(txt, true, out var t)) settings.Theme = t;
                }

                // Font size
                if (this.FindName("SliderFontSize") is System.Windows.Controls.Slider slider)
                    settings.FontSize = slider.Value;

                // Font family (optional text field)
                if (this.FindName("TxtFontFamily") is TextBox tbf && !string.IsNullOrWhiteSpace(tbf.Text))
                    settings.FontFamily = tbf.Text.Trim();
                else
                    settings.FontFamily = settings.FontFamily ?? "Segoe UI";

                // Save & apply via App helper (persist then apply)
                if (Application.Current is App app)
                {
                    app.UpdateAndApplyUiSettings(settings);
                }
                else
                {
                    // Fallback: write directly
                    try
                    {
                        var dir = Path.GetDirectoryName(_settingsPath);
                        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                        var opts = new JsonSerializerOptions { WriteIndented = true };
                        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, opts));
                    }
                    catch { }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save settings: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


