using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Message that transports the currently detected board firmware version.
    /// </summary>
    public class BoardVersionChangedMessage : ValueChangedMessage<string>
    {
        public BoardVersionChangedMessage(string version) : base(version) { }

        /// <summary>Gets the firmware or hardware revision string reported by the service.</summary>
        public string Version => Value;
    }
}
