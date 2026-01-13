using System;
using System.Linq;
using System.Windows;

namespace RCL.Win.Helpers.Services
{
    public static class ThemeManager
    {
        public static void ApplyLightTheme()
        {
            ReplaceColorDictionary("Resources/Colors.Light.xaml");
        }

        public static void ApplyDarkTheme()
        {
            ReplaceColorDictionary("Resources/Colors.Dark.xaml");
        }

        public static void SetFontSize(double size)
        {
            var dict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("ThemeTokens.xaml"));
            if (dict != null)
            {
                dict["FontSizeMedium"] = size;
            }
        }

        private static void ReplaceColorDictionary(string uri)
        {
            var app = Application.Current;
            var existing = app.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("Colors.Light") || d.Source.OriginalString.Contains("Colors.Dark")));
            var newDict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };

            if (existing != null)
            {
                var index = app.Resources.MergedDictionaries.IndexOf(existing);
                app.Resources.MergedDictionaries[index] = newDict;
            }
            else
            {
                app.Resources.MergedDictionaries.Insert(0, newDict);
            }
        }
    }
}

