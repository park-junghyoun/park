using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class BoardVersionChangedMessage : ValueChangedMessage<string>
    {
        public BoardVersionChangedMessage(string version) : base(version) { }

        public string Version => Value;
    }
}
