using System;

namespace RCL.Core.Services
{
    public partial class LocalDatabaseService
    {
        // Compatibility overload: accept a dbPath string (some UI code constructs services with a path)
        public LocalDatabaseService(string dbPath) : this(baseFolder: dbPath)
        {
        }
    }
}
