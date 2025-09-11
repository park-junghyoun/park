using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class ProfileChangedMessage : ValueChangedMessage<string>
    {
        public ProfileChangedMessage(string profile) : base(profile) { }

        public string Profile => Value;
    }
}
