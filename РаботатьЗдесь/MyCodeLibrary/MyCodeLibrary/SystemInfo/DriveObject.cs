using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Drive Object
    /// </summary>
    public class DriveObject
    {
        /// <summary>
        /// Drive Name
        /// </summary>
        public String Name { get; internal set; }
        /// <summary>
        /// Drive Format
        /// </summary>
        public String Format { get; internal set; }
        /// <summary>
        /// Drive Label
        /// </summary>
        public String Label { get; internal set; }
        /// <summary>
        /// Drive Type
        /// </summary>
        public String DriveType { get; internal set; }
        /// <summary>
        /// Drive Total Size
        /// </summary>
        public String TotalSize { get; internal set; }
        /// <summary>
        /// Drive Total Free Space
        /// </summary>
        public String TotalFree { get; internal set; }

        public DriveObject()
        {
            this.DriveType = String.Empty;
            this.Format = String.Empty;
            this.Label= String.Empty;
            this.Name= String.Empty;
            this.TotalFree= String.Empty;
            this.TotalSize= String.Empty;
        }
    }
}