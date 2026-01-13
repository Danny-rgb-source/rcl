using System;

namespace RCL.Core.Models
{
    public partial class Customer
    {
        // UI expects FirstName / LastName while core stores Name.
        public string FirstName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return string.Empty;
                var parts = Name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 ? parts[0] : string.Empty;
            }
            set
            {
                var last = LastName;
                Name = string.IsNullOrWhiteSpace(value) ? last : (string.IsNullOrWhiteSpace(last) ? value : value + " " + last);
            }
        }

        public string LastName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return string.Empty;
                var parts = Name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1] : string.Empty;
            }
            set
            {
                var first = FirstName;
                Name = string.IsNullOrWhiteSpace(first) ? value : (string.IsNullOrWhiteSpace(value) ? first : first + " " + value);
            }
        }

        // Phone maps to PhoneNumber
        public string Phone
        {
            get => PhoneNumber;
            set => PhoneNumber = value;
        }

        // Points maps to VisitCount
        public int Points
        {
            get => VisitCount;
            set => VisitCount = value;
        }
    }
}
