using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace RCL.Win
{
    public partial class App : Application
    {
        private readonly string _settingsPath;

        public App()
        {
            InitializeComponent();

            // Prepare settings file location (local appdata)
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RCL", "settings.json");
            try { Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath) ?? string.Empty); } catch { /* best-effort */ }

            // Global handlers
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Immediately apply saved UI settings (if any)
            try
            {
                var s = GetUiSettings();
                ApplyUiSettings(s);
            }
            catch
            {
                // swallow - don't crash during startup for styling issues
            }
        }

        // Exposed method: retrieve persisted UI settings (or default)
        public UserUiSettings GetUiSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return new UserUiSettings();
                var txt = File.ReadAllText(_settingsPath);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var settings = JsonSerializer.Deserialize<UserUiSettings>(txt, opts);
                return settings ?? new UserUiSettings();
            }
            catch
            {
                return new UserUiSettings();
            }
        }

        // Exposed method: update persisted settings and apply them immediately
        public void UpdateAndApplyUiSettings(UserUiSettings settings)
        {
            if (settings == null) return;
            try
            {
                // Persist
                var dir = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, opts);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // non-fatal
            }

            // Apply to current application resources
            try { ApplyUiSettings(settings); } catch { }
        }

        // Central function: update Application.Current.Resources keys according to UserUiSettings
        private void ApplyUiSettings(UserUiSettings settings)
        {
            if (settings == null) return;

            // Decide color palette for themes (simple tokens)
            bool dark = settings.Theme == UiTheme.Dark;

            var primary = ColorFromHex(dark ? "#1E88E5" : "#1E88E5"); // same blue works both
            var primaryDark = ColorFromHex(dark ? "#1976D2" : "#1976D2");
            var surface = ColorFromHex(dark ? "#0F1720" : "#FFFFFF");
            var cardSurface = ColorFromHex(dark ? "#0B1217" : "#FFFFFF");
            var background = ColorFromHex(dark ? "#071019" : "#F3F7FA");
            var foreground = ColorFromHex(dark ? "#E6EEF8" : "#222222");
            var secondary = ColorFromHex(dark ? "#9AA8B2" : "#6B7280");

            // Write or update resources
            SetResource("PrimaryBrush", new SolidColorBrush(primary));
            SetResource("PrimaryBrushDark", new SolidColorBrush(primaryDark));
            SetResource("SurfaceBrush", new SolidColorBrush(surface));
            SetResource("CardSurface", new SolidColorBrush(cardSurface));
            SetResource("WindowBackground", new SolidColorBrush(background));
            SetResource("BackgroundBrush", new SolidColorBrush(background));
            SetResource("ForegroundBrush", new SolidColorBrush(foreground));
            SetResource("SecondaryBrush", new SolidColorBrush(secondary));

            // Font tokens
            SetResource("AppFontSize", settings.FontSize);
            SetResource("AppFontFamily", new System.Windows.Media.FontFamily(settings.FontFamily ?? "Segoe UI"));

            // Freeze brushes where possible to improve rendering (defensive)
            TryFreezeBrushInResources("PrimaryBrush");
            TryFreezeBrushInResources("PrimaryBrushDark");
            TryFreezeBrushInResources("SurfaceBrush");
            TryFreezeBrushInResources("CardSurface");
            TryFreezeBrushInResources("WindowBackground");
            TryFreezeBrushInResources("BackgroundBrush");
            TryFreezeBrushInResources("ForegroundBrush");
            TryFreezeBrushInResources("SecondaryBrush");

            // If desired: notify existing windows to refresh some visuals by walking windows; not always needed
            try
            {
                foreach (Window w in Current.Windows)
                {
                    // Force theme re-evaluation for data-bound properties by updating Layout
                    w.Dispatcher.Invoke(() =>
                    {
                        w.Resources.MergedDictionaries.Clear(); // leave empty - using global resources directly
                        w.InvalidateVisual();
                        w.UpdateLayout();
                    }, DispatcherPriority.ApplicationIdle);
                }
            }
            catch
            {
                // ignore
            }
        }

        private static Color ColorFromHex(string hex)
        {
            try
            {
                // ColorConverter returns System.Windows.Media.Color
                var c = (Color)ColorConverter.ConvertFromString(hex);
                return c;
            }
            catch
            {
                return Colors.Transparent;
            }
        }

        private void SetResource(string key, object value)
        {
            try
            {
                if (Application.Current == null) return;
                if (Application.Current.Resources.Contains(key))
                    Application.Current.Resources[key] = value;
                else
                    Application.Current.Resources.Add(key, value);
            }
            catch
            {
                // ignore
            }
        }

        private void TryFreezeBrushInResources(string key)
        {
            try
            {
                if (Application.Current == null) return;
                if (Application.Current.Resources.Contains(key) && Application.Current.Resources[key] is SolidColorBrush sb && sb.CanFreeze)
                {
                    sb.Freeze();
                    // reassign a frozen brush so consumers grab the frozen instance
                    Application.Current.Resources[key] = sb;
                }
            }
            catch { }
        }

        // Exception handlers (unchanged)
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RCL", "Logs");
                try { Directory.CreateDirectory(logDir); } catch { }
                var file = Path.Combine(logDir, $"rcl_errors_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(file, $"[{DateTime.UtcNow:O}] DispatcherUnhandledException: {e.Exception}\n\n");
                MessageBox.Show($"Unhandled UI exception: {e.Exception.Message}", "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RCL", "Logs");
                try { Directory.CreateDirectory(logDir); } catch { }
                var file = Path.Combine(logDir, $"rcl_errors_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(file, $"[{DateTime.UtcNow:O}] TaskScheduler_UnobservedTaskException: {e.Exception}\n\n");
                MessageBox.Show($"Unhandled background exception: {e.Exception.Message}", "Background error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch { }
            try { e.SetObserved(); } catch { }
        }

        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception ?? new Exception("Unhandled non-Exception");
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RCL", "Logs");
                try { Directory.CreateDirectory(logDir); } catch { }
                var file = Path.Combine(logDir, $"rcl_errors_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(file, $"[{DateTime.UtcNow:O}] AppDomain.UnhandledException: {ex}\n\n");
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            try
            {
                this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            }
            catch { }
        }
    }
}

