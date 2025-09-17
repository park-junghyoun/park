using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class SchedulesUpdatedMessage : ValueChangedMessage<int>
    {
        public SchedulesUpdatedMessage(int cellId) : base(cellId)
        {
        }
    }
}

