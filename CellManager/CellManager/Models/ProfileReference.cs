using System;
using CellManager.Models.TestProfile;

namespace CellManager.Models
{
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

        public string DisplayNameAndId => $"ID: {Id} - {Name}";

        public override bool Equals(object? obj) => Equals(obj as ProfileReference);

        public bool Equals(ProfileReference? other) => other != null && UniqueId == other.UniqueId;

        public override int GetHashCode() => UniqueId.GetHashCode();
    }
}

