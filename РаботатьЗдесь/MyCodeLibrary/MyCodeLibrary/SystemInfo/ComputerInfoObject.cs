using System;
using System.IO;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// Creates an object that holds info about the computer.
    /// </summary>
    public class ComputerInfoObject
    {
        /// <summary>
        /// Constructor initializes values();
        /// </summary>
        public ComputerInfoObject()
        {
            Hardware = ReinitializeHardware();
            OS = ReinitalizeOS();
        }

        ///// <summary>
        ///// List of posible computers
        ///// </summary>
        //public enum ComputerList
        //{
        //    /// <summary>
        //    /// Localhost
        //    /// </summary>
        //    Localhost
        //}

        /// <summary>
        /// Returns information about the Computers hardware.
        /// </summary>
        public HWObject Hardware { get; set; }
        /// <summary>
        /// Returns information about the Computers operating system.
        /// </summary>
        public OSObject OS { get; set; }

        /// <summary>
        /// Initalizes the hardware class.
        /// </summary>
        /// <returns></returns>
        public static HWObject ReinitializeHardware()
        {
            String error = String.Empty;
            var Hardware = new HWObject
            {
                SystemOEM = SystemInfoProcessor.ManufacturerName,
                ProductName = SystemInfoProcessor.ProductName,
                #region BIOS
                BIOS = new BIOSObject
                {
                    Name = SystemInfoProcessor.BiosName,
                    ReleaseDate = SystemInfoProcessor.BiosReleaseDate,
                    Vendor = SystemInfoProcessor.BiosVendor,
                    Version = SystemInfoProcessor.BiosVersion
                },
                #endregion
                #region Network
                Network = new NetworkObject
                {
                    ConnectionStatus = CNetworkProcessor.ConnectionStatus,
                    InternalIPAddress = CNetworkProcessor.InternalIPAddress,
                    ExternalIPAddress = CNetworkProcessor.ExternalIPAddress(out error)
                },
                #endregion
                Processor = new ProcessorObject { Name = SystemInfoProcessor.ProcessorName, Cores = SystemInfoProcessor.ProcessorCount},
                RAM = new RAMObject { TotalInstalled = SystemInfoProcessor.GetTotalRam }
            };

            #region Storage
            var Storage = new StorageObject();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                String drivetype = String.Empty;
                bool activeDrive = false;
                if (drive.IsReady)
                {
                    if (drive.DriveType == DriveType.Fixed)
                    {
                        try
                        {
                            if (drive.TotalSize != 0.0 && drive.TotalFreeSpace != 0.0)
                            {
                                activeDrive = true; drivetype = "Fixed";
                            }
                        }
                        catch (Exception) { throw; }
                    }
                    if (drive.DriveType == DriveType.Removable)
                    {
                        try
                        {
                            if ((drive.TotalSize != 0.0) && (drive.TotalFreeSpace != 0.0))
                            {
                                activeDrive = true; drivetype = "Removable";
                            }
                        }
                        catch (Exception) { throw; }
                    }

                    if (activeDrive == true)
                    {
                        DriveObject newdrive = new DriveObject
                        {
                            Name = drive.Name,
                            Format = drive.DriveFormat,
                            Label = drive.VolumeLabel,
                            TotalSize = CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.TotalSize)),
                            TotalFree = CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.AvailableFreeSpace)),
                            DriveType = drivetype
                        };
                        Storage.InstalledDrives.Add(newdrive);
                        if (drive.Name.Trim() == SystemInfoProcessor.SystemDrivePath)
                        {
                            Storage.SystemDrive = newdrive;
                        }
                    }
                }
            }

            Hardware.Storage = Storage;
            #endregion

            return Hardware;
        }
        /// <summary>
        /// Initalizes the software class.
        /// </summary>
        /// <returns></returns>
        public static OSObject ReinitalizeOS()
        {
            return new OSObject
            {
                ComputerName = SystemInfoProcessor.ComputerNameActive,
                ComputerNamePending = SystemInfoProcessor.ComputerNamePending,
                DomainName = SystemInfoProcessor.CurrentDomainName,
                LoggedInUserName = SystemInfoProcessor.LoggedInUserName,
                RegisteredOrganization = SystemInfoProcessor.RegisteredOrganization,
                RegisteredOwner = SystemInfoProcessor.RegisteredOwner,
                InstallInfo = new InstallInfoObject
                {
                    ActivationStatus = SystemInfoProcessor.IsActivatedWMI,
                    Architecture = SystemInfoProcessor.ArchitectureString,
                    NameExpanded = SystemInfoProcessor.OsVersionStringLong,
                    Name = SystemInfoProcessor.OsVersionString,
                    ProductKey = SystemInfoProcessor.OsProductKey,
                    ServicePack = SystemInfoProcessor.OsServicePackString,
                    ServicePackNumber = SystemInfoProcessor.OsServicePackNumber,
                    Version = new VersionObject
                    {
                        Build = myOsVersionInfo.get_Build(),
                        Main = myOsVersionInfo.get_Main(),
                        Major = myOsVersionInfo.get_Major(),
                        Minor = myOsVersionInfo.get_Minor(),
                        Number = myOsVersionInfo.get_Number(),
                        Revision = myOsVersionInfo.get_Revision()
                    }
                }
            };
        }
    }
}
