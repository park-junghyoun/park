using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
#if CELL_MANAGER_WPF
using CellManager.Configuration;
#endif

namespace CellManager.Models.TestProfile
{
    internal static class TestProfileDataErrorInfoHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyLookupCache = new();
        private static readonly object CacheLock = new();

        public static string? GetError(TestProfileType profileType, object profile, string columnName)
        {
            if (profile is null)
            {
                return null;
            }

            var propertyName = NormalizeColumnName(columnName);
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            var value = GetPropertyValue(profile, propertyName);
            return GetValidationError(profileType, propertyName, value);
        }

#if CELL_MANAGER_WPF
        private static string? GetValidationError(TestProfileType profileType, string propertyName, object? value)
        {
            return TestProfileValidationRules.Validate(profileType, propertyName, value);
        }
#else
        private static string? GetValidationError(TestProfileType profileType, string propertyName, object? value)
        {
            return null;
        }
#endif

        private static string NormalizeColumnName(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return string.Empty;
            }

            var separatorIndex = columnName.LastIndexOf('.');
            return separatorIndex >= 0 ? columnName[(separatorIndex + 1)..] : columnName;
        }

        private static object? GetPropertyValue(object profile, string propertyName)
        {
            var type = profile.GetType();

            Dictionary<string, PropertyInfo> lookup;
            lock (CacheLock)
            {
                if (!PropertyLookupCache.TryGetValue(type, out lookup))
                {
                    lookup = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
                    foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (property.GetMethod != null)
                        {
                            lookup[property.Name] = property;
                        }
                    }

                    PropertyLookupCache[type] = lookup;
                }
            }

            return lookup.TryGetValue(propertyName, out var propertyInfo)
                ? propertyInfo.GetValue(profile)
                : null;
        }
    }

    public partial class ChargeProfile : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] =>
            TestProfileDataErrorInfoHelper.GetError(TestProfileType.Charge, this, columnName);
    }

    public partial class DischargeProfile : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] =>
            TestProfileDataErrorInfoHelper.GetError(TestProfileType.Discharge, this, columnName);
    }

    public partial class RestProfile : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] =>
            TestProfileDataErrorInfoHelper.GetError(TestProfileType.Rest, this, columnName);
    }

    public partial class OCVProfile : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] =>
            TestProfileDataErrorInfoHelper.GetError(TestProfileType.OCV, this, columnName);
    }

    public partial class ECMPulseProfile : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName] =>
            TestProfileDataErrorInfoHelper.GetError(TestProfileType.ECM, this, columnName);
    }
}
