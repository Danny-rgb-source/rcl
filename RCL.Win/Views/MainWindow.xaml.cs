using System;
using System.Windows;
using RCL.Win.ViewModels;

namespace RCL.Win.Views
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
        }
    }
}
