using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Broadcasts that a new <see cref="Cell"/> has been persisted so dependent view models can refresh.
    /// </summary>
    public class CellAddedMessage : ValueChangedMessage<Cell>
    {
        public CellAddedMessage(Cell addedCell) : base(addedCell) { }

        /// <summary>Convenience accessor for the newly created <see cref="Cell"/>.</summary>
        public Cell AddedCell => Value;
    }
}