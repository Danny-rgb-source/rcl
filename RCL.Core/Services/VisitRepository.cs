using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using RCL.Core.Models;

namespace RCL.Core.Services
{
    public class VisitRepository
    {
        private readonly LocalDatabaseService _db;

        public VisitRepository(LocalDatabaseService db)
        {
            _db = db;
        }

        public void AddVisit(Visit visit)
        {
            if (visit == null) throw new ArgumentNullException(nameof(visit));
            if (string.IsNullOrWhiteSpace(visit.Id)) visit.Id = Guid.NewGuid().ToString();

            using var conn = _db.GetConnection();
            conn.Open();

            conn.Execute(@"
INSERT OR REPLACE INTO Visits (Id, CustomerId, BusinessId, Timestamp, Amount)
VALUES (@Id, @CustomerId, @BusinessId, @Timestamp, @Amount)",
                new
                {
                    visit.Id,
                    visit.CustomerId,
                    visit.BusinessId,
                    Timestamp = visit.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                    Amount = visit.Amount
                });
        }

        public IEnumerable<Visit> GetVisitsForCustomer(string customerId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var rows = conn.Query("SELECT Id, CustomerId, BusinessId, Timestamp, Amount FROM Visits WHERE CustomerId = @CustomerId ORDER BY Timestamp DESC", new { CustomerId = customerId });
            return rows.Select(MapRowToVisit).ToList();
        }

        public IEnumerable<Visit> GetVisitsForBusiness(string businessId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var rows = conn.Query("SELECT Id, CustomerId, BusinessId, Timestamp, Amount FROM Visits WHERE BusinessId = @BusinessId ORDER BY Timestamp DESC", new { BusinessId = businessId });
            return rows.Select(MapRowToVisit).ToList();
        }

        public IEnumerable<Visit> GetAllVisits()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var rows = conn.Query("SELECT Id, CustomerId, BusinessId, Timestamp, Amount FROM Visits ORDER BY Timestamp DESC");
            return rows.Select(MapRowToVisit).ToList();
        }

        public int GetVisitCountForCustomerBusiness(string customerId, string businessId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            return conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Visits WHERE CustomerId = @Cid AND BusinessId = @Bid", new { Cid = customerId, Bid = businessId });
        }

        /// <summary>
        /// Returns the number of rewards earned by integer division: visits / visitsRequired.
        /// </summary>
        public int CalculateRewards(string customerId, string businessId, int visitsRequired)
        {
            if (visitsRequired <= 0) return 0;
            var count = GetVisitCountForCustomerBusiness(customerId, businessId);
            return count / visitsRequired;
        }

        private static Visit MapRowToVisit(dynamic row)
        {
            var v = new Visit
            {
                Id = row.Id,
                CustomerId = row.CustomerId,
                BusinessId = row.BusinessId,
                Amount = row.Amount is null ? 0m : Convert.ToDecimal(row.Amount)
            };

            try
            {
                var s = row.Timestamp as string;
                if (!string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt))
                    v.Timestamp = dt;
            }
            catch { }

            return v;
        }
    }
}

