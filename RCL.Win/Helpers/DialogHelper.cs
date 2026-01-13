using System.Threading.Tasks;
using RCL.Win.ViewModels;

namespace RCL.Win.Helpers
{
    /// <summary>
    /// Lightweight dialog helper used by ViewModels to open windows.
    /// Implementations call WPF windows; here we provide async-friendly stubs
    /// that the UI windows will call into. When the real windows exist they
    /// should call the same methods and return the expected results.
    /// </summary>
    public static class DialogHelper
    {
        // These methods are intentionally simple and return sensible defaults.
        // When the actual windows are implemented, replace the bodies with
        // code that opens the WPF windows and returns the results.

        public static Task<CustomerViewModel?> ShowAddCustomerDialogAsync()
        {
            // TODO: Replace with real dialog invocation.
            return Task.FromResult<CustomerViewModel?>(null);
        }

        public static Task<bool> ShowLogVisitDialogAsync(CustomerViewModel customer)
        {
            // TODO: Replace with real dialog invocation.
            return Task.FromResult(false);
        }

        public static void ShowSettingsDialog()
        {
            // TODO: Replace with real dialog invocation.
        }
    }
}
