using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;

namespace MyCodeLibrary
{
    /// <summary>
    /// NT-Объект свойств сборки и различные операции со сборками.
    /// </summary>
    public class AssemblyInfo
    {
        /// <summary>
        /// Specifies the version of the assembly being attributed.
        /// </summary>
        public string m_Version;
        /// <summary>
        /// Instructs a compiler to use a specific version number for the Win32 file version resource. 
        /// The Win32 file version is not required to be the same as the assembly's version number.
        /// </summary>
        public string m_FileVersion;
        /// <summary>
        /// Defines an assembly title custom attribute for an assembly manifest.
        /// </summary>
        public string m_Title;
        /// <summary>
        /// Provides a text description for an assembly.
        /// </summary>
        public string m_Description;
        /// <summary>
        /// Defines a product name custom attribute for an assembly manifest.
        /// </summary>
        public string m_Product; 
        /// <summary>
        /// Defines a copyright custom attribute for an assembly manifest.
        /// </summary>
        public string m_Copyright;
        /// <summary>
        /// Defines a company name
        /// </summary>
        public string m_Company;
        /// <summary>
        /// Specifies which culture the assembly supports.
        /// </summary>
        public string m_Culture;



        /// <summary>
        /// NT-Default constructor
        /// </summary>
        public AssemblyInfo()
        {
            return;
        }
        /// <summary>
        /// NT-Constructor
        /// </summary>
        /// <param name="a">Assembly</param>
        public AssemblyInfo(Assembly a)
        {
            this.m_Company = AssemblyInfo.getCompany(a);
            this.m_Copyright = AssemblyInfo.getCopyright(a);
            this.m_Description = AssemblyInfo.getDescription(a);
            this.m_Product = AssemblyInfo.getProduct(a);
            this.m_Title = AssemblyInfo.getTitle(a);
            this.m_Version = AssemblyInfo.getVersion(a);
            this.m_Culture = AssemblyInfo.getCulture(a);
            this.m_FileVersion = AssemblyInfo.getFileVersion(a);

            return;
        }

        /// <summary>
        /// NT-Constructor
        /// </summary>
        /// <param name="path">Assembly file path</param>
        public AssemblyInfo(string path)
        {
            Assembly a = AssemblyInfo.getAssembly(path);

            this.m_Company = AssemblyInfo.getCompany(a);
            this.m_Copyright = AssemblyInfo.getCopyright(a);
            this.m_Description = AssemblyInfo.getDescription(a);
            this.m_Product = AssemblyInfo.getProduct(a);
            this.m_Title = AssemblyInfo.getTitle(a);
            this.m_Version = AssemblyInfo.getVersion(a);

            this.m_Culture = AssemblyInfo.getCulture(a);
            this.m_FileVersion = AssemblyInfo.getFileVersion(a);

            return;
        }



        #region Static assembly attributes reading
        /// <summary>
        /// NT-Get calling assembly
        /// </summary>
        /// <returns></returns>
        public static Assembly getAssembly()
        {
            return Assembly.GetCallingAssembly();
        }
        /// <summary>
        /// NT- get assembly by file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Assembly getAssembly(string path)
        {
            return Assembly.LoadFile(path);
        }
        /// <summary>
        /// NT- get assembly of exe file
        /// </summary>
        /// <returns></returns>
        public static Assembly getEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// NT-Returns the version number
        /// </summary>
        public static String getVersionString(Assembly a)
        {
            return a.GetName().Version.ToString(); 
        }

