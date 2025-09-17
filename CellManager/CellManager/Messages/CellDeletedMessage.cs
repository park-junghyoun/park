using CellManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CellManager.Messages
{
    /// <summary>
    ///     Notification published when a cell entity has been removed from the repository.
    /// </summary>
    public class CellDeletedMessage : ValueChangedMessage<Cell>
    {
        public CellDeletedMessage(Cell deletedCell) : base(deletedCell) { }

        /// <summary>Convenience accessor for the deleted <see cref="Cell"/> instance.</summary>
        public Cell DeletedCell => Value;
    }
}