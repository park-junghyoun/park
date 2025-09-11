using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class TestStatusChangedMessage : ValueChangedMessage<string>
    {
        public TestStatusChangedMessage(string status) : base(status) { }

        public string Status => Value;
    }
}
