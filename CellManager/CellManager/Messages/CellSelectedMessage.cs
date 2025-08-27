using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    public class CellSelectedMessage : ValueChangedMessage<Cell>
    {
        public CellSelectedMessage(Cell selectedCell) : base(selectedCell) { }

        public Cell SelectedCell => Value;
    }
}