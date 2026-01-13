using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using RCL.Core.Models;

namespace RCL.Core.Services
{
    public class CustomerRepository
    {
        private readonly LocalDatabaseService _db;

        public CustomerRepository(LocalDatabaseService db)
        {
            _db = db;
        }

        public void AddCustomer(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            if (string.IsNullOrWhiteSpace(customer.Id)) customer.Id = Guid.NewGuid().ToString();

            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute(@"
INSERT OR REPLACE INTO Customers
(Id, Name, Email, PhoneNumber, VisitCount, RewardAvailable, CreatedAt)
VALUES
(@Id, @Name, @Email, @PhoneNumber, @VisitCount, @RewardAvailable, @CreatedAt)",
                new
                {
                    customer.Id,
                    Name = customer.Name ?? string.Empty,
                    Email = customer.Email ?? string.Empty,
                    PhoneNumber = customer.PhoneNumber ?? string.Empty,
                    VisitCount = customer.VisitCount,
                    RewardAvailable = customer.RewardAvailable ? 1 : 0,
                    CreatedAt = customer.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
                });
        }

        public void UpdateCustomer(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute(@"
UPDATE Customers
SET Name = @Name,
    Email = @Email,
    PhoneNumber = @PhoneNumber,
    VisitCount = @VisitCount,
    RewardAvailable = @RewardAvailable
WHERE Id = @Id",
                new
                {
                    customer.Id,
                    Name = customer.Name ?? string.Empty,
                    Email = customer.Email ?? string.Empty,
                    PhoneNumber = customer.PhoneNumber ?? string.Empty,
                    VisitCount = customer.VisitCount,
                    RewardAvailable = customer.RewardAvailable ? 1 : 0
                });
        }

        public void DeleteCustomer(string id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            conn.Execute("DELETE FROM Customers WHERE Id = @Id", new { Id = id });
        }

        public Customer? GetCustomer(string id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var row = conn.QueryFirstOrDefault(
                "SELECT Id, Name, Email, PhoneNumber, VisitCount, RewardAvailable, CreatedAt FROM Customers WHERE Id = @Id",
                new { Id = id });

            if (row == null) return null;

            return MapRowToCustomer(row);
        }

        public IEnumerable<Customer> GetAll()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var rows = conn.Query("SELECT Id, Name, Email, PhoneNumber, VisitCount, RewardAvailable, CreatedAt FROM Customers");
            return rows.Select(MapRowToCustomer).ToList();
        }

        public void IncrementVisitCount(string customerId, int increment = 1)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute("UPDATE Customers SET VisitCount = VisitCount + @Inc WHERE Id = @Id", new { Inc = increment, Id = customerId });
        }

        public void SetRewardAvailable(string customerId, bool available)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute("UPDATE Customers SET RewardAvailable = @Av WHERE Id = @Id", new { Av = available ? 1 : 0, Id = customerId });
        }

        private static Customer MapRowToCustomer(dynamic row)
        {
            // Dapper returns dynamic. Map defensively.
            var c = new Customer
            {
                Id = row.Id,
                Name = row.Name ?? string.Empty,
                Email = row.Email ?? string.Empty,
                PhoneNumber = row.PhoneNumber ?? string.Empty,
                VisitCount = row.VisitCount is int vi ? vi : Convert.ToInt32(row.VisitCount),
                RewardAvailable = (row.RewardAvailable is int ri ? ri : Convert.ToInt32(row.RewardAvailable)) != 0
            };

            // CreatedAt parsing
            try
            {
                var created = row.CreatedAt as string;
                if (!string.IsNullOrWhiteSpace(created) && DateTime.TryParse(created, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                {
                    c.CreatedAt = dt;
                }
            }
            catch { /* ignore */ }

            return c;
        }
    }
}

