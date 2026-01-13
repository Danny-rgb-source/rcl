using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using RCL.Core.Models;

namespace RCL.Win.ViewModels
{
    /// <summary>
    /// ViewModel wrapper around <see cref="Customer"/> for data-binding.
    /// Implements INotifyPropertyChanged and exposes a settable RewardAvailable
    /// so TwoWay bindings in XAML will not throw on write attempts.
    /// </summary>
    public class CustomerViewModel : INotifyPropertyChanged
    {
        private readonly Customer _c;

        // Local backing fields so the VM can be updated independently of the model,
        // and changes can be pushed back to the model when SaveToModel() is called.
        private string _name;
        private string _phoneNumber;
        private string _email;
        private int _visitCount;
        private bool _rewardAvailable;
        private DateTime _createdAt;

        public CustomerViewModel(Customer c)
        {
            _c = c ?? throw new ArgumentNullException(nameof(c));

            // initialize backing fields from the model
            _name = _c.Name ?? string.Empty;
            _phoneNumber = _c.PhoneNumber ?? string.Empty;
            _email = _c.Email ?? string.Empty;
            _visitCount = _c.VisitCount;
            _rewardAvailable = _c.RewardAvailable;
            _createdAt = _c.CreatedAt;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        // Id is read-only (identifies the underlying model)
        public string Id => _c.Id;

        // Editable/viewable properties — update backing fields and raise notifications.
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber == value) return;
                _phoneNumber = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (_email == value) return;
                _email = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public int VisitCount
        {
            get => _visitCount;
            set
            {
                if (_visitCount == value) return;
                _visitCount = value;
                OnPropertyChanged();
                // If RewardAvailable is derived from VisitCount in your domain,
                // consider updating RewardAvailable here or call RefreshFromModel/SaveToModel as appropriate.
            }
        }

        /// <summary>
        /// Exposed as read/write on the ViewModel so TwoWay bindings won't throw.
        /// Changes are applied to the local backing field; call SaveToModel() to persist to the underlying model if it supports writing.
        /// </summary>
        public bool RewardAvailable
        {
            get => _rewardAvailable;
            set
            {
                if (_rewardAvailable == value) return;
                _rewardAvailable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Formatted CreatedAt for display; updates when CreatedAt backing field changes.
        /// </summary>
        public string CreatedAtFormatted =>
            _createdAt == default ? string.Empty : _createdAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

        /// <summary>
        /// Expose the underlying model for edit/save operations.
        /// Use SaveToModel() to push changes from the VM back to the model safely.
        /// </summary>
        public Customer Model => _c;

        /// <summary>
        /// Pushes editable ViewModel fields back into the underlying Customer model if the model has writable properties.
        /// Uses reflection to avoid compile-time coupling to model setter availability.
        /// </summary>
        public void SaveToModel()
        {
            // Attempt to set writable properties on the model if available.
            // If a property isn't writable we simply skip it.
            TrySetModelProperty(nameof(Customer.Name), _name);
            TrySetModelProperty(nameof(Customer.PhoneNumber), _phoneNumber);
            TrySetModelProperty(nameof(Customer.Email), _email);
            TrySetModelProperty(nameof(Customer.VisitCount), _visitCount);
            TrySetModelProperty(nameof(Customer.RewardAvailable), _rewardAvailable);
            TrySetModelProperty(nameof(Customer.CreatedAt), _createdAt);
        }

        /// <summary>
        /// Refresh ViewModel fields from the underlying model. Useful if the model was updated externally.
        /// Raises PropertyChanged for all bound properties.
        /// </summary>
        public void RefreshFromModel()
        {
            _name = _c.Name ?? string.Empty;
            _phoneNumber = _c.PhoneNumber ?? string.Empty;
            _email = _c.Email ?? string.Empty;
            _visitCount = _c.VisitCount;
            _rewardAvailable = _c.RewardAvailable;
            _createdAt = _c.CreatedAt;

            // Raise notifications for the UI to update
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(VisitCount));
            OnPropertyChanged(nameof(RewardAvailable));
            OnPropertyChanged(nameof(CreatedAtFormatted));
        }

        /// <summary>
        /// Helper: try to set a property on the model if it exists and is writable.
        /// Uses reflection so SaveToModel() is safe even if some model properties are read-only.
        /// </summary>
        private void TrySetModelProperty(string propertyName, object? value)
        {
            try
            {
                var prop = typeof(Customer).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && prop.CanWrite)
                {
                    // Attempt to convert simple types if necessary
                    object? converted = value;
                    if (value != null && prop.PropertyType != value.GetType())
                    {
                        try
                        {
                            converted = Convert.ChangeType(value, prop.PropertyType);
                        }
                        catch
                        {
                            // if conversion fails, skip setting this property
                            return;
                        }
                    }

                    prop.SetValue(_c, converted);
                }
            }
            catch
            {
                // Intentionally swallow exceptions to avoid crashing the UI when model doesn't support setting.
                // In production you may want to log this.
            }
        }
    }
}

