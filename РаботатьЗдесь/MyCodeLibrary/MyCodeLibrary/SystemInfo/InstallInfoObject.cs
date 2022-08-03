using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Install Info Object
    /// </summary>
    public class InstallInfoObject
    {
        /// <summary>
        /// Activation Status
        /// </summary>
        public String ActivationStatus { get; internal set; }
        /// <summary>
        /// Architecture
        /// </summary>
        public String Architecture { get; internal set; }
        /// <summary>
        /// Display Version
        /// </summary>
        public String NameExpanded { get; internal set; }
        /// <summary>
        /// Name
        /// </summary>
        public String Name { get; internal set; }
        /// <summary>
        /// Product Key
        /// </summary>
        public String ProductKey { get; internal set; }
        /// <summary>
        /// Service Pack
        /// </summary>
        public String ServicePack { get; internal set; }
        /// <summary>
        /// Service Pack Number
        /// </summary>
        public int ServicePackNumber { get; internal set; }
        /// <summary>
        /// Version Object
        /// </summary>
        public VersionObject Version { get; internal set; }


        public InstallInfoObject()
        {
            this.ActivationStatus = String.Empty;
            this.Architecture = String.Empty;
            this.Name = String.Empty;
            this.NameExpanded = String.Empty;
            this.ProductKey = String.Empty;
            this.ServicePack = String.Empty;
            this.ServicePackNumber = 0;
            this.Version = new VersionObject();
        }
    }
}