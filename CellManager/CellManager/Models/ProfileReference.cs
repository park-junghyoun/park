using CellManager.Models.TestProfile;

namespace CellManager.Models
{
    public class ProfileReference
    {
        public TestProfileType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Generates a unique identifier that accounts for the profile type.
        /// This prevents ID collisions between different profile tables.
        /// </summary>
        public int UniqueId => ((int)Type * 1_000_000) + Id;

        public string DisplayNameAndId => $"ID: {UniqueId} - {Name}";
    }
}
