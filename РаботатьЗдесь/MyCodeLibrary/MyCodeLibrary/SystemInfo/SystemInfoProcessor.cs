using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;

namespace MyCodeLibrary.SystemInfo
{
    /* Тут в этот класс сведены все статические функции определения параметров системы,
     *  для их использования по-одиночке.
     * Если нужно получить кешированную версию со всеми параметрами и удобно разбитыми на разделы, 
     *  то нужно использовать объект класса MyCodeLibrary.SystemInfo.ComputerInfoObject.
     *  Хотя и она тоже использует функции SystemInfoProcessor поодиночке, это не оптимально.
     * TODO: Надо бы написать более цельный процесс инициализации для объектов ComputerInfoObject.
     *  но сейчас это не критично, отложить на более поздний срок.
     */


    /// <summary>
    /// Gets the different infos provides by the operating system.
    /// </summary>
    public static class SystemInfoProcessor
    {

        /// <summary>
        /// Return a full version String, es.: "Windows XP Service Pack 2 (32 Bit)"
        /// </summary>
        /// <returns>A String representing a fully displayable version</returns>
        public static String OsVersionStringLong
        {
            get
            {
                String ServicePack = String.Empty;
                String TextFormat = "{0} {1} {2} ({3} Bit)";
                if (IsWin8OrLater)
                {
                    ServicePack = " - " + myOsVersionInfo.get_Build().ToString(CultureInfo.CurrentCulture);
                    return String.Format(TextFormat, SystemInfoProcessor.OsVersionString, SystemInfoProcessor.OsEditionString, ServicePack, SystemInfoProcessor.ArchitectureNumber);
                }
                var SPString = ServicePack;
                ServicePack = " SP" + SPString.Substring(SPString.Length - 1);
                return String.Format(TextFormat, SystemInfoProcessor.OsVersionString, SystemInfoProcessor.OsEditionString, ServicePack, SystemInfoProcessor.ArchitectureNumber);
            }
        }

        /// <summary>
        /// Return a full version String, es.: "Windows XP Service Pack 2 (32 Bit)"
        /// </summary>
        /// <returns>A String representing a fully displayable version</returns>
        public static String OsVersionStringLong2
        {
            get
            {
                String key = "Software\\\\Microsoft\\\\Windows NT\\\\CurrentVersion";
                String value = "ProductName";
                String name = CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);

                String ServicePack = String.Empty;
                String TextFormat = "{0} {1} ({2} Bit)";
                if (SystemInfoProcessor.IsWin8OrLater)
                {
                    ServicePack = " - " + myOsVersionInfo.get_Build().ToString(CultureInfo.CurrentCulture);
                    return String.Format(TextFormat, name, ServicePack, SystemInfoProcessor.ArchitectureNumber);
                }
                String SPString = ServicePack;
                ServicePack = " SP" + SPString.Substring(SPString.Length - 1);
                return String.Format(TextFormat, name, ServicePack, SystemInfoProcessor.ArchitectureNumber);
            }
        }

        /// <summary>
        /// Returns the name of the operating system running on this Computer.
        /// </summary>
        public static OsType OsVersionType
        {
            get
            {
                switch (myOsVersionInfo.get_Number())
                {
                    case 51: return OsType.WindowsXP;
                    case 52: return IsServer
                        ? (CNativeMethodProcessor.GetSystemMetrics((int)OtherConsts.SMServerR2)
                            ? OsType.Windows2003R2
                            : OsType.Windows2003)
                        : OsType.WindowsXP64;
                    case 60: return IsServer ? OsType.Windows2008 : OsType.WindowsVista;
                    case 61: return IsServer ? OsType.Windows2008R2 : OsType.Windows7;
                    case 62: return IsServer ? OsType.Windows2012 : OsType.Windows8;
                    case 63: return IsServer ? OsType.Windows2012R2 : OsType.Windows81;
                    case 64: return IsServer ? OsType.Windows2016 : OsType.Windows10;
                }
                return OsType.Windows2000AndPrevious;
            }
        }

