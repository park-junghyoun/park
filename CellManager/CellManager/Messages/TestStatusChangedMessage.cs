using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Communicates a change in the run-time status of an executing test sequence.
    /// </summary>
    public class TestStatusChangedMessage : ValueChangedMessage<string>
    {
        public TestStatusChangedMessage(string status) : base(status) { }

        /// <summary>Gets the descriptive status string that should be displayed in the UI.</summary>
        public string Status => Value;
    }
}
