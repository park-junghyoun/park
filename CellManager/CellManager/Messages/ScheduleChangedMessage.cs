using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Raised whenever the active <see cref="Schedule"/> is replaced or cleared.
    /// </summary>
    public class ScheduleChangedMessage : ValueChangedMessage<Schedule?>
    {
        public ScheduleChangedMessage(Schedule? schedule) : base(schedule) { }

        /// <summary>Gets the updated schedule instance, if one exists.</summary>
        public Schedule? Schedule => Value;
    }
}
