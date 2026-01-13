using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RCL.Core.Models
{
    public partial class Customer : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private int _visitCount;
        private bool _rewardAvailable;
        private DateTime _createdAt = DateTime.UtcNow;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        public int VisitCount
        {
            get => _visitCount;
            set { _visitCount = value; OnPropertyChanged(); }
        }

        public bool RewardAvailable
        {
            get => _rewardAvailable;
            set { _rewardAvailable = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}


