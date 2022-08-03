namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Storage Object
    /// </summary>
    public class StorageObject
    {
        /// <summary>
        /// List of installed Drives
        /// </summary>
        public System.Collections.Generic.List<DriveObject> InstalledDrives { get; internal set; }

        /// <summary>
        /// System Boot Drive
        /// </summary>
        public DriveObject SystemDrive { get; internal set; }

        public StorageObject()
        {
            this.InstalledDrives  = new System.Collections.Generic.List<DriveObject>();
            this.SystemDrive = null;
        }
    }
}