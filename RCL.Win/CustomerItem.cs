using System;
using RCL.Core.Models;

namespace RCL.Win
{
    // Duplicate of the UI DTO placed in the RCL.Win root namespace so existing UI references resolve.
    public class CustomerItem
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Points { get; set; }

        public CustomerItem() { }

        public CustomerItem(Customer c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));
            Id = c.Id;
            FirstName = c.FirstName;
            LastName = c.LastName;
            Phone = c.Phone;
            Points = c.Points;
        }

        public Customer ToModel() => new Customer
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString() : Id,
            Name = string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName) ? string.Empty : (FirstName + (string.IsNullOrWhiteSpace(LastName) ? "" : " " + LastName)),
            PhoneNumber = Phone,
            VisitCount = Points,
            CreatedAt = DateTime.UtcNow
        };
    }
}