        /// <summary>
        /// Returns the name of the operating system running on this Computer.
        /// </summary>
        /// <returns>A String containing the the operating system name.</returns>
        public static String OsVersionString
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        {
                            switch (myOsVersionInfo.get_Major())
                            {
                                case 5:
                                    {
                                        switch (myOsVersionInfo.get_Minor())
                                        {
                                            case 1: return "Windows XP";
                                            case 2:
                                                return IsServer
                                                    ? (CNativeMethodProcessor.GetSystemMetrics((int)OtherConsts.SMServerR2)
                                                        ? "Windows Server 2003 R2"
                                                        : "Windows Server 2003")
                                                    : "WindowsXP x64";
                                        }
                                        break;
                                    }
                                case 6:
                                    {
                                        switch (myOsVersionInfo.get_Minor())
                                        {
                                            case 0: return IsServer ? "Windows 2008" : "Windows Vista";
                                            case 1: return IsServer ? "Windows 2008 R2" : "Windows 7";
                                            case 2: return IsServer ? "Windows 2012" : "Windows 8";
                                            case 3: return IsServer ? "Windows 2012 R2" : "Windows 8.1";
                                        }
                                        break;
                                    }
                                case 10:
                                    {
                                        switch (myOsVersionInfo.get_Minor())
                                        {
                                            case 0: return IsServer ? "Windows 2016" : "Windows 10";
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
                return "UNKNOWN";
            }
        }

        /// <summary>
        /// Gets the current Computer name.
        /// </summary>
        public static String ComputerNameActive
        {
            get
            {
                String key = "System\\ControlSet001\\Control\\ComputerName\\ActiveComputerName";
                String value = "ComputerNameActive";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Gets the pending Computer name that it will update to on reboot.
        /// </summary>
        public static String ComputerNamePending
        {
            get
            {
                String key = "System\\ControlSet001\\Control\\ComputerName\\ActiveComputerName";
                String value = "ComputerNameActive";
                String text = CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
                return text.Equals(ComputerNameActive) ? "N/A" : text;
            }
        }

        /// <summary>
        /// Identifies if OS is a 64 Bit OS
        /// </summary>
        /// <returns>True if OS is a 64 Bit OS</returns>
        ///
        public static Boolean Is64BitOS { get { return ((IntPtr.Size == 8) || ((IntPtr.Size == 4) && Is32BitProcessOn64BitProcessor)); } }

        /// <summary>
        /// Identifies if OS is a Windows Server OS
        /// </summary>
        /// <returns>True if OS is a Windows Server OS</returns>
        ///
        public static Boolean IsServer { get { return ((ProductType)SystemInfoProcessor.OsProductType != ProductType.NTWorkstation); } }

        /// <summary>
        /// Identifies if OS is a Windows Domain Controller
        /// </summary>
        /// <returns>True if OS is a Windows Server OS</returns>
        ///
        public static Boolean IsDomainController { get { return ((ProductType)SystemInfoProcessor.OsProductType == ProductType.NTDomainController); } }

        /// <summary>
        /// Identifies Arch of running process
        /// </summary>
        /// <returns>True if process is 32bit running on a 64bit machine</returns>
        ///
        public static Boolean Is32BitProcessOn64BitProcessor
        {
            get
            {
                if (IntPtr.Size == 8) return true; // 64-bit programs run only on Win64
                // 32-bit programs run on both 32-bit and 64-bit Windows
                // Detect whether the current process is a 32-bit process running on a 64-bit system.
                Boolean flag;
                return (Win32MethodExists && CNativeMethodProcessor.IsWow64Process(CNativeMethodProcessor.GetCurrentProcess(), out flag)) && flag;
            }
        }

        /// <summary>
        /// The function determines whether a method exists in the export
        /// table of a certain module.
        /// </summary>
        internal static Boolean Win32MethodExists
        {
            get
            {
                var moduleHandle = CNativeMethodProcessor.GetModuleHandle("kernel32.dll");
                if (moduleHandle == IntPtr.Zero) return false;
                return CNativeMethodProcessor.GetProcAddress(moduleHandle, "IsWow64Process") != IntPtr.Zero;
            }
        }

        //TODO: следует ли эти группы функций превратить в подклассы внутри SystemInfoProcessor?

        #region VersionLaterChecks
        //TODO: перенести эти функции в объект myOsVersionInfo
        /// <summary>
        /// Return if running on XP or later
        /// </summary>
        /// <returns>true means XP or later</returns>
        /// <returns>false means 2000 or previous</returns>
        public static Boolean IsXPOrLater { get { return (myOsVersionInfo.get_Number() >= 51); } }

        /// <summary>
        /// Return if running on XP 64 or later
        /// </summary>
        /// <returns>true means XP 64 or later</returns>
        /// <returns>false means XP or previous</returns>
        public static Boolean IsXP64OrLater { get { return (myOsVersionInfo.get_Number() >= 52); } }

        /// <summary>
        /// Return if running on Vista or later
        /// </summary>
        /// <returns>true means Vista or later</returns>
        /// <returns>false means Xp or previous</returns>
        public static Boolean IsVistaOrLater { get { return (myOsVersionInfo.get_Number() >= 60); } }

        /// <summary>
        /// Return if running on Windows7 or later
        /// </summary>
        /// <returns>true means Windows7 or later</returns>
        /// <returns>false means Vista or previous</returns>
        public static Boolean IsWin7OrLater { get { return (myOsVersionInfo.get_Number() >= 61); } }

        /// <summary>
        /// Return if running on Windows8 or later
        /// </summary>
        /// <returns>true means Windows8 or later</returns>
        /// <returns>false means Win7 or previous</returns>
        public static Boolean IsWin8OrLater { get { return (myOsVersionInfo.get_Number() >= 62); } }

        /// <summary>
        /// Return if running on Windows8.1 or later
        /// </summary>
        /// <returns>true means Windows8.1 or later</returns>
        /// <returns>false means Win8 or previous</returns>
        public static Boolean IsWin81OrLater { get { return (myOsVersionInfo.get_Number() >= 63); } }

        /// <summary>
        /// Return if running on Windows10 or later
        /// </summary>
        /// <returns>true means Windows10 or later</returns>
        /// <returns>false means Win10 or previous</returns>
        public static Boolean IsWin10OrLater { get { return (myOsVersionInfo.get_Number() >= 100); } }

        #endregion 

        #region isActivated
        /// <summary>
        /// Checks If Windows Is Activated. Uses the newer WMI.
        /// </summary>
        /// <returns>Licensed If Genuinely Activated</returns>
        public static String IsActivatedWMI
        {
            get
            {
                String str = String.Empty;
                try
                {
                    const String ComputerName = "localhost";
                    var Scope = new System.Management.ManagementScope(String.Format(CultureInfo.CurrentCulture, @"\\{0}\root\CIMV2", ComputerName), null);

                    Scope.Connect();
                    var Query = new System.Management.ObjectQuery("SELECT * FROM SoftwareLicensingProduct Where PartialProductKey <> null AND ApplicationId='55c92734-d682-4d71-983e-d6ec3f16059f' AND LicenseIsAddon=False");
                    using (var Searcher = new System.Management.ManagementObjectSearcher(Scope, Query))
                    {
                        foreach (var WmiObject in Searcher.Get())
                        {
                            switch ((uint)WmiObject["LicenseStatus"])
                            {
                                case 0:
                                    str = "Unlicensed";
                                    break;

                                case 1:
                                    str = "Licensed";
                                    break;

                                case 2:
                                    str = "Out-Of-Box Grace";
                                    break;

                                case 3:
                                    str = "Out-Of-Tolerance Grace";
                                    break;

                                case 4:
                                    str = "Non Genuine Grace";
                                    break;

                                case 5:
                                    str = "Notification";
                                    break;

                                case 6:
                                    str = "Extended Grace";
                                    break;

                                default:
                                    str = "Unknown License Status";
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    str = "Unknown License Status";
                }
                return str;
            }
        }

        /// <summary>
        /// Checks If Windows Is Activated. Uses the older Software Licensing Manager Script.
        /// </summary>
        /// <returns>Licensed If Genuinely Activated</returns>
        ///
        public static String IsActivatedSLMGR
        {
            //Довольно сложная процедура!
            //Тут запускается программа и vbs-скрипт из командной строки и в ее выводе ищется строка по ключевой фразе и выводится ее содержание.
            //Но само окно, вероятно, не показывается на экране.
            get
            {
                String strActivationStatus = String.Empty;
                CProcessProcessor.Output results = CProcessProcessor.Run(@"cscript C:\Windows\System32\Slmgr.vbs /dli", true);

                foreach (String line in results.Result)
                {
                    if (line.Contains("License Status: ")) strActivationStatus = line.Remove(0, 16);
                }

                return strActivationStatus;
            }
        }
        #endregion

        #region Architecture
        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        public static String ArchitectureString
        {
            get { return SystemInfoProcessor.Is64BitOS ? "64 bit" : "32 bit"; }
        }

        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        public static int ArchitectureNumber
        {
            get { return SystemInfoProcessor.Is64BitOS ? 64 : 32; }
        }

        #endregion 
        
        #region BIOS information
        /// <summary>
        /// Returns the full name of the system BIOS stored in the registry.
        /// </summary>
        public static String BiosName
        {
            get { return String.Format("{0} {1}", BiosVendor, BiosVersion); }
        }

        /// <summary>
        /// Returns the system BIOS release date stored in the registry.
        /// </summary>
        public static String BiosReleaseDate
        {
            get
            {
                const String key = @"HARDWARE\DESCRIPTION\System\BIOS";
                const String value = "BIOSReleaseDate";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Returns the system BIOS version stored in the registry.
        /// </summary>
        public static String BiosVersion
        {
            get
            {
                const String key = "HARDWARE\\DESCRIPTION\\System\\BIOS";
                const String value = "BIOSVersion";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Returns the system BIOS vendor name stored in the registry.
        /// </summary>
        public static String BiosVendor
        {
            get
            {
                const String key = "HARDWARE\\DESCRIPTION\\System\\BIOS";
                const String value = "BIOSVendor";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }
        #endregion

        #region Processor functions
        /// <summary>
        /// Returns the system processor name that is stored in the registry.
        /// </summary>
        public static String ProcessorName
        {
            get
            {
                const String key = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0";
                const String value = "ProcessorNameString";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Returns the number of cores available on the system processor.
        /// </summary>
        public static int ProcessorCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }
        #endregion
        #region Manufacturer information

        /// <summary>
        /// Returns the system manufacturer name that is stored in the registry.
        /// </summary>
        public static String ManufacturerName
        {
            get
            {
                var key = @"HARDWARE\DESCRIPTION\System\BIOS";
                var value = "SystemManufacturer";
                var text = CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
                if (String.IsNullOrEmpty(text))
                {
                    key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInFormation";
                    value = "Manufacturer";
                    return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
                }
                return text;
            }
        }

        /// <summary>
        /// Returns the system product name that is stored in the registry.
        /// </summary>
        public static String ProductName
        {
            get
            {
                var key = "HARDWARE\\DESCRIPTION\\System\\BIOS";
                var value = "SystemProductName";
                var text = CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
                if (String.IsNullOrEmpty(text))
                {
                    key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInFormation";
                    value = "Model";
                    return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
                }
                return text;
            }
        }
        
        #endregion
        #region RAM information

        /// <summary>
        /// Returns the total ram installed on the Computer.
        /// </summary>
        public static String GetTotalRam
        {
            get
            {
                try
                {
                    long installedMemory = 0;
                    CNativeMethodProcessor.GetPhysicallyInstalledSystemMemory(out installedMemory);
                    return CNumberProcessor.ConvertKilobytes((double)installedMemory);
                }
                catch (NullReferenceException)
                {
                    return String.Empty;
                }
            }
        }
        #endregion

        #region Storages information

        /// <summary>
        /// Returns list of installed drives and their information
        /// </summary>
        public static List<DriveObject> InstalledDrives
        {
            get
            {
                List<DriveObject> Drives = new List<DriveObject>();
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    var drivetype = String.Empty;
                    var ActiveDrive = false;
                    if (drive.IsReady)
                    {
                        if (drive.DriveType == DriveType.Fixed)
                        {
                            try
                            {
                                if (drive.TotalSize != 0.0 && drive.TotalFreeSpace != 0.0)
                                {
                                    ActiveDrive = true; drivetype = "Fixed";
                                }
                            }
                            catch (Exception) { throw; }
                        }
                        if (drive.DriveType == DriveType.Removable)
                        {
                            try
                            {
                                if (drive.TotalSize != 0.0 && drive.TotalFreeSpace != 0.0)
                                {
                                    ActiveDrive = true; drivetype = "Removable";
                                }
                            }
                            catch (Exception) { throw; }
                        }

                        if (ActiveDrive)
                        {
                            var newdrive = new DriveObject
                            {
                                Name = drive.Name,
                                Format = drive.DriveFormat,
                                Label = drive.VolumeLabel,
                                TotalSize = CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.TotalSize)),
                                TotalFree = CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.AvailableFreeSpace)),
                                DriveType = drivetype
                            };
                            Drives.Add(newdrive);
                            if (drive.Name.Trim() == SystemDrivePath) SystemDrive = newdrive;
                        }
                    }
                }
                return Drives;
            }
        }

        /// <summary>
        /// Returns information about the drive Windows is installed on.
        /// </summary>
        public static DriveObject SystemDrive { get; internal set; }

        /// <summary>
        /// NT-
        /// </summary>
        public static String SystemDrivePath
        {
            get 
            { 
                return Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
            }
        }
        /// <summary>
        /// Returns the drive size of the drive Windows is installed on.
        /// </summary>
        public static String SystemDriveSizeString
        {
            get
            {
                try
                {
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        return drive.IsReady && drive.Name == SystemDrivePath ? CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.TotalSize)) : String.Empty;
                    }
                    return String.Empty;
                }
                catch (NullReferenceException)
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Returns the free space of drive of the drive Windows is installed on.
        /// </summary>
        public static String SystemDriveFreeSpaceString
        {
            get
            {
                try
                {
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        return drive.IsReady && drive.Name == SystemDrivePath ? CNumberProcessor.ConvertBytes(Convert.ToDouble(drive.TotalFreeSpace)) : String.Empty;
                    }
                    return String.Empty;
                }
                catch (NullReferenceException)
                {
                    return String.Empty;
                }
            }
        }

        #endregion

        #region User info - Gets info about the currently logged in user account.

        /// <summary>
        /// Gets the current Registered Organization.
        /// </summary>
        public static String RegisteredOrganization
        {
            get
            {
                const String key = "Software\\Microsoft\\Windows NT\\CurrentVersion";
                const String value = "RegisteredOrganization";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Gets the current Registered Owner.
        /// </summary>
        public static String RegisteredOwner
        {
            get
            {
                const String key = "Software\\Microsoft\\Windows NT\\CurrentVersion";
                const String value = "RegisteredOwner";
                return CRegistryProcessor.getStringValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);
            }
        }

        /// <summary>
        /// Gets the user name of the person who is currently logged on to the Windows operating system.
        /// </summary>
        public static String LoggedInUserName
        {
            get { return Environment.UserName; }
        }

        /// <summary>
        /// Gets the network domain name associated with the current user.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">The operating system does not support retrieving the network domain name.</exception>
        /// <exception cref="InvalidOperationException">The network domain name cannot be retrieved.</exception>
        public static String CurrentDomainName
        {
            get { return Environment.UserDomainName; }
        }
        
        #endregion

#region Product Type - Gets the product type of the operating system running on this Computer.
        /// <summary>
        /// NT-Returns the product type of the operating system running on this Computer.
        /// </summary>
        /// <returns>A String containing the the operating system product type.</returns>
        public static String OsEditionString
        {
            get
            {
                switch (myOsVersionInfo.get_Major())
                {
                    case 5:
                        return editionGetVersion5();

                    case 6:
                    case 10:
                        return editionGetVersion6More();
                }
                return String.Empty;
            }
        }

        /// <summary>
        /// NT-Returns the product type from Windows 2000 to XP and Server 2000 to 2003
        /// </summary>
        /// <returns></returns>
        private static String editionGetVersion5()
        {

            var osVersionInfo = new CNativeMethodProcessor.OSVERSIONINFOEX
            {
                dwOSVersionInfoSize = Marshal.SizeOf(typeof(CNativeMethodProcessor.OSVERSIONINFOEX))
            };
            if (!CNativeMethodProcessor.GetVersionEx(ref osVersionInfo)) return String.Empty;

            CExceptionProcessor.ExceptionIfNull(osVersionInfo, "osVersionInfo Cannot Be Null!", "osVersionInfo");

            var Mask = (VERSuite)osVersionInfo.wSuiteMask;

            if (CNativeMethodProcessor.GetSystemMetrics((int)OtherConsts.SMMediaCenter)) return " Media Center";
            if (CNativeMethodProcessor.GetSystemMetrics((int)OtherConsts.SMTabletPC)) return " Tablet PC";
            if (SystemInfoProcessor.IsServer)
            {
                if (myOsVersionInfo.get_Minor() == 0)
                {
                    if ((Mask & VERSuite.Datacenter) == VERSuite.Datacenter)
                    {
                        // Windows 2000 Datacenter Server
                        return " Datacenter Server";
                    }
                    if ((Mask & VERSuite.Enterprise) == VERSuite.Enterprise)
                    {
                        // Windows 2000 Advanced Server
                        return " Advanced Server";
                    }
                    // Windows 2000 Server
                    return " Server";
                }
                if (myOsVersionInfo.get_Minor() == 2)
                {
                    if ((Mask & VERSuite.Datacenter) == VERSuite.Datacenter)
                    {
                        // Windows Server 2003 Datacenter Edition
                        return " Datacenter Edition";
                    }
                    if ((Mask & VERSuite.Enterprise) == VERSuite.Enterprise)
                    {
                        // Windows Server 2003 Enterprise Edition
                        return " Enterprise Edition";
                    }
                    if ((Mask & VERSuite.StorageServer) == VERSuite.StorageServer)
                    {
                        // Windows Server 2003 Storage Edition
                        return " Storage Edition";
                    }
                    if ((Mask & VERSuite.ComputeServer) == VERSuite.ComputeServer)
                    {
                        // Windows Server 2003 Compute Cluster Edition
                        return " Compute Cluster Edition";
                    }
                    if ((Mask & VERSuite.Blade) == VERSuite.Blade)
                    {
                        // Windows Server 2003 Web Edition
                        return " Web Edition";
                    }
                    // Windows Server 2003 Standard Edition
                    return " Standard Edition";
                }
            }
            else
            {
                if ((Mask & VERSuite.EmbeddedNT) == VERSuite.EmbeddedNT)
                {
                    //Windows XP Embedded
                    return " Embedded";
                }
                // Windows XP / Windows 2000 Professional
                return (Mask & VERSuite.Personal) == VERSuite.Personal ? " Home" : " Professional";
            }
            return String.Empty;
        }

        /// <summary>
        /// NT-Returns the product type from Windows Vista to 10 and Server 2008 to 2016
        /// </summary>
        /// <returns></returns>
        private static String editionGetVersion6More()
        {
            switch ((ProductEditionEnum)getProductInfo())
            {
                case ProductEditionEnum.Ultimate:
                case ProductEditionEnum.UltimateE:
                case ProductEditionEnum.UltimateN:
                    return "Ultimate";

                case ProductEditionEnum.Professional:
                case ProductEditionEnum.ProfessionalE:
                case ProductEditionEnum.ProfessionalN:
                    return "Professional";

                case ProductEditionEnum.HomePremium:
                case ProductEditionEnum.HomePremiumE:
                case ProductEditionEnum.HomePremiumN:
                    return "Home Premium";

                case ProductEditionEnum.HomeBasic:
                case ProductEditionEnum.HomeBasicE:
                case ProductEditionEnum.HomeBasicN:
                    return "Home Basic";

                case ProductEditionEnum.Enterprise:
                case ProductEditionEnum.EnterpriseE:
                case ProductEditionEnum.EnterpriseN:
                case ProductEditionEnum.EnterpriseServerV:
                    return "Enterprise";

                case ProductEditionEnum.Business:
                case ProductEditionEnum.BusinessN:
                    return "Business";

                case ProductEditionEnum.Starter:
                case ProductEditionEnum.StarterE:
                case ProductEditionEnum.StarterN:
                    return "Starter";

                case ProductEditionEnum.ClusterServer:
                    return "Cluster Server";

                case ProductEditionEnum.DatacenterServer:
                case ProductEditionEnum.DatacenterServerV:
                    return "Datacenter";

                case ProductEditionEnum.DatacenterServerCore:
                case ProductEditionEnum.DatacenterServerCoreV:
                    return "Datacenter (Core installation)";

                case ProductEditionEnum.EnterpriseServer:
                    return "Enterprise Server";

                case ProductEditionEnum.EnterpriseServerCore:
                case ProductEditionEnum.EnterpriseServerCoreV:
                    return "Enterprise (Core installation)";

                case ProductEditionEnum.EnterpriseServerIA64:
                    return "Enterprise For Itanium-based Systems";

                case ProductEditionEnum.SmallBusinessServer:
                    return "Small Business Server";

                //case SmallBusinessServerPremium:
                //  return "Small Business Server Premium Edition";

                case ProductEditionEnum.ServerForSmallBusiness:
                case ProductEditionEnum.ServerForSmallBusinessV:
                    return "Windows Essential Server Solutions";

                case ProductEditionEnum.StandardServer:
                case ProductEditionEnum.StandardServerV:
                    return "Standard";

                case ProductEditionEnum.StandardServerCore:
                case ProductEditionEnum.StandardServerCoreV:
                    return "Standard (Core installation)";

                case ProductEditionEnum.WebServer:
                case ProductEditionEnum.WebServerCore:
                    return "Web Server";

                case ProductEditionEnum.MediumBusinessServerManagement:
                case ProductEditionEnum.MediumBusinessServerMessaging:
                case ProductEditionEnum.MediumBusinessServerSecurity:
                    return "Windows Essential Business Server";

                case ProductEditionEnum.StorageEnterpriseServer:
                case ProductEditionEnum.StorageExpressServer:
                case ProductEditionEnum.StorageStandardServer:
                case ProductEditionEnum.StorageWorkgroupServer:
                    return "Storage Server";
            }
            return String.Empty;
        }

        /// <summary>
        /// NT-Gets the product type of the operating system running on this Computer.
        /// </summary>
        public static byte OsProductType
        {
            get
            {
                var osVersionInfo = new CNativeMethodProcessor.OSVERSIONINFOEX
                {
                    dwOSVersionInfoSize = Marshal.SizeOf(typeof(CNativeMethodProcessor.OSVERSIONINFOEX))
                };
                if (!CNativeMethodProcessor.GetVersionEx(ref osVersionInfo))
                    return (int)(MyCodeLibrary.SystemInfo.ProductType.Undefined);//? наобум назначен енум
                return osVersionInfo.wProductType;
            }
        }
        /// <summary>
        /// NR-
        /// </summary>
        /// <returns></returns>
        private static int getProductInfo()
        {
            return CNativeMethodProcessor.getProductInfo(myOsVersionInfo.get_Major(), myOsVersionInfo.get_Minor());
        }
    
#endregion


            #region Product key  - Gets And Decrypts The Current Product Key From The Registry
        /// <summary>
        /// Gets And Decrypts The Current Product Key From The Registry
        /// </summary>
        /// <returns>Returns Product Key As A String</returns>
        ///
        public static String OsProductKey
        {
            get
            {
                byte[] digitalProductId = null;
                const String key = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
                const String value = "DigitalProductId";
                digitalProductId = CRegistryProcessor.getByteValue(CRegistryProcessor.HKEY.LOCAL_MACHINE, key, value);

                if (digitalProductId == null) { return "Cannot Retrieve Product Key."; }

                return SystemInfoProcessor.IsWin8OrLater ? DecodeOsProductKeyWin8AndUp(digitalProductId) : DecodeOsProductKeyWin7AndBelow(digitalProductId);
            }
        }

        /// <summary>
        /// Returns the decoded product key from the provided byte array. Works with Windows 7 and below.
        /// </summary>
        /// <param name="digitalProductId"></param>
        public static String DecodeOsProductKeyWin7AndBelow(byte[] digitalProductId)
        {
            CExceptionProcessor.ExceptionIfNull(digitalProductId, "The specified digitalProductId cannot be null!", "digitalProductId");
            // Length of decoded product key
            const int decodeKeyLength = 29;
            // Length of decoded product key in byte-Form.
            // Each byte represents 2 chars.
            const int decodeStringLength = 15;
            // Offset of first byte of encoded product key in
            //  'DigitalProductIdxxx" REGBINARY value. Offset = 34H.
            const int keyStartIndex = 52;
            // Offset of last byte of encoded product key in
            //  'DigitalProductIdxxx" REGBINARY value. Offset = 43H.
            const int keyEndIndex = keyStartIndex + decodeStringLength;
            // Possible alpha-numeric characters in product key.
            const string digits = "BCDFGHJKMPQRTVWXY2346789";
            // Array of containing the decoded product key.
            var decodedChars = new char[decodeKeyLength];
            // Extract byte 52 to 67 inclusive.
            var hexPid = new System.Collections.ArrayList();
            for (int i = keyStartIndex; i <= keyEndIndex; i++) hexPid.Add(digitalProductId[i]);
            for (int i = decodeKeyLength - 1; i >= 0; i--)
            {
                // Do the actual decoding.
                var digitMapIndex = 0;
                for (int j = decodeStringLength - 1; j >= 0; j--)
                {
                    var byteValue = (digitMapIndex << 8) | (byte)hexPid[j];
                    hexPid[j] = (byte)(byteValue / 24);
                    digitMapIndex = byteValue % 24;
                    decodedChars[i] = digits[digitMapIndex];
                }
            }
            var key = new string(decodedChars);
            // Every sixth char is a separator.
            for (var i = 5; i < key.Length; i += 6) key = key.Insert(i, "-");
            return key;
        }

        /// <summary>
        /// Returns the decoded product key from the provided byte array. Works with Windows 8 and up.
        /// </summary>
        /// <param name="digitalProductId"></param>
        public static String DecodeOsProductKeyWin8AndUp(byte[] digitalProductId)
        {
            CExceptionProcessor.ExceptionIfNull(digitalProductId, "The specified digitalProductId cannot be null!", "digitalProductId");
            var key = String.Empty;
            // Length of decoded product key in byte-Form.
            // Each byte represents 2 chars.
            const int decodeStringLength = 15;
            // Offset of first byte of encoded product key in
            //  'DigitalProductIdxxx" REGBINARY value. Offset = 34H.
            const int keyStartIndex = 52;
            // Offset of last byte of encoded product key in
            //  'DigitalProductIdxxx" REGBINARY value. Offset = 43H.
            const int keyEndIndex = keyStartIndex + decodeStringLength;
            var isWin8 = (byte)((digitalProductId[keyEndIndex - 1] / 6) & 1);
            digitalProductId[keyEndIndex - 1] = (byte)((digitalProductId[keyEndIndex - 1] & 247) | (isWin8 & 2) * 4);

            // Possible alpha-numeric characters in product key.
            const string digits = "BCDFGHJKMPQRTVWXY2346789";
            var last = 0;
            for (var i = 24; i >= 0; i--)
            {
                var current = 0;
                for (var j = decodeStringLength - 1; j >= 0; j--)
                {
                    current *= 256;
                    current = digitalProductId[j + keyStartIndex] + current;
                    digitalProductId[j + keyStartIndex] = (byte)(current / 24);
                    current %= 24;
                    last = current;
                }
                key = digits[current] + key;
            }
            var keypart1 = key.Substring(1, last);
            const string insert = "N";
            key = key.Substring(1).Replace(keypart1, keypart1 + insert);
            if (last == 0) key = insert + key;
            // Every sixth char is a separator.
            for (var i = 5; i < key.Length; i += 6) key = key.Insert(i, "-");
            return key;
        }
        
        #endregion

        #region ServicePack - Gets the service pack information of the operating system running on this Computer.

        /// <summary>
        /// Returns the service pack information of the operating system running on this Computer.
        /// </summary>
        /// <returns>A String containing the operating system service pack inFormation.</returns>
        ///
        public static String OsServicePackString
        {
            get
            {
                var sp = Environment.OSVersion.ServicePack;
                return SystemInfoProcessor.IsWin8OrLater ? String.Empty : (String.IsNullOrEmpty(sp) ? "Service Pack 0" : sp);
            }
        }

        /// <summary>
        /// Returns the service pack information of the operating system running on this Computer.
        /// </summary>
        /// <returns>A int containing the operating system service pack number.</returns>
        ///
        public static int OsServicePackNumber
        {
            get
            {
                var osVersionInfo = new CNativeMethodProcessor.OSVERSIONINFOEX
                {
                    dwOSVersionInfoSize = Marshal.SizeOf(typeof(CNativeMethodProcessor.OSVERSIONINFOEX))
                };
                return !CNativeMethodProcessor.GetVersionEx(ref osVersionInfo) ? -1 : osVersionInfo.wServicePackMajor;
            }
        }
        
        #endregion

    }

    /// <summary>
    /// List of all operating systems
    /// </summary>
    public enum OsType
    {
        ///<summary>
        /// Windows 95/98, NT4.0, 2000
        ///</summary>
        Windows2000AndPrevious,
        ///<summary>
        /// Windows XP x86
        ///</summary>
        WindowsXP,
        ///<summary>
        /// Windows XP x64
        ///</summary>
        WindowsXP64,
        ///<summary>
        /// Windows Vista
        ///</summary>
        WindowsVista,
        ///<summary>
        /// Windows 7
        ///</summary>
        Windows7,
        ///<summary>
        /// Windows 8
        ///</summary>
        Windows8,
        ///<summary>
        /// Windows 8
        ///</summary>
        Windows81,
        ///<summary>
        /// Windows 10
        ///</summary>
        Windows10,
        ///<summary>
        /// Windows 2003 Server
        ///</summary>
        Windows2003,
        ///<summary>
        /// Windows 2003 R2 Server
        ///</summary>
        Windows2003R2,
        ///<summary>
        /// Windows 2008 Server
        ///</summary>
        Windows2008,
        ///<summary>
        /// Windows 2008 R2 Server
        ///</summary>
        Windows2008R2,
        ///<summary>
        /// Windows 2012 Server
        ///</summary>
        Windows2012,
        ///<summary>
        /// Windows 2012 R2 Server
        ///</summary>
        Windows2012R2,
        ///<summary>
        /// Windows 2016 Server
        ///</summary>
        Windows2016
    }

    /// <summary>
    /// A list of Product Types according to ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    /// </summary>
    [Flags]
    public enum ProductType
    {
        Undefined = 0,
        /// <summary>
        /// Workstation
        /// </summary>
        NTWorkstation = 1,
        /// <summary>
        /// Domain Controller
        /// </summary>
        NTDomainController = 2,
        /// <summary>
        /// Server
        /// </summary>
        NTServer = 3
    }

    /// <summary>
    /// General constants
    /// </summary>
    /// <summary>
    /// A list of Product Editions according to ( http://msdn.microsoft.com/en-us/library/ms724358(VS.85).aspx )
    /// </summary>
    [Flags]
    public enum ProductEditionEnum
    {
        /// <summary>
        /// Business
        /// </summary>
        Business = 6,
        /// <summary>
        /// BusinessN
        /// </summary>
        BusinessN = 16,
        /// <summary>
        /// ClusterServer
        /// </summary>
        ClusterServer = 18,
        /// <summary>
        /// DatacenterServer
        /// </summary>
        DatacenterServer = 8,
        /// <summary>
        /// DatacenterServerCore
        /// </summary>
        DatacenterServerCore = 12,
        /// <summary>
        /// DatacenterServerCoreV
        /// </summary>
        DatacenterServerCoreV = 39,
        /// <summary>
        /// DatacenterServerV
        /// </summary>
        DatacenterServerV = 37,

        //DeveloperPreview = 74,
        /// <summary>
        /// Enterprise
        /// </summary>
        Enterprise = 4,

        /// <summary>
        /// EnterpriseE
        /// </summary>
        EnterpriseE = 70,
        /// <summary>
        /// EnterpriseN
        /// </summary>
        EnterpriseN = 27,
        /// <summary>
        /// EnterpriseServer
        /// </summary>
        EnterpriseServer = 10,
        /// <summary>
        /// EnterpriseServerCore
        /// </summary>
        EnterpriseServerCore = 14,
        /// <summary>
        /// EnterpriseServerCoreV
        /// </summary>
        EnterpriseServerCoreV = 41,
        /// <summary>
        /// EnterpriseServerIA64
        /// </summary>
        EnterpriseServerIA64 = 15,
        /// <summary>
        /// EnterpriseServerV
        /// </summary>
        EnterpriseServerV = 38,
        /// <summary>
        /// HomeBasic
        /// </summary>
        HomeBasic = 2,
        /// <summary>
        /// HomeBasicE
        /// </summary>
        HomeBasicE = 67,
        /// <summary>
        /// HomeBasicN
        /// </summary>
        HomeBasicN = 5,
        /// <summary>
        /// HomePremium
        /// </summary>
        HomePremium = 3,
        /// <summary>
        /// HomePremiumE
        /// </summary>
        HomePremiumE = 68,
        /// <summary>
        /// HomePremiumN
        /// </summary>
        HomePremiumN = 26,

        //HomePremiumServer = 34,
        //HyperV = 42,
        /// <summary>
        /// MediumBusinessServerManagement
        /// </summary>
        MediumBusinessServerManagement = 30,

        /// <summary>
        /// MediumBusinessServerMessaging
        /// </summary>
        MediumBusinessServerMessaging = 32,
        /// <summary>
        /// MediumBusinessServerSecurity
        /// </summary>
        MediumBusinessServerSecurity = 31,
        /// <summary>
        /// Professional
        /// </summary>
        Professional = 48,
        /// <summary>
        /// ProfessionalE
        /// </summary>
        ProfessionalE = 69,
        /// <summary>
        /// ProfessionalN
        /// </summary>
        ProfessionalN = 49,

        //SBSolutionServer = 50,
        /// <summary>
        /// ServerForSmallBusiness
        /// </summary>
        ServerForSmallBusiness = 24,

        /// <summary>
        /// ServerForSmallBusinessV
        /// </summary>
        ServerForSmallBusinessV = 35,

        //ServerFoundation = 33,
        /// <summary>
        /// SmallBusinessServer
        /// </summary>
        SmallBusinessServer = 9,

        //SmallBusinessServerPremium = 25,
        //SolutionEmbeddedServer = 56,
        /// <summary>
        /// StandardServer
        /// </summary>
        StandardServer = 7,

        /// <summary>
        /// StandardServerCore
        /// </summary>
        StandardServerCore = 13,
        /// <summary>
        /// StandardServerCoreV
        /// </summary>
        StandardServerCoreV = 40,
        /// <summary>
        /// StandardServerV
        /// </summary>
        StandardServerV = 36,
        /// <summary>
        /// Starter
        /// </summary>
        Starter = 11,
        /// <summary>
        /// StarterE
        /// </summary>
        StarterE = 66,
        /// <summary>
        /// StarterN
        /// </summary>
        StarterN = 47,
        /// <summary>
        /// StorageEnterpriseServer
        /// </summary>
        StorageEnterpriseServer = 23,
        /// <summary>
        /// StorageExpressServer
        /// </summary>
        StorageExpressServer = 20,
        /// <summary>
        /// StorageStandardServer
        /// </summary>
        StorageStandardServer = 21,
        /// <summary>
        /// StorageWorkgroupServer
        /// </summary>
        StorageWorkgroupServer = 22,
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Ultimate
        /// </summary>
        Ultimate = 1,
        /// <summary>
        /// UltimateE
        /// </summary>
        UltimateE = 71,
        /// <summary>
        /// UltimateN
        /// </summary>
        UltimateN = 28,
        /// <summary>
        /// WebServer
        /// </summary>
        WebServer = 17,
        /// <summary>
        /// WebServerCore
        /// </summary>
        WebServerCore = 29
    }

    /// <summary>
    /// A list of Version Suite Masks according to ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    /// </summary>
    [Flags]
    public enum VERSuite
    {
        //SmallBusiness = 1,
        /// <summary>
        /// Enterprise
        /// </summary>
        Enterprise = 2,

        //BackOffice = 4,
        //Terminal = 16,
        //SmallBusinessRestricted = 32,
        /// <summary>
        /// EmbeddedNT
        /// </summary>
        EmbeddedNT = 64,

        /// <summary>
        /// Datacenter
        /// </summary>
        Datacenter = 128,

        //SingleUserTS = 256,
        /// <summary>
        /// Personal
        /// </summary>
        Personal = 512,

        /// <summary>
        /// Blade
        /// </summary>
        Blade = 1024,
        /// <summary>
        /// StorageServer
        /// </summary>
        StorageServer = 8192,
        /// <summary>
        /// ComputeServer
        /// </summary>
        ComputeServer = 16384
        //WHServer = 32768
    }

    [Flags]
    internal enum OtherConsts
    {
        //Type bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
        //VERMinorVersion = 1,
        //VERMajorVersion = 2,
        //VERBuildNumber = 4,
        //VERPlatformID = 8,
        //VERServicePackMinor = 16,
        //VERServicePackMajor = 32,
        //VERSuiteName = 64,
        //VERProductType = 128,

        //Condition bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
        //VEREqual = 1,
        //VERGreater = 2,
        //VERGreaterEqual = 3,
        //VERLess = 4,
        //VERLessEqual = 5,
        //VERAnd = 6, // only For wSuiteMask
        //VEROr = 7, // only For wSuiteMask

        //sysMetrics ( http://msdn.microsoft.com/en-us/library/ms724385(VS.85).aspx )
        SMTabletPC = 86,
        SMMediaCenter = 87,
        //SMStarter = 88,
        SMServerR2 = 89
    }

    
}
