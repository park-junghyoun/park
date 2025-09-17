using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Message issued when the active profile selection changes in the UI.
    /// </summary>
    public class ProfileChangedMessage : ValueChangedMessage<string>
    {
        public ProfileChangedMessage(string profile) : base(profile) { }

        /// <summary>Gets the identifier or name of the profile that was selected.</summary>
        public string Profile => Value;
    }
}
