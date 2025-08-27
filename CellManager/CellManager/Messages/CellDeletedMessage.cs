using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class CellDeletedMessage : ValueChangedMessage<Cell>
    {
        public CellDeletedMessage(Cell deletedCell) : base(deletedCell) { }

        public Cell DeletedCell => Value;
    }
}