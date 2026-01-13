using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using RCL.Core.Models;
using System.Text.Json;

namespace RCL.Core.Services
{
    public class BusinessRepository
    {
        private readonly LocalDatabaseService _db;

        public BusinessRepository(LocalDatabaseService db)
        {
            _db = db;
        }

        public void AddBusiness(Business business)
        {
            if (business == null) throw new ArgumentNullException(nameof(business));
            if (string.IsNullOrWhiteSpace(business.Id)) business.Id = Guid.NewGuid().ToString();
            business.CreatedAt = business.CreatedAt == default ? DateTime.UtcNow : business.CreatedAt;

            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute(@"
INSERT OR REPLACE INTO Businesses (Id, Name, RewardRuleJson, CreatedAt)
VALUES (@Id, @Name, @RewardRuleJson, @CreatedAt)",
                new
                {
                    business.Id,
                    Name = business.Name ?? string.Empty,
                    RewardRuleJson = business.RewardRuleJson ?? string.Empty,
                    CreatedAt = business.CreatedAt.ToString("o", CultureInfo.InvariantCulture)
                });
        }

        public void UpdateBusiness(Business business)
        {
            if (business == null) throw new ArgumentNullException(nameof(business));

            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute(@"
UPDATE Businesses
SET Name = @Name,
    RewardRuleJson = @RewardRuleJson
WHERE Id = @Id",
                new
                {
                    business.Id,
                    Name = business.Name ?? string.Empty,
                    RewardRuleJson = business.RewardRuleJson ?? string.Empty
                });
        }

        public void DeleteBusiness(string id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            conn.Execute("DELETE FROM Businesses WHERE Id = @Id", new { Id = id });
        }

        public Business? GetBusiness(string id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var row = conn.QueryFirstOrDefault("SELECT Id, Name, RewardRuleJson, CreatedAt FROM Businesses WHERE Id = @Id", new { Id = id });
            if (row == null) return null;

            var b = new Business
            {
                Id = row.Id,
                Name = row.Name ?? string.Empty,
                RewardRuleJson = row.RewardRuleJson ?? string.Empty
            };

            try
            {
                var created = row.CreatedAt as string;
                if (!string.IsNullOrWhiteSpace(created) && DateTime.TryParse(created, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                    b.CreatedAt = dt;
            }
            catch { }

            return b;
        }

        public IEnumerable<Business> GetAll()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var rows = conn.Query("SELECT Id, Name, RewardRuleJson, CreatedAt FROM Businesses");
            return rows.Select(r =>
            {
                var business = new Business
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty,
                    RewardRuleJson = r.RewardRuleJson ?? string.Empty
                };

                try
                {
                    var created = r.CreatedAt as string;
                    if (!string.IsNullOrWhiteSpace(created) && DateTime.TryParse(created, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                        business.CreatedAt = dt;
                }
                catch { }

                return business;
            }).ToList();
        }

        /// <summary>
        /// Convenience helper: returns the deserialized RewardRule for the business (or default if missing).
        /// </summary>
        public RewardRule GetRewardRuleForBusiness(string businessId)
        {
            var b = GetBusiness(businessId);
            if (b == null) return RewardRule.Default();

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var rr = JsonSerializer.Deserialize<RewardRule>(b.RewardRuleJson ?? string.Empty, options);
                return rr ?? RewardRule.Default();
            }
            catch
            {
                return RewardRule.Default();
            }
        }

        public void SetRewardRuleForBusiness(string businessId, RewardRule rule)
        {
            var b = GetBusiness(businessId);
            if (b == null) throw new InvalidOperationException("Business not found.");

            b.SetRewardRule(rule);
            UpdateBusiness(b);
        }
    }
}

