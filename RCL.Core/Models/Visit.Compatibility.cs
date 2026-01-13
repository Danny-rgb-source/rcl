using System;

namespace RCL.Core.Models
{
    public partial class Visit
    {
        // UI expects VisitAt; core stores Timestamp
        public DateTime VisitAt
        {
            get => Timestamp;
            set => Timestamp = value;
        }

        // UI expects Notes; core did not have it — add a backing field
        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => _notes = value ?? string.Empty;
        }
    }
}
