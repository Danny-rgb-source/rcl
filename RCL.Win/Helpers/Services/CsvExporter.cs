using Microsoft.Win32;
using RCL.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RCL.Win.Services
{
    public static class CsvExporter
    {
        /// <summary>
        /// Exports the provided customers to a CSV file. If path is null, shows SaveFileDialog.
        /// Returns the path written or null if cancelled.
        /// </summary>
        public static async Task<string?> ExportCustomersAsync(IEnumerable<Customer> customers, string? path = null)
        {
            if (path == null)
            {
                var dlg = new SaveFileDialog()
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"customers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv"
                };

                var ok = dlg.ShowDialog();
                if (ok != true) return null;
                path = dlg.FileName;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,PhoneNumber,VisitCount,RewardAvailable,CreatedAt");

            foreach (var c in customers)
            {
                var line = $"{Escape(c.Id)},{Escape(c.Name)},{Escape(c.Email)},{Escape(c.PhoneNumber)},{c.VisitCount},{(c.RewardAvailable ? 1 : 0)},{Escape(c.CreatedAt.ToString("o"))}";
                sb.AppendLine(line);
            }

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        private static string Escape(string? s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
            {
                return $"\"{s.Replace("\"", "\"\"")}\"";
            }
            return s;
        }
    }
}

