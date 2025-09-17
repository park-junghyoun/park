using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class TestProfilesUpdatedMessage : ValueChangedMessage<int>
    {
        public TestProfilesUpdatedMessage(int cellId) : base(cellId)
        {
        }
    }
}

