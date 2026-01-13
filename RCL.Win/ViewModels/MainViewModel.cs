using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using RCL.Core.Models;
using RCL.Core.Services;

namespace RCL.Win.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private readonly CustomerRepository _customerRepo;
        private readonly VisitRepository _visitRepo;

        public ObservableCollection<CustomerViewModel> Customers { get; } = new();
        public ICommand AddCustomerCommand { get; }
        public ICommand LogVisitCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public MainViewModel(CustomerRepository customerRepo, VisitRepository visitRepo)
        {
            _customerRepo = customerRepo ?? throw new ArgumentNullException(nameof(customerRepo));
            _visitRepo = visitRepo ?? throw new ArgumentNullException(nameof(visitRepo));

            AddCustomerCommand = new RelayCommand(async _ => await AddCustomerAsync());
            LogVisitCommand = new RelayCommand(async param => await LogVisitAsync(param as CustomerViewModel));
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // Fire-and-forget initial load
            _ = RefreshAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task RefreshAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Customers.Clear();
                var list = await _customerRepo.GetAllAsync().ConfigureAwait(false);
                foreach (var c in list.OrderByDescending(x => x.LastVisitAt ?? x.CreatedAt))
                {
                    Customers.Add(new CustomerViewModel(c));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddCustomerAsync()
        {
            // Open AddCustomer dialog via helper; returns CustomerViewModel or null
            var vm = await Helpers.DialogHelper.ShowAddCustomerDialogAsync();
            if (vm == null) return;

            var model = vm.ToModel();
            await _customerRepo.AddAsync(model).ConfigureAwait(false);
            await RefreshAsync().ConfigureAwait(false);
        }

        private async Task LogVisitAsync(CustomerViewModel? customer)
        {
            if (customer == null) return;

            var success = await Helpers.DialogHelper.ShowLogVisitDialogAsync(customer);
            if (!success) return;

            var visit = new Visit
            {
                CustomerId = customer.Id,
                BusinessId = string.Empty,
                VisitAt = DateTime.UtcNow,
                Amount = 0m,
                Notes = "Logged from UI"
            };

            await _visitRepo.AddAsync(visit).ConfigureAwait(false);
            // update last visit
            var model = await _customerRepo.GetByIdAsync(customer.Id).ConfigureAwait(false);
            if (model != null)
            {
                model.LastVisitAt = DateTime.UtcNow;
                await _customerRepo.UpdateAsync(model).ConfigureAwait(false);
            }

            await RefreshAsync().ConfigureAwait(false);
        }

        private void OpenSettings()
        {
            Helpers.DialogHelper.ShowSettingsDialog();
        }
    }
}

