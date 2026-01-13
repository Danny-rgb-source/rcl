using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using RCL.Win.Helpers.Services;

namespace RCL.Win.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly string _settingsPath;
        private UserSettings _settings = new UserSettings();

        public SettingsWindow() : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "rcl_user_settings.json")) { }

        public SettingsWindow(string settingsPath)
        {
            InitializeComponent();
            _settingsPath = settingsPath;
            LoadSettings();

            LightBtn.Click += (s, e) => ThemeManager.ApplyLightTheme();
            DarkBtn.Click += (s, e) => ThemeManager.ApplyDarkTheme();

            SmallTextBtn.Click += (s, e) => ThemeManager.SetFontSize(12);
            MediumTextBtn.Click += (s, e) => ThemeManager.SetFontSize(14);
            LargeTextBtn.Click += (s, e) => ThemeManager.SetFontSize(18);

            SaveBtn.Click += (s, e) => { SaveSettings(); MessageBox.Show("Settings saved."); };
            CloseBtn.Click += (s, e) => Close();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch { _settings = new UserSettings(); }

            if (_settings.Theme == "Dark") ThemeManager.ApplyDarkTheme();
            else ThemeManager.ApplyLightTheme();
            ThemeManager.SetFontSize(_settings.FontSize);

            CompactRowsCheck.IsChecked = _settings.CompactRows;
        }

        private void SaveSettings()
        {
            _settings.Theme = "Light"; // simplified
            _settings.FontSize = (double)Application.Current.Resources["FontSizeMedium"];
            _settings.CompactRows = CompactRowsCheck.IsChecked == true;

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
    }

    public class UserSettings
    {
        public string Theme { get; set; } = "Light";
        public double FontSize { get; set; } = 14;
        public bool CompactRows { get; set; } = false;
    }
}