        /// <summary>
        /// NT-Returns the version number 
        /// </summary>
        public static string getVersion(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyVersionAttribute)attributes[0]).Version;
        }

        /// <summary>
        /// NT-Returns the title of the assembly.
        /// </summary>
        public static string getTitle(Assembly a) 
        {    
            var attributes = a.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (!String.IsNullOrEmpty(titleAttribute.Title)) return titleAttribute.Title;
                }
                return Path.GetFileNameWithoutExtension(a.CodeBase);
        }

        /// <summary>
        /// NT-Returns the description  of the assembly.
        /// </summary>
        public static String getDescription(Assembly a)
        {
                var attributes = a.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// NT-Returns the product name  of the assembly.
        /// </summary>
        public static String getProduct(Assembly a)
        {

                var attributes = a.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) return String.Empty;
                return ((AssemblyProductAttribute)attributes[0]).Product;

        }

        /// <summary>
        /// NT-Returns the copyright info  of the assembly.
        /// </summary>
        public static String getCopyright(Assembly a)
        {
               var attributes = a.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) return String.Empty;
                else
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;

        }

        /// <summary>
        /// NT-Returns the company name  of the assembly.
        /// </summary>
        public static String getCompany(Assembly a)
        {
                var attributes = a.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) return String.Empty;
                else
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
        /// <summary>
        /// NT-Get Win32 file version string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getFileVersion(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyFileVersionAttribute)attributes[0]).Version;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getCulture(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(AssemblyCultureAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyCultureAttribute)attributes[0]).Culture;
        }

        #endregion

        #region Get application icon

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbSizeFileInfo,
            uint uFlags);

        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;

        /// <summary>
        /// NT-Извлечь иконку приложения из файла приложения
        /// </summary>
        /// <param name="appPath">Путь к файлу .exe  или .dll</param>
        /// <returns></returns>
        public static Icon getApplicationIcon(String appPath, bool LargeIcon)
        {
            IntPtr hImgSmall;
            SHFILEINFO shfi = new SHFILEINFO();
            //select icon size
            uint flags = SHGFI_ICON;
            if (LargeIcon == false)
                flags |= SHGFI_SMALLICON;
            //get icon from system memory
            //! нужно освободить память вызовом DestroyIcon после использования.
            //так что этот пример некорректный?
            hImgSmall = SHGetFileInfo(appPath, 0, ref shfi, (uint)Marshal.SizeOf(shfi), SHGFI_ICON | SHGFI_SMALLICON);
            if ((Int32)(shfi.hIcon) != 0)
                return Icon.FromHandle(shfi.hIcon);
            else
                //а это альтернативная версия, но она дает иконки похуже, чем весь код выше.
                return Icon.ExtractAssociatedIcon(appPath);
        }



        #endregion

        /// <summary>
        /// NT-Extract Embedded resource from assembly manifest
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="fileToExtractTo"></param>
        public static void SaveResourceToDisk(Assembly a, String resourceName, String fileToExtractTo)
        {
            var s = a.GetManifestResourceStream(resourceName);
            using (var resourceFile = new FileStream(fileToExtractTo, FileMode.Create))
            {
                var b = new byte[s.Length + 1];
                s.Read(b, 0, Convert.ToInt32(s.Length));
                resourceFile.Write(b, 0, Convert.ToInt32(b.Length - 1));
                resourceFile.Flush();
            }
        }
    }
    /// <summary>
    /// NT-Объект подробных свойств сборки и унаследованные операции со сборками.
    /// </summary>
    public class AssemblyInfoEx : AssemblyInfo
    {

        /// <summary>
        /// Specifies the build configuration, such as retail or debug, for an assembly.
        /// </summary>
        public string m_Configuration;
        /// <summary>
        /// Defines a trademark custom attribute for an assembly manifest.
        /// </summary>
        public string m_Trademark;
        /// <summary>
        /// Controls accessibility of an individual managed type or member, or of all types within an assembly, to COM.
        /// </summary>
        public string m_ComVisible;
        /// <summary>
        /// Supplies an explicit System.Guid when an automatic GUID is undesirable.
        /// </summary>
        public string m_Guid;
        /// <summary>
        /// Specifies an algorithm to hash all files in an assembly.
        /// </summary>
        public string m_AlgoritmId;
        /// <summary>
        /// Defines a friendly default alias for an assembly manifest.
        /// </summary>
        public string m_DefaultAlias;
        /// <summary>
        /// Specifies that the assembly is not fully signed when created.
        /// </summary>
        public string m_DelaySign;
        /// <summary>
        /// Specifies a bitwise combination of AssemblyNameFlags flags for an assembly, 
        /// describing just-in-time (JIT) compiler options, whether the assembly is retargetable, 
        /// and whether it has a full or tokenized public key. 
        /// </summary>
        public string m_Flags;
        /// <summary>
        /// Defines additional version information for an assembly manifest.
        /// </summary>
        public string m_InformationalVersion;
        /// <summary>
        /// Specifies the name of a file containing the key pair used to generate a strong name.
        /// </summary>
        public string m_KeyFile;
        /// <summary>
        /// Specifies the name of a key container within the CSP containing the key pair used to generate a strong name.
        /// </summary>
        public string m_KeyName;




        /// <summary>
        /// NT-Default constructor
        /// </summary>
        public AssemblyInfoEx():base()
        {
            return;
        }
        /// <summary>
        /// NT-Constructor
        /// </summary>
        /// <param name="a">Assembly</param>
        public AssemblyInfoEx(Assembly a):base(a)
        {
            this.m_Configuration = AssemblyInfoEx.getConfiguration(a);
            this.m_Trademark = AssemblyInfoEx.getTrademark(a);
            this.m_ComVisible = AssemblyInfoEx.getComVisible(a);
            this.m_Guid = AssemblyInfoEx.getGuid(a);
            this.m_AlgoritmId = AssemblyInfoEx.getAlgoritmId(a);
            this.m_DefaultAlias = AssemblyInfoEx.getDefaultAlias(a);
            this.m_DelaySign = AssemblyInfoEx.getDelaySign(a);
            this.m_Flags = AssemblyInfoEx.getFlags(a);
            this.m_InformationalVersion = AssemblyInfoEx.getInformationalVersion(a);
            this.m_KeyFile = AssemblyInfoEx.getKeyFile(a);
            this.m_KeyName = AssemblyInfoEx.getKeyName(a);

            return;
        }

        /// <summary>
        /// NT-Constructor
        /// </summary>
        /// <param name="path">Assembly file path</param>
        public AssemblyInfoEx(string path)
        {
            //тут все поля этого и родительского класов заполняем отсюда, чтобы не загружать сборку дважды.
            Assembly a = AssemblyInfo.getAssembly(path);

            this.m_Company = AssemblyInfo.getCompany(a);
            this.m_Copyright = AssemblyInfo.getCopyright(a);
            this.m_Description = AssemblyInfo.getDescription(a);
            this.m_Product = AssemblyInfo.getProduct(a);
            this.m_Title = AssemblyInfo.getTitle(a);
            this.m_Version = AssemblyInfo.getVersion(a);

            this.m_Configuration = AssemblyInfoEx.getConfiguration(a);
            this.m_Trademark = AssemblyInfoEx.getTrademark(a);
            this.m_Culture = AssemblyInfoEx.getCulture(a);
            this.m_ComVisible = AssemblyInfoEx.getComVisible(a);
            this.m_Guid = AssemblyInfoEx.getGuid(a);
            this.m_FileVersion = AssemblyInfoEx.getFileVersion(a);

            this.m_AlgoritmId = AssemblyInfoEx.getAlgoritmId(a);
            this.m_DefaultAlias = AssemblyInfoEx.getDefaultAlias(a);
            this.m_DelaySign = AssemblyInfoEx.getDelaySign(a);
            this.m_Flags = AssemblyInfoEx.getFlags(a);
            this.m_InformationalVersion = AssemblyInfoEx.getInformationalVersion(a);
            this.m_KeyFile = AssemblyInfoEx.getKeyFile(a);
            this.m_KeyName = AssemblyInfoEx.getKeyName(a);

            return;
        }



        #region Static assembly attributes reading

        /// <summary>
        /// NT-Get assembly Guid string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getGuid(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((System.Runtime.InteropServices.GuidAttribute)attributes[0]).Value;
        }
        /// <summary>
        /// NT-Get assembly attribute. 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getComVisible(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(ComVisibleAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((ComVisibleAttribute)attributes[0]).Value.ToString();
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getTrademark(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyTrademarkAttribute)attributes[0]).Trademark;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getConfiguration(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((System.Reflection.AssemblyConfigurationAttribute)attributes[0]).Configuration;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getKeyName(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyKeyNameAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyKeyNameAttribute)attributes[0]).KeyName;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getKeyFile(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyKeyFileAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyKeyFileAttribute)attributes[0]).KeyFile;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getInformationalVersion(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getFlags(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyFlagsAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
            {
                int flags = ((AssemblyFlagsAttribute)attributes[0]).AssemblyFlags;
                AssemblyNameFlags f = (AssemblyNameFlags) flags;
                return f.ToString();
            }
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getDelaySign(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyDelaySignAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyDelaySignAttribute)attributes[0]).DelaySign.ToString();
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getDefaultAlias(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyDefaultAliasAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyDefaultAliasAttribute)attributes[0]).DefaultAlias;
        }
        /// <summary>
        /// NT-Get assembly attribute string
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string getAlgoritmId(Assembly a)
        {
            var attributes = a.GetCustomAttributes(typeof(System.Reflection.AssemblyAlgorithmIdAttribute), false);
            if (attributes.Length == 0) return String.Empty;
            else
                return ((AssemblyAlgorithmIdAttribute)attributes[0]).AlgorithmId.ToString();
        }

        #endregion
    }//end class

}
