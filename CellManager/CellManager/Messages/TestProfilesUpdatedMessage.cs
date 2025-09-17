using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Indicates that the collection of test profiles associated with the specified cell identifier changed.
    /// </summary>
    public class TestProfilesUpdatedMessage : ValueChangedMessage<int>
    {
        public TestProfilesUpdatedMessage(int cellId) : base(cellId)
        {
        }
    }
}

