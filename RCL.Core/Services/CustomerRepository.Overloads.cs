using System;

namespace RCL.Core.Services
{
    public partial class CustomerRepository
    {
        // Compatibility overload: allow constructing repository with a path string
        public CustomerRepository(string dbPath) : this(new LocalDatabaseService(dbPath))
        {
        }
    }
}
