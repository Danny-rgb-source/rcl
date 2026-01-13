using System;
using System.Windows;
using System.Windows.Media;

namespace RCL.Win.Helpers
{
    public static class ThemeManager
    {
        private static Brush ToBrush(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Brushes.Transparent;
            try
            {
                var conv = new BrushConverter();
                var o = conv.ConvertFromString(hex);
                return o as Brush ?? Brushes.Transparent;
            }
            catch
            {
                try
                {
                    // fallback: try ColorConverter->SolidColorBrush
                    var c = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(c);
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
        }

        public static void ApplyLightTheme()
        {
            var r = Application.Current.Resources;
            r["WindowBackground"] = ToBrush("#FFFFFFFF");
            r["WindowForeground"] = ToBrush("#FF111111");
            r["SurfaceBrush"] = ToBrush("#FFF8F9FA");
            r["ControlBackground"] = ToBrush("#FFFFFFFF");
            r["ControlBorderBrush"] = ToBrush("#FFDDDDDD");
            r["SubtleBorderBrush"] = ToBrush("#FFE6E6E6");
            r["AccentBrush"] = ToBrush("#FF0B8A3E");
            r["AccentDarkBrush"] = ToBrush("#FF08733B");
            r["DangerBrush"] = ToBrush("#FFC62828");
            r["DangerDarkBrush"] = ToBrush("#FF9B1F1F");
            r["MutedTextBrush"] = ToBrush("#FF666666");
            r["CardBackground"] = ToBrush("#FFFFFFFF");
        }

        public static void ApplyDarkTheme()
        {
            var r = Application.Current.Resources;
            r["WindowBackground"] = ToBrush("#FF202124");
            r["WindowForeground"] = ToBrush("#FFF1F1F1");
            r["SurfaceBrush"] = ToBrush("#FF2A2C2E");
            r["ControlBackground"] = ToBrush("#FF232426");
            r["ControlBorderBrush"] = ToBrush("#FF3A3C3F");
            r["SubtleBorderBrush"] = ToBrush("#FF313335");
            r["AccentBrush"] = ToBrush("#FF4CAF50");
            r["AccentDarkBrush"] = ToBrush("#FF388E3C");
            r["DangerBrush"] = ToBrush("#FFEF5350");
            r["DangerDarkBrush"] = ToBrush("#FFE53935");
            r["MutedTextBrush"] = ToBrush("#FFAAAAAA");
            r["CardBackground"] = ToBrush("#FF252627");
        }

        /// <summary>
        /// Set the application font size dynamically.
        /// </summary>
        public static void SetFontSize(double size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            Application.Current.Resources["AppFontSize"] = size;
        }
    }
}

