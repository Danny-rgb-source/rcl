using System.Windows;

namespace RCL.Win.Views
{
    public partial class CustomerDialog : Window
    {
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerPhone { get; private set; } = string.Empty;

        public CustomerDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            CustomerName = txtName.Text.Trim();
            CustomerPhone = txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhone))
            {
                MessageBox.Show("Please provide name and phone.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

