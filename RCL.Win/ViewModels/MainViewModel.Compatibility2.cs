using System;
using RCL.Core.Services;

namespace RCL.Win.ViewModels
{
    public partial class MainViewModel
    {
        // Constructor overload used by UI code that passes a string path
        public MainViewModel(string dbPath) : this(new CustomerRepository(dbPath), new VisitRepository(dbPath))
        {
        }

        // Compatibility overload expected by some UI code: AddOrUpdateCustomer with a second flag parameter
        public void AddOrUpdateCustomer(object item, bool isUpdate)
        {
            if (item is RCL.Win.CustomerItem ci)
            {
                AddOrUpdateCustomer(ci);
            }
            else
            {
                // best-effort: try to handle other shapes
                try
                {
                    var ci2 = item as dynamic;
                    var tmp = new RCL.Win.CustomerItem
                    {
                        Id = tmpSafe(ci2.Id),
                        FirstName = tmpSafe(ci2.FirstName),
                        LastName = tmpSafe(ci2.LastName),
                        Phone = tmpSafe(ci2.Phone),
                        Points = (int?)ci2.Points ?? 0
                    };
                    AddOrUpdateCustomer(tmp);
                }
                catch
                {
                    // swallow to preserve compatibility; UI will refresh anyway
                }
            }

            static string tmpSafe(object? o) => o?.ToString() ?? string.Empty;
        }
    }
}
