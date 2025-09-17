using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Signals that the collection of schedules for a particular cell needs to be refreshed.
    /// </summary>
    public class SchedulesUpdatedMessage : ValueChangedMessage<int>
    {
        public SchedulesUpdatedMessage(int cellId) : base(cellId)
        {
        }
    }
}

