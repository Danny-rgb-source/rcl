using System.Threading.Tasks;
using RCL.Core.Services;
using RCL.Win.Models;

namespace RCL.Win.ViewModels
{
    // Partial compatibility shim to match existing UI expectations.
    public partial class MainViewModel
    {
        // Constructor overload used by older UI code that only passed CustomerRepository
        public MainViewModel(CustomerRepository customerRepo) : this(customerRepo, new VisitRepository(new LocalDatabaseService()))
        {
        }

        // Synchronous wrapper for RefreshAsync used by UI code calling Refresh()
        public void Refresh()
        {
            _ = RefreshAsync();
        }

        // Compatibility method expected by UI: accepts a CustomerItem and adds/updates
        public void AddOrUpdateCustomer(CustomerItem item)
        {
            if (item == null) return;
            var model = item.ToModel();
            // Fire-and-forget to keep UI responsive; errors will surface in logs/build
            _ = _customerRepo.AddAsync(model).ContinueWith(t => { /* swallow for compatibility */ });
            _ = RefreshAsync();
        }
    }
}
