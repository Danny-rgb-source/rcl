using System.Windows;

namespace RCL.Win.Views
{
    public partial class CustomerDialogWindow : Window
    {
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerPhone { get; private set; } = string.Empty;

        public CustomerDialogWindow()
        {
            InitializeComponent();
            OkBtn.Click += (s, e) =>
            {
                CustomerName = NameBox.Text ?? string.Empty;
                CustomerPhone = PhoneBox.Text ?? string.Empty;
                DialogResult = true;
            };
            CancelBtn.Click += (s, e) => { DialogResult = false; };
        }
    }
}

