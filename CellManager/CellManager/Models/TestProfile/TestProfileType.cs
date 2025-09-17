namespace CellManager.Models.TestProfile
{
    /// <summary>
    ///     Identifies the type of profile node so that the UI can render the appropriate editor.
    /// </summary>
    public enum TestProfileType
    {
        Charge,
        Discharge,
        Rest,
        OCV,
        ECM
    }
}