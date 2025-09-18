using System;
using System.Collections.Generic;
using CellManager.Models.TestProfile;

namespace CellManager.Configuration
{
    internal static class TestProfileValidationRules
    {
        public static string? Validate(TestProfileType profileType, string propertyName, object? value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            if (!TestProfileConstraintProvider.TryGetFieldConstraint(profileType, propertyName, out var constraint))
            {
                return null;
            }

            return constraint.CreateValidationError(value);
        }

        public static IReadOnlyList<string> GetFieldNames(TestProfileType profileType)
        {
            return TestProfileConstraintProvider.GetFieldNames(profileType);
        }
    }
}
