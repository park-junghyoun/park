using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CellManager.Configuration;
using CellManager.Models.TestProfile;

namespace CellManager.ViewModels.TestSetup
{
    /// <summary>
    ///     Base view model that coordinates validation and dialog commands for test profile editors.
    /// </summary>
    public abstract partial class TestProfileDetailViewModel : ObservableObject, IDataErrorInfo
    {
        private readonly IReadOnlyList<string> _validatedFields;

        protected TestProfileDetailViewModel(TestProfileType profileType)
        {
            ProfileType = profileType;
            _validatedFields = TestProfileValidationRules.GetFieldNames(ProfileType);

            SaveCommand = new RelayCommand<Window>(ExecuteSave, CanSave);
            CancelCommand = new RelayCommand<Window>(ExecuteCancel);
        }

        public TestProfileType ProfileType { get; }

        public RelayCommand<Window> SaveCommand { get; }

        public RelayCommand<Window> CancelCommand { get; }

        /// <summary>Exposes the concrete profile instance for the dialog content presenter.</summary>
        public abstract object ProfileObject { get; }

        public string Error => null;

        public virtual string this[string columnName]
        {
            get
            {
                var propertyName = NormalizePropertyName(columnName);
                if (string.IsNullOrEmpty(propertyName) || !ShouldValidateField(propertyName))
                {
                    return null;
                }

                var value = GetFieldValue(propertyName);
                return TestProfileValidationRules.Validate(ProfileType, propertyName, value);
            }
        }

        public bool HasErrors => _validatedFields.Any(field => ShouldValidateField(field) && !string.IsNullOrEmpty(this[field]));

        protected virtual bool ShouldValidateField(string propertyName) => true;

        protected abstract object? GetFieldValue(string propertyName);

        protected void RefreshValidationState()
        {
            OnPropertyChanged(nameof(HasErrors));
            SaveCommand.NotifyCanExecuteChanged();
        }

        private static string NormalizePropertyName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            var separatorIndex = columnName.IndexOf('.');
            return separatorIndex >= 0 ? columnName[(separatorIndex + 1)..] : columnName;
        }

        private bool CanSave(Window? window)
        {
            if (HasErrors)
            {
                return false;
            }

            return window == null || !HasVisualErrors(window);
        }

        private static bool HasVisualErrors(DependencyObject root)
        {
            if (Validation.GetHasError(root))
            {
                return true;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject dependency && HasVisualErrors(dependency))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ExecuteSave(Window? window)
        {
            if (window is null)
            {
                return;
            }

            window.DialogResult = true;
            window.Close();
        }

        private static void ExecuteCancel(Window? window)
        {
            if (window is null)
            {
                return;
            }

            window.DialogResult = false;
            window.Close();
        }
    }

    /// <summary>Concrete view model implementation used for the individual profile editors.</summary>
    public sealed class TestProfileDetailViewModel<TProfile> : TestProfileDetailViewModel where TProfile : ObservableObject
    {
        private readonly Dictionary<string, PropertyInfo> _propertyLookup;

        public TestProfileDetailViewModel(TestProfileType profileType, TProfile profile)
            : base(profileType)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _propertyLookup = typeof(TProfile)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetMethod != null)
                .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

            Profile.PropertyChanged += OnProfilePropertyChanged;
            RefreshValidationState();
        }

        public TProfile Profile { get; }

        public override object ProfileObject => Profile;

        protected override object? GetFieldValue(string propertyName)
        {
            return _propertyLookup.TryGetValue(propertyName, out var property)
                ? property.GetValue(Profile)
                : null;
        }

        private void OnProfilePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || ShouldValidateField(e.PropertyName))
            {
                RefreshValidationState();
            }
        }
    }
}
