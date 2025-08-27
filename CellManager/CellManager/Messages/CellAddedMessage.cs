using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class CellAddedMessage : ValueChangedMessage<Cell>
    {
        public CellAddedMessage(Cell addedCell) : base(addedCell) { }

        public Cell AddedCell => Value;
    }
}