using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class ScheduleChangedMessage : ValueChangedMessage<Schedule?>
    {
        public ScheduleChangedMessage(Schedule? schedule) : base(schedule) { }

        public Schedule? Schedule => Value;
    }
}
