using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace RCL.Core.Services
{
    public class LocalDatabaseService
    {
        private readonly string _dbPath;

        public LocalDatabaseService(string dbPath)
        {
            _dbPath = dbPath;
            Initialize();
        }

        private void Initialize()
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var conn = GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Customers(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Phone TEXT,
                JoinedUtc TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Visits(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                VisitUtc TEXT NOT NULL
            );
            ";
            cmd.ExecuteNonQuery();
        }

        public SqliteConnection GetConnection()
        {
            var cs = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
            return new SqliteConnection(cs);
        }

        // Stubs (replace with real logic later)
        public void EnsureDemoSeed() { }
        public IEnumerable<(string Name,string Phone,DateTime JoinedUtc,int VisitCount)> GetCustomersWithVisits()
            => new List<(string,string,DateTime,int)>();
        public void LogVisit(string name) { }
        public void AddOrUpdateCustomer(string name, string phone) { }
    }
}

