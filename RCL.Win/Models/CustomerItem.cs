using System;
using RCL.Core.Models;

namespace RCL.Win.Models
{
    // Lightweight DTO used by existing UI code that expected a CustomerItem type.
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
            FirstName = FirstName,
            LastName = LastName,
            Phone = Phone,
            Points = Points,
            CreatedAt = DateTime.UtcNow
        };
    }
}
