using System.ComponentModel;
using CellManager.Configuration;

namespace CellManager.Models
{
    public partial class Cell : IDataErrorInfo
    {
        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName]
            => CellDetailValidation.GetError(this, columnName) ?? string.Empty;
    }
}
