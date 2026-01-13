using System;
using System.Windows;
using RCL.Win.ViewModels;

namespace RCL.Win
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel(System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "rcl_local.db"));
            DataContext = _vm;
            CustomerGrid.ItemsSource = _vm.Customers;

            LoadedCountText.Text = _vm.Customers.Count.ToString();

            LogVisitButton.Click += (s, e) =>
            {
                var selected = CustomerGrid.SelectedItem as CustomerItem;
                if (selected == null)
                {
                    MessageBox.Show("Please select a customer first.");
                    return;
                }
                var dlg = new RCL.Win.Views.LogVisitWindow(selected.Name, selected.Visits);
                dlg.ShowDialog();
                _vm.Refresh();
                LoadedCountText.Text = _vm.Customers.Count.ToString();
            };

            RefreshButton.Click += (s, e) =>
            {
                _vm.Refresh();
                LoadedCountText.Text = _vm.Customers.Count.ToString();
            };

            AddCustomerButton.Click += (s, e) =>
            {
                var dlg = new RCL.Win.Views.CustomerDialogWindow();
                if (dlg.ShowDialog() == true)
                {
                    _vm.AddOrUpdateCustomer(dlg.CustomerName, dlg.CustomerPhone);
                    LoadedCountText.Text = _vm.Customers.Count.ToString();
                }
            };

            SettingsButton.Click += (s, e) =>
            {
                var settingsPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "rcl_user_settings.json");
                var sw = new RCL.Win.Views.SettingsWindow(settingsPath);
                sw.ShowDialog();
            };
        }
    }
}

