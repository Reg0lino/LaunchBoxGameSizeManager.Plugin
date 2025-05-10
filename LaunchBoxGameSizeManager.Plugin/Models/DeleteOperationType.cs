namespace LaunchBoxGameSizeManager.Models
{
    public enum DeleteOperationType
    {
        RemoveEntryOnly,    // Remove from LaunchBox, keep files
        DeleteFilesOnly,    // Delete files, keep LaunchBox entry (requires audit afterwards)
        DeleteAll           // Delete files, media, and LaunchBox entry
    }
}