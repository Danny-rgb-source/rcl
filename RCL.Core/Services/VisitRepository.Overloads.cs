using System;

namespace RCL.Core.Services
{
    public partial class VisitRepository
    {
        // Compatibility overload: allow constructing repository with a path string
        public VisitRepository(string dbPath) : this(new LocalDatabaseService(dbPath))
        {
        }
    }
}
