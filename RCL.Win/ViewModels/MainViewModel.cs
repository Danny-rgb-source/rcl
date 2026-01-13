using System;
using System.Collections.ObjectModel;
using RCL.Core.Models;
using RCL.Core.Services;

namespace RCL.Win.ViewModels
{
    public class CustomerItem
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Visits { get; set; }
        public DateTime Joined { get; set; }
        public bool RewardEligible { get; set; }
    }

    public class MainViewModel
    {
        private readonly LocalDatabaseService _db;
        private readonly RewardRule _rule = RewardRule.Default();

        public ObservableCollection<CustomerItem> Customers { get; } = new ObservableCollection<CustomerItem>();

        public MainViewModel(string dbPath)
        {
            _db = new LocalDatabaseService(dbPath);
            _db.EnsureDemoSeed();
            Refresh();
        }

        public void Refresh()
        {
            Customers.Clear();
            foreach (var c in _db.GetCustomersWithVisits())
            {
                Customers.Add(new CustomerItem {
                    Name = c.Name,
                    Phone = c.Phone,
                    Visits = c.VisitCount,
                    Joined = c.JoinedUtc.ToLocalTime(),
                    RewardEligible = _rule.IsEligible(c.VisitCount)
                });
            }
        }

        public void LogVisit(string name)
        {
            _db.LogVisit(name);
            Refresh();
        }

        public void AddOrUpdateCustomer(string name, string phone)
        {
            _db.AddOrUpdateCustomer(name, phone);
            Refresh();
        }
    }
}

