using CellManager.Models.TestProfile;

namespace CellManager.Models
{
    public class ProfileReference
    {
        public TestProfileType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayNameAndId => $"ID: {Id} - {Name}";
    }
}