using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace MyCodeLibrary.SystemInfo
{
    /// <summary>
    /// NT-Класс версии ОС с получением данных через WMI
    /// </summary>
    public class myOsVersionInfo : myOsVersionInfoBase
    {
        //TODO: все-таки надо разделить статический класс версии и класс-объект версии.

        /// <summary>
        /// Gets the full version of the operating system running on this Computer. Uses the newer WMI.
        /// </summary>
        public static new String get_Main()
        {
            return GetVersionInfo(VersionRequestType.Main); 
        }

        /// <summary>
        /// Gets the major version of the operating system running on this Computer. Uses the newer WMI.
        /// </summary>
        public static new int get_Major()
        {
            return Convert.ToInt32(GetVersionInfo(VersionRequestType.Major), CultureInfo.CurrentCulture); 
        }

        /// <summary>
        /// Gets the minor version of the operating system running on this Computer. Uses the newer WMI.
        /// </summary>
        public static new int get_Minor()
        {
            return Convert.ToInt32(GetVersionInfo(VersionRequestType.Minor), CultureInfo.CurrentCulture); 
        }

        /// <summary>
        /// Gets the build version of the operating system running on this Computer. Uses the newer WMI.
        /// </summary>
        public static new int get_Build()
        {
            return Convert.ToInt32(GetVersionInfo(VersionRequestType.Build), CultureInfo.CurrentCulture); 
        }

        /// <summary>
        /// Gets the revision version of the operating system running on this Computer. Uses the newer WMI.
        /// </summary>
        public static new int get_Revision()
        {
            return Convert.ToInt32(GetVersionInfo(VersionRequestType.Revision), CultureInfo.CurrentCulture); 
        }

        /// <summary>
        /// Return a numeric value representing OS version. Uses the newer WMI.
        /// </summary>
        /// <returns>(OSMajorVersion * 10 + OSMinorVersion)</returns>
        public static new int get_Number()
        {
            return (get_Major() * 10 + get_Minor()); 
        }
        /// <summary>
        /// NT-Get OS version info from WMI
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static String GetVersionInfo(VersionRequestType type)
        {
            try
            {
                String VersionString = String.Empty;
                using (var objMOS = new System.Management.ManagementObjectSearcher("SELECT * FROM  Win32_OperatingSystem"))
                {
                    foreach (var o in objMOS.Get()) { VersionString = o["Version"].ToString(); }//todo: check here
                }

                var Temp = String.Empty;
                var IndexOfPeriod = VersionString.IndexOf(".", StringComparison.CurrentCulture);
                var Major = VersionString.Substring(0, IndexOfPeriod);
                Temp = VersionString.Substring(Major.Length + 1);
                var Minor = Temp.Substring(0, IndexOfPeriod - 1);
                Temp = VersionString.Substring(Major.Length + 1 + Minor.Length + 1);
                String Build;
                String Revision;
                if (Temp.Contains("."))
                {
                    Build = Temp.Substring(0, IndexOfPeriod - 1);
                    Revision = VersionString.Substring(Major.Length + 1 + Minor.Length + 1 + Build.Length + 1);
                }
                else
                {
                    Build = Temp;
                    Revision = "0";
                }


                var ReturnString = "0";
                switch (type)
                {
                    case VersionRequestType.Main:
                        ReturnString = VersionString;
                        break;

                    case VersionRequestType.Major:
                        ReturnString = Major;
                        break;

                    case VersionRequestType.Minor:
                        ReturnString = Minor;
                        break;

                    case VersionRequestType.Build:
                        ReturnString = Build;
                        break;

                    case VersionRequestType.Revision:
                        ReturnString = Revision;
                        break;
                }

                if (String.IsNullOrEmpty(ReturnString)) return "0";
                return ReturnString;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                return "0";
            }
        }
        /// <summary>
        /// NT-типы запрашиваемых значений
        /// </summary>
        internal enum VersionRequestType
        {
            Main,
            Major,
            Minor,
            Build,
            Revision
        }

    }
    
    
    /// <summary>
    /// NT-Класс версии ОС с получением данных через Environment.OSVersion
    /// </summary>
    /// <remarks>
    /// Семантическая избыточность в версиях классов версий ОС
    /// Взяты функции из VersionObject, Version 
    /// 
    /// </remarks>
    public class myOsVersionInfoBase
    {

        #region Static functions
        /// <summary>
        /// Gets the full version of the operating system running on this Computer. Uses the deprecated OSVersion.
        /// </summary>
        ///
        public static String get_Main()
        {
            return Environment.OSVersion.Version.ToString();
        }

        /// <summary>
        /// Gets the major version of the operating system running on this Computer. Uses the deprecated OSVersion.
        /// </summary>
        public static int get_Major()
        {
            return Environment.OSVersion.Version.Major;
        }

        /// <summary>
        /// Gets the minor version of the operating system running on this Computer. Uses the deprecated OSVersion.
        /// </summary>
        public static int get_Minor()
        {
            return Environment.OSVersion.Version.Minor;
        }

        /// <summary>
        /// Gets the build version of the operating system running on this Computer. Uses the deprecated OSVersion.
        /// </summary>
        public static int get_Build()
        {
            return Environment.OSVersion.Version.Build; 
        }


        /// <summary>
        /// Gets the revision version of the operating system running on this Computer. Uses the deprecated OSVersion.
        /// </summary>
        public static int get_Revision()
        {
            return Environment.OSVersion.Version.Revision;
        }

        /// <summary>
        /// Return a numeric value representing OS version. Uses the deprecated OSVersion.
        /// </summary>
        /// <returns>(OSMajorVersion * 10 + OSMinorVersion)</returns>
        public static int get_Number()
        {
            return (get_Major() * 10 + get_Minor());
        }

        #endregion

    }
}
