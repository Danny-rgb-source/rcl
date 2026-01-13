using System;
using System.Windows;

namespace RCL.Win.Helpers
{
    public static class DialogHelper
    {
        public static void ShowInfo(string message, string caption = "Info")
        {
            MessageBox.Show(message ?? string.Empty, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowWarning(string message, string caption = "Warning")
        {
            MessageBox.Show(message ?? string.Empty, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(message ?? string.Empty, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowException(Exception ex, string caption = "Error")
        {
            if (ex == null) return;
            var msg = ex.Message + Environment.NewLine + (ex.InnerException?.Message ?? string.Empty);
            MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

