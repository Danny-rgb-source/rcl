using System;

namespace RCL.Core.Models
{
    public partial class Visit
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = string.Empty;
        public string BusinessId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; } = 0m; // optional: for spend-based rules
    }
}


