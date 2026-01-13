using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace RCL.Win
{
    public partial class AddCustomerWindow : Window
    {
        private readonly object _db;
        public AddCustomerWindow(object db)
        {
            InitializeComponent();

            // __IDEMPOTENT_ATTACH_MARKER__ - ensures handlers are attached only once
            try { BtnCancel.Click -= BtnCancel_Click; BtnCancel.Click += BtnCancel_Click; } catch { }
            try { BtnSave.Click -= BtnSave_Click; BtnSave.Click += BtnSave_Click; } catch { }

            _db = db;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ValidationText.Visibility = Visibility.Collapsed;

            var name = TxtFullName.Text?.Trim() ?? string.Empty;
            var phone = TxtPhone.Text?.Trim() ?? string.Empty;
            var email = TxtEmail.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                ShowValidation("Full name is required.");
                return;
            }

            try
            {
                var props = new System.Collections.Generic.Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Name"] = name,
                    ["PhoneNumber"] = phone,
                    ["Email"] = email,
                    ["CreatedAt"] = DateTime.UtcNow
                };

                bool saved = TrySaveThroughRepository("CustomerRepository", "Customer", props, out string errorMessage);

                if (!saved)
                {
                    MessageBox.Show("Failed to save customer:\n" + errorMessage, "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error while saving customer:\n" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowValidation(string message)
        {
            ValidationText.Text = message;
            ValidationText.Visibility = Visibility.Visible;
        }

        // --- Reflection-based "best-effort" saver (robust, avoids naming collisions) ---
        private bool TrySaveThroughRepository(string repoNameContains, string modelNameContains, System.Collections.Generic.Dictionary<string, object> props, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // find repository type
                var repoType = assemblies
                    .SelectMany(a => SafeGetTypes(a))
                    .FirstOrDefault(t => t.Name.IndexOf(repoNameContains, StringComparison.OrdinalIgnoreCase) >= 0
                                         && t.Name.EndsWith("Repository", StringComparison.OrdinalIgnoreCase));

                if (repoType == null)
                {
                    errorMsg = $"Repository type containing '{repoNameContains}' not found.";
                    return false;
                }

                // instantiate repository (prefer ctor that accepts our _db)
                object repoInstance = InstantiateRepository(repoType, _db);

                if (repoInstance == null)
                {
                    errorMsg = $"Could not construct repository type {repoType.FullName}.";
                    return false;
                }

                // find model type
                var modelType = assemblies
                    .SelectMany(a => SafeGetTypes(a))
                    .FirstOrDefault(t => t.Name.IndexOf(modelNameContains, StringComparison.OrdinalIgnoreCase) >= 0
                                         && t.IsClass && !t.IsAbstract);

                if (modelType == null)
                {
                    errorMsg = $"Model type containing '{modelNameContains}' not found.";
                    return false;
                }

                // create model instance and set properties
                var model = Activator.CreateInstance(modelType);
                foreach (var kvp in props)
                {
                    var p = modelType.GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p != null && p.CanWrite)
                    {
                        try
                        {
                            p.SetValue(model, ConvertIfNeeded(kvp.Value, p.PropertyType));
                        }
                        catch
                        {
                            // ignore individual set failures
                        }
                    }
                }

                // find a save-like method
                var candidateNames = new[] { "Add", "Insert", "Create", "Save", "Upsert", "AddAsync" };
                MethodInfo saveMethod = null;

                foreach (var name in candidateNames)
                {
                    saveMethod = repoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)
                                             && m.GetParameters().Length == 1);
                    if (saveMethod != null) break;
                }

                if (saveMethod == null)
                {
                    saveMethod = repoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(m =>
                        {
                            var ps = m.GetParameters();
                            return ps.Length == 1 && (ps[0].ParameterType.IsAssignableFrom(modelType) || ps[0].ParameterType == typeof(object));
                        });
                }

                if (saveMethod == null)
                {
                    errorMsg = $"No suitable save method found on repository {repoType.Name}.";
                    return false;
                }

                var result = saveMethod.Invoke(repoInstance, new[] { model });
                if (result is System.Threading.Tasks.Task t) t.GetAwaiter().GetResult();

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
                return false;
            }
        }

        private static object InstantiateRepository(Type repoType, object dbInstance)
        {
            // try ctor that accepts dbInstance's type
            if (dbInstance != null)
            {
                foreach (var ctor in repoType.GetConstructors())
                {
                    var ps = ctor.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(dbInstance.GetType()))
                    {
                        try { return ctor.Invoke(new[] { dbInstance }); } catch { }
                    }
                }
            }

            // try parameterless
            var paramless = repoType.GetConstructor(Type.EmptyTypes);
            if (paramless != null)
            {
                try { return Activator.CreateInstance(repoType); } catch { }
            }

            // fallback: first ctor with default args
            var firstCtor = repoType.GetConstructors().FirstOrDefault();
            if (firstCtor != null)
            {
                var args = firstCtor.GetParameters().Select(p => GetDefault(p.ParameterType)).ToArray();
                try { return firstCtor.Invoke(args); } catch { }
            }

            return string.Empty;
        }

        private static System.Type[] SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); } catch { return System.Type.EmptyTypes; }
        }

        private static object ConvertIfNeeded(object value, Type targetType)
        {
            if (value == null) return string.Empty;
            if (targetType.IsInstanceOfType(value)) return value;
            try
            {
                if (targetType.IsEnum && value is string s) return Enum.Parse(targetType, s, true);
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
            }
            catch { return value; }
        }

        private static object GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
    }
}













