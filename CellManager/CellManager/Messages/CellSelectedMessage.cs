using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Broadcasts the <see cref="Cell"/> currently selected in the library list so other tabs can react.
    /// </summary>
    public class CellSelectedMessage : ValueChangedMessage<Cell>
    {
        public CellSelectedMessage(Cell selectedCell) : base(selectedCell) { }

        /// <summary>Gets the cell that the user highlighted in the UI.</summary>
        public Cell SelectedCell => Value;
    }
}