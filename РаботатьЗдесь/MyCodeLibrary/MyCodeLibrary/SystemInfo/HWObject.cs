using System;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Hardware Data Objects
    /// </summary>
    public class HWObject
    {
        /// <summary>
        /// System OEM
        /// </summary>
        public String SystemOEM { get; internal set; }
        /// <summary>
        /// Product Name
        /// </summary>
        public String ProductName { get; internal set; } 
        /// <summary>
        /// BIOS Object
        /// </summary>
        public BIOSObject BIOS { get; internal set; } 
        /// <summary>
        /// Network Object
        /// </summary>
        public NetworkObject Network { get; internal set; }
        /// <summary>
        /// Processor Object
        /// </summary>
        public ProcessorObject Processor { get; internal set; }
        /// <summary>
        /// RAM Object
        /// </summary>
        public RAMObject RAM { get; internal set; }
        /// <summary>
        /// Storage Object
        /// </summary>
        public StorageObject Storage { get; internal set; } 


        public HWObject()
        {
            this.BIOS = new BIOSObject();
            this.Network = new NetworkObject();
            this.Processor = new ProcessorObject();
            this.ProductName= String.Empty;
            this.RAM = new RAMObject();
            this.Storage= new StorageObject();
            this.SystemOEM = String.Empty;
        }
    }
}
