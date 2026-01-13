using System;
using RCL.Core.Models;

namespace RCL.Win.ViewModels
{
    public class CustomerViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Points { get; set; }

        public CustomerViewModel() { }

        public CustomerViewModel(Customer c)
        {
            Id = c.Id;
            FirstName = c.FirstName;
            LastName = c.LastName;
            Email = c.Email;
            Phone = c.Phone;
            Points = c.Points;
        }

        public Customer ToModel() => new Customer
        {
            Id = string.IsNullOrWhiteSpace(Id) ? Guid.NewGuid().ToString() : Id,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Phone = Phone,
            Points = Points,
            CreatedAt = DateTime.UtcNow
        };
    }
}
