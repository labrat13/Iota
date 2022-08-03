using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Network Object
    /// </summary>
    public class NetworkObject
    {
        /// <summary>
        /// Internal IP Address
        /// </summary>
        public String InternalIPAddress { get; internal set; }
        /// <summary>
        /// External IP Address
        /// </summary>
        public String ExternalIPAddress { get; internal set; }
        /// <summary>
        /// Internet Connection Status
        /// </summary>
        public Boolean ConnectionStatus { get; internal set; }

        public NetworkObject()
        {
            this.ConnectionStatus = false;
            this.ExternalIPAddress = String.Empty;
            this.InternalIPAddress = String.Empty;
        }
    }
}