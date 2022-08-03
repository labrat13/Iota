using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// BIOS Object
    /// </summary>
    public class BIOSObject
    {
             
        /// <summary>
        /// BIOS Name
        /// </summary>
        public String Name { get; internal set; }
        /// <summary>
        /// BIOS Release Date
        /// </summary>
        public String ReleaseDate { get; internal set; }
        /// <summary>
        /// BIOS Vendor
        /// </summary>
        public String Vendor { get; internal set; }
        /// <summary>
        /// BIOS Version
        /// </summary>
        public String Version { get; internal set; }

        public BIOSObject()
        {
            this.Name = String.Empty;
            this.ReleaseDate = String.Empty;
            this.Vendor = String.Empty;
            this.Version = String.Empty;
        }

    }

}