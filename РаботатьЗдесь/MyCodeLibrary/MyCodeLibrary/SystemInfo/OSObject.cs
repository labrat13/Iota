using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// OS Object
    /// </summary>
    public class OSObject
    {
        /// <summary>
        /// Computer Name
        /// </summary>
        public String ComputerName { get; internal set; }
        /// <summary>
        /// Computer Name Pending
        /// </summary>
        public String ComputerNamePending { get; internal set; }
        /// <summary>
        /// Install Info Object
        /// </summary>
        public InstallInfoObject InstallInfo { get; internal set; }
        /// <summary>
        /// Registered Organization Name
        /// </summary>
        public String RegisteredOrganization { get; internal set; }
        /// <summary>
        /// Registered Owner Name
        /// </summary>
        public String RegisteredOwner { get; internal set; }
        /// <summary>
        /// Logged In Username
        /// </summary>
        public String LoggedInUserName { get; internal set; }
        /// <summary>
        /// Currently Joined Domain Name
        /// </summary>
        public String DomainName { get; internal set; }

        public OSObject()
        {
            this.ComputerName = String.Empty;
            this.ComputerNamePending = String.Empty;
            this.DomainName = String.Empty;
            this.InstallInfo = new InstallInfoObject();
            this.LoggedInUserName = String.Empty;
            this.RegisteredOrganization = String.Empty;
            this.RegisteredOwner = String.Empty;
        }
    }
}