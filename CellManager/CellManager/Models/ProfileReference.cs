using System;
using CellManager.Models.TestProfile;

namespace CellManager.Models
{
    /// <summary>
    ///     Lightweight projection of a test profile used for list selections and schedule references.
    /// </summary>
    public class ProfileReference : IEquatable<ProfileReference>
    {
        public int CellId { get; set; }
        public TestProfileType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Generates a unique identifier that accounts for the cell and profile type,
        /// preventing ID collisions across cells and tables while keeping values ordered.
        /// The identifier format is: CCCC T IIII (C=cell, T=type, I=profile id).
        /// </summary>
        public int UniqueId => (CellId * 1_000_000) + ((int)Type * 10_000) + Id;

        /// <summary>Formatted text used when listing profile choices to the operator.</summary>
        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        public override bool Equals(object? obj) => Equals(obj as ProfileReference);

        public bool Equals(ProfileReference? other) => other != null && UniqueId == other.UniqueId;

        public override int GetHashCode() => UniqueId.GetHashCode();
    }
}

