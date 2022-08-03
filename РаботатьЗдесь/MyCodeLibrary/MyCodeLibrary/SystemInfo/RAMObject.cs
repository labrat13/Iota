using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// RAM Object
    /// </summary>
    public class RAMObject
    {
        /// <summary>
        /// Total Installed RAM
        /// </summary>
        public String TotalInstalled { get; internal set; }

        public RAMObject()
        {
            this.TotalInstalled = String.Empty;
        }
    }
}