using System;
//using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MyCodeLibrary.ShellLinkOne
{
    #region Interfaces 
    
       public enum ESFGAO
    {
        SFGAO_BROWSABLE = 0x8000000,
        SFGAO_CANCOPY = 1,
        SFGAO_CANDELETE = 0x20,
        SFGAO_CANLINK = 4,
        SFGAO_CANMOVE = 2,
        SFGAO_CANRENAME = 0x10,
        SFGAO_CAPABILITYMASK = 0x177,
        SFGAO_COMPRESSED = 0x4000000,
        SFGAO_CONTENTSMASK = -2147483648,
        SFGAO_DISPLAYATTRMASK = 0xf0000,
        SFGAO_DROPTARGET = 0x100,
        SFGAO_FILESYSANCESTOR = 0x10000000,
        SFGAO_FILESYSTEM = 0x40000000,
        SFGAO_FOLDER = 0x20000000,
        SFGAO_GHOSTED = 0x80000,
        SFGAO_HASPROPSHEET = 0x40,
        SFGAO_HASSUBFOLDER = -2147483648,
        SFGAO_LINK = 0x10000,
        SFGAO_NONENUMERATED = 0x100000,
        SFGAO_READONLY = 0x40000,
        SFGAO_REMOVABLE = 0x2000000,
        SFGAO_SHARE = 0x20000,
        SFGAO_VALIDATE = 0x1000000
    }

    public enum ESHCONTF
    {
        SHCONTF_FOLDERS = 0x20,
        SHCONTF_INCLUDEHIDDEN = 0x80,
        SHCONTF_NONFOLDERS = 0x40
    }

    public enum ESHGNO
    {
        SHGDN_FORADDRESSBAR = 0x4000,
        SHGDN_FORPARSING = 0x8000,
        SHGDN_INFOLDER = 1,
        SHGDN_NORMAL = 0
    }

    public enum ESTRRET
    {
        STRRET_WSTR,
        STRRET_OFFSET,
        STRRET_CSTR
    }

    [Flags()]
    public enum SHELL_LINK_DATA_FLAGS
    {
        SLDF_DEFAULT = 0x00000000,
        SLDF_HAS_ID_LIST = 0x00000001,
        SLDF_HAS_LINK_INFO = 0x00000002,
        SLDF_HAS_NAME = 0x00000004,
        SLDF_HAS_RELPATH = 0x00000008,
        SLDF_HAS_WORKINGDIR = 0x00000010,
        SLDF_HAS_ARGS = 0x00000020,
        SLDF_HAS_ICONLOCATION = 0x00000040,
        SLDF_UNICODE = 0x00000080,
        SLDF_FORCE_NO_LINKINFO = 0x00000100,
        SLDF_HAS_EXP_SZ = 0x00000200,
        SLDF_RUN_IN_SEPARATE = 0x00000400,
        SLDF_HAS_LOGO3ID = 0x00000800,
        SLDF_HAS_DARWINID = 0x00001000,
        SLDF_RUNAS_USER = 0x00002000,
        SLDF_HAS_EXP_ICON_SZ = 0x00004000,
        SLDF_NO_PIDL_ALIAS = 0x00008000,
        SLDF_FORCE_UNCNAME = 0x00010000,
        SLDF_RUN_WITH_SHIMLAYER = 0x00020000,
        SLDF_FORCE_NO_LINKTRACK = 0x00040000,
        SLDF_ENABLE_TARGET_METADATA = 0x000800000,
        SLDF_DISABLE_LINK_PATH_TRACKING = 0x00100000,
        SLDF_DISABLE_KNOWNFOLDER_RELATIVE_TRACKING = 0x00200000,
        SLDF_NO_KF_ALIAS = 0x00400000,
        SLDF_ALLOW_LINK_TO_LINK = 0x00800000,
        SLDF_UNALIAS_ON_SAVE = 0x01000000,
        SLDF_PREFER_ENVIRONMENT_PATH = 0x02000000,
        SLDF_KEEP_LOCAL_IDLIST_FOR_UNC_TARGET = 0x04000000,
        SLDF_PERSIST_VOLUME_ID_RELATIVE = 0x08000000,
        SLDF_VALID = 0x0FFFF7FF,
        SLDF_RESERVED = unchecked((int)0x80000000),
    }

    [ComImport, Guid("00000002-0000-0000-C000-000000000046"), InterfaceType((short)1)]
    public interface IMalloc
    {
        [PreserveSig]
        int Alloc([In] int cb);
        [PreserveSig]
        int Realloc([In] IntPtr pv, [In] int cb);
        [PreserveSig]
        void Free([In] IntPtr pv);
        [PreserveSig]
        int GetSize([In] IntPtr pv);
        [PreserveSig]
        int DidAlloc([In] IntPtr pv);
        [PreserveSig]
        void HeapMinimize();
    }

    [ComImport, Guid("0000010C-0000-0000-C000-000000000046"), InterfaceType((short)1)]
    public interface IPersist
    {
        void GetClassID([In, Out] ref Guid pClassID);
    }

    [ComImport, InterfaceType((short)1), Guid("0000010B-0000-0000-C000-000000000046")]
    public interface IPersistFile : IPersist
    {
        void GetClassID([In, Out] ref Guid pClassID);
        [PreserveSig]
        int IsDirty();
        void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In] int dwMode);
        void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In] int fRemember);
        void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([In, Out, MarshalAs(UnmanagedType.LPWStr)] ref string ppszFileName);
    }

    [ComImport, InterfaceType((short)1), Guid("000214E6-0000-0000-C000-000000000046")]
    public interface IShellFolder
    {
        void ParseDisplayName([In] IntPtr hwndOwner, [In] IntPtr pbcReserved, [In, MarshalAs(UnmanagedType.LPWStr)] string lpszDisplayName, [In, Out] ref int pchEaten, [In, Out] ref IntPtr ppidl, [In, Out] ref int pdwAttributes);
        void EnumObjects([In] int hwndOwner, [In] int grfFlags, [In, Out] ref int ppenumIDList);
        void BindToObject([In] int pidl, [In] int pbcReserved, [In] ref Guid riid, [In, Out] ref int ppvOut);
        void BindToStorage([In] int pidl, [In] int pbcReserved, [In] ref Guid riid, [In, Out] ref int ppvObj);
        void CompareIDs([In] int lParam, [In] int pidl1, [In] int pidl2);
        void CreateViewObject([In] int hwndOwner, [In] ref Guid riid, [In, Out] ref int ppvOut);
        void GetAttributesOf([In] int cidl, [In] ref int apidl, [In, Out] ref int rgfInOut);
        void GetUIObjectOf([In] int hwndOwner, [In] int cidl, [In] ref int apidl, [In] ref Guid riid, [In, Out] ref int prgfInOut, [In, Out] ref int ppvOut);
        void GetDisplayNameOf([In] int pidl, [In] int uFlags, [In, Out] ref STRRET lpName);
        void SetNameOf([In] int hwndOwner, [In] int pidl, [In, MarshalAs(UnmanagedType.LPWStr)] string lpszName, [In] int uFlags, [In, Out] ref int ppidlOut);
    }

    [ComImport, InterfaceType((short)1), Guid("000214EE-0000-0000-C000-000000000046")]
    public interface IShellLink
    {
        void GetPath([In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, [In] int cchMaxPath, [In, Out] IntPtr pfd, [In] int fFlags);
        void GetIDList(out int ppidl);
        void SetIDList([In] IntPtr pidl);
        void GetDescription([In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, [In] int cchMaxName);
        void SetDescription([In, MarshalAs(UnmanagedType.LPStr)] string pszName);
        void GetWorkingDirectory([In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, [In] int cchMaxPath);
        void SetWorkingDirectory([In, MarshalAs(UnmanagedType.LPStr)] string pszDir);
        void GetArguments([In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, [In] int cchMaxPath);
        void SetArguments([In, MarshalAs(UnmanagedType.LPStr)] string pszArgs);
        void GetHotkey([In, Out] ref short pwHotkey);
        void SetHotkey([In] short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd([In] int iShowCmd);
        void GetIconLocation([In, Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, [In] int cchIconPath, [In, Out] ref int piIcon);
        void SetIconLocation([In, MarshalAs(UnmanagedType.LPStr)] string pszIconPath, [In] int iIcon);
        void SetRelativePath([In, MarshalAs(UnmanagedType.LPStr)] string pszPathRel, [In] int dwReserved);
        void Resolve([In] int hwnd, [In] int fFlags);
        void SetPath([In, MarshalAs(UnmanagedType.LPStr)] string pszFile);
    }

    [ComImportAttribute()]
    [GuidAttribute("45e2b4ae-b1c3-11d0-b92f-00a0c90312e1")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellLinkDataList
    {
        void AddDataBlock(IntPtr pDataBlock);
        void CopyDataBlock(int dwSig, out IntPtr ppDataBlock);
        void RemoveDataBlock(int dwSig);
        void GetFlags(out SHELL_LINK_DATA_FLAGS pdwFlags);
        void SetFlags(SHELL_LINK_DATA_FLAGS dwFlags);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct ITEMIDLIST
    {
        public SHITEMID mkid;
    }

    [ComImport, InterfaceType((short)1), Guid("00000000-0000-0000-C000-000000000046")]
    public interface IUnknown
    {
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct SHITEMID
    {
        public short cb;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] abID;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct STRRET
    {
        public ESTRRET uType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] cStr;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DATABLOCK_HEADER
    {
        int cbSize;
        int dwSignature;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
    public struct EXP_DARWIN_LINK_ANSI
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDarwinID;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    public struct EXP_DARWIN_LINK_UNI
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szwDarwinID;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct EXP_DARWIN_LINK
    {
        public DATABLOCK_HEADER dbh;
        public EXP_DARWIN_LINK_ANSI ansiDarwinID;
        public EXP_DARWIN_LINK_UNI uniDarwinID;
    }

#endregion
    //commande d'affichage
    public enum SW
    {
        SW_NORMAL = 0x1, //normal
        SW_MINIMIZED = 0x2, //réduite
        SW_MAXIMIZED = 0x3 //agrandie
    }

    //raccourci clavier
    public enum HOTKEYF
    {
        HOTKEYF_NONE = 0x0,
        HOTKEYF_SHIFT = 0x1,
        HOTKEYF_CONTROL = 0x2,
        HOTKEYF_ALT = 0x4,
        HOTKEYF_EXT = 0x8
    }

    //propriétés d'un raccourci
    public class ShellLink
    {
        public string TargetFile { get; internal set; } //cible
        public string WorkingDir { get; internal set; } //dossier démarrage
        public string Decription { get; internal set; } //description
        public string IconPath { get; internal set; } //chemin du fichier sui contient l'icone affichée
        public int IconIndex { get; internal set; } //index de l'icone dans ce fichier
        public byte HotKey { get; internal set; } //touche de raccourci clavier
        public HOTKEYF HotKeyModifier { get; internal set; } //touche de raccourci clavier Ctrl Shift Alt
        public SW ShowCommand { get; internal set; } //commande d'affichage
        public SHELL_LINK_DATA_FLAGS Flags { get; internal set; } //flags
        public string Arguments { get; internal set; } // argument de la ligne de commande

        public ShellLink()
        {
            this.TargetFile = string.Empty;
            this.WorkingDir = string.Empty;
            this.Decription = string.Empty;
            this.IconPath = string.Empty;
            this.IconIndex = -1;
            this.HotKey = 0;
            this.HotKeyModifier = HOTKEYF.HOTKEYF_NONE;
            this.ShowCommand = SW.SW_NORMAL;
            this.Arguments = string.Empty;
        }
    }

    public class ShellLinksManager
    {

        //*********************************************
        //Ce module est basé sur le code de JC Alsup (www.vbbyjc.com)
        //(la fonction CreateShortCut)
        private const int CLSCTX_INPROC_SERVER = 1; //CoCreateInstance
        //context flag
        private const int S_OK = 0; //COM - success
        private const int S_FALSE = 1; //COM - failure
        private const int cNULL = 0; //C-style NULL
        private const int STGM_DIRECT = 0x0; //Storage flags
        private const int SLR_UPDATE = 0x4; //IShellLink resolve
        //flags
        private const int EXP_DARWIN_ID_SIG = unchecked((int)0xa0000006);

        [DllImport("ole32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        extern public static int CoCreateInstance(ref Guid refCLSID, IntPtr pkUnkOuter, int dwClsContext, ref Guid refIID, ref IntPtr ppv);
        [DllImport("shell32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        extern public static int SHGetMalloc(ref IntPtr ppMalloc);
        [DllImport("shell32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        extern public static int SHGetDesktopFolder(ref IntPtr ppshf);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);
        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern Int32 MsiGetProductInfo(string product, string property, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder valueBuf, ref int len); 

        //****************************************************************
        //    ByVal sShortcutTitle As String : titre du raccourci
        //    ByVal sTargetFile As String : fichier cible existant
        //    ByVal sCreateInPath As String : dossier dans lequel le fichier .lnk sera créé
        //    Optional ByVal sWorkingDir As String : dossier dans lequel la cible est executée
        //    Optional ByVal sIconPath As String : chemin du fichier contenant l'icone du raccourci
        //    Optional ByVal iIconIndex As Long : index de l'icone à afficher
        //    Optional ByVal iHotKey As Byte : Touche de raccourci (code Ascii)
        //    Optional ByVal iHotKeyModifier As HOTKEYF : touches de raccourci complementaires
        //    Optional ByVal iShowCommand As SW = SW_NORMAL : affichage de la cible
        //    Optional ByVal sArguments As String = "" : argument de la ligne de commande de l'executable cible
        //****************************************************************
        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex, byte iHotKey, HOTKEYF iHotKeyModifier, SW iShowCommand, string sArguments, string sDescription, SHELL_LINK_DATA_FLAGS flags)
        {

            //un code d'erreur API

            //On définit les GUID dont on a besoins ----------------------------------------
            Guid IID_IShellLink = new Guid("000214EE-0000-0000-C000-000000000046");
            Guid IID_IPersistFile = new Guid("0000010B-0000-0000-C000-000000000046");
            Guid CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");

            //CLSID_ShellLink = "{00021401-0000-0000-C000-000000000046}"
            //IID_IShellLink = "{000214EE-0000-0000-C000-000000000046}"
            //IID_IPersistFile = "{0000010B-0000-0000-C000-000000000046}"

            //on verifie que sCreateInPath se termine par un "\"
            if (!sCreateInPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                sCreateInPath = sCreateInPath + System.IO.Path.DirectorySeparatorChar;
            }

            string sSHFile = sCreateInPath + sShortcutTitle + ".lnk";

            //on verifie que sCreateInPath ne se termine pas par un "\"
            if (sWorkingDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                sWorkingDir = sWorkingDir.Substring(0, sWorkingDir.Length - 2);
            }

            IntPtr ppvMalloc = IntPtr.Zero;
            IMalloc oMalloc = null;
            IntPtr ppvDesktop = IntPtr.Zero;
            IShellFolder oDesktop = null;
            IntPtr pidl = IntPtr.Zero;
            int nEaten = 0;
            IntPtr ppvShellLink = IntPtr.Zero;
            IShellLink oShellLink = null;
            IntPtr ppvIPersistFile = IntPtr.Zero;
            IPersistFile oPersistFile = null; //Pointeur vers l'interface IShellLink pour le
            //On obtient le handle du Shell's memory allocator
            int r = SHGetMalloc(ref ppvMalloc);
            if (r == S_OK)
            {
                oMalloc = (IMalloc)Marshal.GetTypedObjectForIUnknown(ppvMalloc, typeof(IMalloc));

                //On obtient le dossier du bureau.

                r = SHGetDesktopFolder(ref ppvDesktop);
                if (r == S_OK)
                {
                    oDesktop = (IShellFolder)Marshal.GetTypedObjectForIUnknown(ppvDesktop, typeof(IShellFolder));

                    //On essaie d'obtenir le pidl de notre fichier cible.
                    int tempRefParam2 = 0;
                    oDesktop.ParseDisplayName(IntPtr.Zero, IntPtr.Zero, sTargetFile, ref nEaten, ref pidl, ref tempRefParam2);

                    //----------------------------------------------------------------------
                    //On crée un objet ShellLink (CoClass) et on demande son interface IShellLink

                    //ShellLink coclass que nous allons créer.
                    r = CoCreateInstance(ref CLSID_ShellLink, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref IID_IShellLink, ref ppvShellLink);

                    if (r == S_OK)
                    {
                        //Succés - On crée un objet VB pour l'utiliser
                        oShellLink = (IShellLink)Marshal.GetTypedObjectForIUnknown(ppvShellLink, typeof(IShellLink));

                        //On définit la cible du raccourci
                        oShellLink.SetIDList(pidl);

                        //On remplie les differents propriétés du raccourci.
                        //------------------------------------------------------------
                        //On définit nom/description du raccourci
                        oShellLink.SetDescription(sDescription);

                        if (!string.IsNullOrEmpty(sWorkingDir))
                        {
                            //On définit le répertoire de travail
                            oShellLink.SetWorkingDirectory(sWorkingDir);
                        }

                        if (!string.IsNullOrEmpty(sArguments))
                        {
                            //On définit le ou les argument(s) de la ligne de commande
                            oShellLink.SetArguments(sArguments);
                        }

                        if (!string.IsNullOrEmpty(sIconPath))
                        {
                            //On définit le chemin de l'icone (autre que celle du fichier cible)
                            oShellLink.SetIconLocation(sIconPath, iIconIndex);
                        }

                        if (iHotKey != 0)
                        {
                            //On définie la touche de raccourci et les touches de raccourci de contrôle
                            oShellLink.SetHotkey((short)(((int)iHotKeyModifier) << 8 + (int)iHotKey));
                        }

                        if (((int)iShowCommand) != 0)
                        {
                            //On définit la commande d'affichage
                            oShellLink.SetShowCmd((int)iShowCommand);
                        }
                    }
                    else
                        Marshal.ThrowExceptionForHR(r);

                    //On résout le chemein de la cible pour être sur que tout est OK
                    oShellLink.Resolve(0, SLR_UPDATE);
                }
                else
                    Marshal.ThrowExceptionForHR(r);

                if (flags != 0)
                {
                    IShellLinkDataList dataList = (IShellLinkDataList)oShellLink;
                    SHELL_LINK_DATA_FLAGS sFlags;
                    dataList.GetFlags(out sFlags);
                    sFlags |= flags;
                    dataList.SetFlags(sFlags);
                }

                //Nous avons défini les propriété du raccourci, maintenant nous devons
                //l'enregistrer. Nous aurons besoin d'un pointeur vers
                //une interface IPersistFile de ShellLink pour enregistrer
                oPersistFile = (IPersistFile)oShellLink;

                oPersistFile.Save(sSHFile, cNULL);

                //On nettoie et libère le pointeur vers IPersistFile.
                oPersistFile = null;

                //On nettoie et libère le pointeur vers IShellLink.
                oShellLink = null;
            }
            else
                Marshal.ThrowExceptionForHR(r);

            //On libère le pidl
            oMalloc.Free(pidl);

            //On nettoie et libère le pointeur vers Desktop Folder.
            oDesktop = null;

            //On nettoie et libère le pointeur vers IMalloc.
            oMalloc = null;
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex, byte iHotKey, HOTKEYF iHotKeyModifier, SW iShowCommand, string sArguments)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, iIconIndex, iHotKey, iHotKeyModifier, iShowCommand, sArguments, "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex, byte iHotKey, HOTKEYF iHotKeyModifier, SW iShowCommand)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, iIconIndex, iHotKey, iHotKeyModifier, iShowCommand, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex, byte iHotKey, HOTKEYF iHotKeyModifier)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, iIconIndex, iHotKey, iHotKeyModifier, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex, byte iHotKey)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, iIconIndex, iHotKey, HOTKEYF.HOTKEYF_SHIFT, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath, int iIconIndex)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, iIconIndex, 0, HOTKEYF.HOTKEYF_SHIFT, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir, string sIconPath)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, sIconPath, 0, 0, HOTKEYF.HOTKEYF_SHIFT, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath, string sWorkingDir)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, sWorkingDir, String.Empty, 0, 0, HOTKEYF.HOTKEYF_SHIFT, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        public static void CreateShortcut(string sShortcutTitle, string sTargetFile, string sCreateInPath)
        {
            CreateShortcut(sShortcutTitle, sTargetFile, sCreateInPath, String.Empty, String.Empty, 0, 0, HOTKEYF.HOTKEYF_SHIFT, SW.SW_NORMAL, "", "", (SHELL_LINK_DATA_FLAGS)0);
        }

        /// <summary>
        /// NT-Ïðî÷èòàòü ôàéä ÿðëûêà
        /// </summary>
        /// <param name="sFileName">Ïóòü ê ôàéëó ÿðëûêà</param>
        /// <returns>Âîçâðàùàåò îáúåêò ÿðëûêà</returns>
        /// <remarks>//raise COMException if sFileName not a .lnk file</remarks>
        public static ShellLink GetShortcut(string sFileName)
        {

            //Cette fonction retourne les propriétés d'un raccourci dans une structure ShellLink

            //On définit les GUID dont on a besoins ----------------------------------------
            ShellLink result = new ShellLink();
            Guid IID_IShellLink = new Guid("000214EE-0000-0000-C000-000000000046");
            Guid IID_IPersistFile = new Guid("0000010B-0000-0000-C000-000000000046");
            Guid CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");

            //----------------------------------------------------------------------
            string sSHFile = String.Empty;

            //----------------------------------------------------------------------
            //On initialise COM (Ce n'est problablement pas nécessaire mais c'est une bonne
            //pratique néanmoins.

            IntPtr ppvShellLink = IntPtr.Zero;
            IShellLink oShellLink = null;
            IPersistFile oPersistFile = null;
            int HotKey = 0; //Pointeur vers l'interface IShellLink pour le
            //----------------------------------------------------------------------
            //On crée un objet ShellLink (CoClass) et on demande son interface IShellLink

            //ShellLink coclass que nous allons créer.
            int r = CoCreateInstance(ref CLSID_ShellLink, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref IID_IShellLink, ref ppvShellLink);

            if (r == S_OK)
            {
                //Succés - On crée un objet VB pour l'utiliser
                oShellLink = (IShellLink)Marshal.GetTypedObjectForIUnknown(ppvShellLink, typeof(IShellLink));

                //Nous avons un objet IShellLink. Nous allons maintenant ouvrir le fichier .lnk
                //Pour cela, nous avons besoin d'un pointeur vers une interface IPersistFile de ShellLink
                oPersistFile = (IPersistFile)oShellLink;

                //On ouvre le raccourci
                //raise COMException if not a lnk file
                oPersistFile.Load(sFileName, cNULL);
                StringBuilder sb;
                //Ouverture réussie
                //On lit les differentes propriétés du raccourci
                sb = new StringBuilder(255);
                oShellLink.GetArguments(sb, 255);
                result.Arguments = sb.ToString();

                sb = new StringBuilder(255);
                oShellLink.GetDescription(sb, 255);
                result.Decription = sb.ToString();

                short tempRefParam4 = (short)HotKey;
                oShellLink.GetHotkey(ref tempRefParam4);
                HotKey = tempRefParam4;
                result.HotKey = (byte)(HotKey % 256); //= CInt(HotKey And CLng(&HFF))
                result.HotKeyModifier = (HOTKEYF)(((int)HOTKEYF.HOTKEYF_ALT) / 256);

                sb = new StringBuilder(255);
                int t = result.IconIndex;
                oShellLink.GetIconLocation(sb, 255, ref t);
                result.IconPath = sb.ToString();

                sb = new StringBuilder(255);
                IntPtr tempRefParam5 = IntPtr.Zero;
                oShellLink.GetPath(sb, 255, tempRefParam5, 0);
                result.TargetFile = sb.ToString();

                sb = new StringBuilder(255);
                oShellLink.GetWorkingDirectory(sb, 255);
                result.WorkingDir = sb.ToString();

                int tempRefParam6 = (int)result.ShowCommand;
                oShellLink.GetShowCmd(out tempRefParam6);
                result.ShowCommand = (SW)tempRefParam6;

                IShellLinkDataList oShellLinkDataList = (IShellLinkDataList)oShellLink;
                SHELL_LINK_DATA_FLAGS flags;
                oShellLinkDataList.GetFlags(out flags);
                result.Flags = flags;

                if ((flags & SHELL_LINK_DATA_FLAGS.SLDF_HAS_DARWINID) == SHELL_LINK_DATA_FLAGS.SLDF_HAS_DARWINID)
                {
                    IntPtr ptr;
                    oShellLinkDataList.CopyDataBlock(EXP_DARWIN_ID_SIG, out ptr);

                    EXP_DARWIN_LINK dw = (EXP_DARWIN_LINK)Marshal.PtrToStructure(ptr, typeof(EXP_DARWIN_LINK));

                    int len = 1024;
                    StringBuilder productName = new StringBuilder(len);
                    if ((flags & SHELL_LINK_DATA_FLAGS.SLDF_UNICODE) == SHELL_LINK_DATA_FLAGS.SLDF_UNICODE)
                        MsiGetProductInfo(dw.uniDarwinID.szwDarwinID, "ProductName", productName, ref len);
                    else
                        MsiGetProductInfo(dw.ansiDarwinID.szDarwinID, "ProductName", productName, ref len);

                    result.TargetFile = productName.ToString();

                    LocalFree(ptr);
                }

                //On nettoie et on libère le pointeur vers IShellLink.
                oShellLink = null;
            }
            else
                Marshal.ThrowExceptionForHR(r);

            //On nettoie et on libère le pointeur vers IPersistFile.
            oPersistFile = null;

            return result;
        }

        //****************************************************************
        //    ByVal sShortcutTitle As String : titre du raccourci
        //    ByVal sCreateInPath As String : dossier dans lequel le fichier .lnk sera créé
        //    Shortcut As ShellLink : structure contenant les propriétés d'un raccourci à modifier
        //****************************************************************
        public static void ModifyShortcut(string sShortcutTitle, string sCreateInPath, ShellLink shortcut)
        {
            CreateShortcut(sShortcutTitle, shortcut.TargetFile, sCreateInPath, shortcut.WorkingDir, shortcut.IconPath, shortcut.IconIndex, shortcut.HotKey, shortcut.HotKeyModifier, shortcut.ShowCommand, shortcut.Arguments, shortcut.Decription, (SHELL_LINK_DATA_FLAGS)0);
        }
    }
}

#region Ïðèìåð èñïîëüçîâàíèÿ êëàññà
//internal partial class frmMain
//        : System.Windows.Forms.Form
//    {
//        private void btnSetTarget_Click(Object eventSender, EventArgs eventArgs)
//        {
//            CDOpen.Filter = "Tous|*.*";
//            if (CDOpen.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
//            txtTarget.Text = CDOpen.FileName;
//        }

//        private void btnSetIconPath_Click(Object eventSender, EventArgs eventArgs)
//        {
//            CDOpen.Filter = "Tous|*.*";
//            if (CDOpen.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
//            txtIconPath.Text = CDOpen.FileName;
//        }

//        private void btnOpenLink_Click(Object eventSender, EventArgs eventArgs)
//        {
//            try
//            {
//                CDOpen.Filter = "Lien|*.lnk";
//                CDOpen.FilterIndex = 0;
//                if (CDOpen.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
//                ShellLink sh = ShellLinksManager.GetShortcut(CDOpen.FileName);
//                txtTarget.Text = sh.TargetFile;
//                txtCreateInPath.Text = System.IO.Path.GetDirectoryName(CDOpen.FileName);
//                txtTitle.Text = System.IO.Path.GetFileNameWithoutExtension(CDOpen.FileName);
//                cbShowCmd.SelectedIndex = ((int)sh.ShowCommand) - 1;
//                txtArgument.Text = sh.Arguments;
//                txtIconPath.Text = (string.IsNullOrEmpty(sh.IconPath) ? sh.TargetFile : sh.IconPath);
//                txtIconIndex.Text = sh.IconIndex.ToString();
//                txtShortcut.Text = GetHK(sh.HotKey, sh.HotKeyModifier);
//                txtDescription.Text = sh.Decription;
//                txtWorkingDir.Text = sh.WorkingDir;
//                txtFlags.Text = sh.Flags.ToString();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(this, "Error:" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void btnSaveLink_Click(Object eventSender, EventArgs eventArgs)
//        {
//            try
//            {
//                HOTKEYF HKM = HOTKEYF.HOTKEYF_NONE;

//                byte HK = 0;
//                if (!string.IsNullOrEmpty(txtShortcut.Text))
//                {
//                    if (txtShortcut.Text.Contains("KeyAscii"))
//                        HK = Byte.Parse(System.Text.RegularExpressions.Regex.Match(txtShortcut.Text, @"KeyAscii\((\d+)\)").Result("$1"));
//                    if (txtShortcut.Text.IndexOf("Shift") >= 0)
//                    {
//                        HKM |= HOTKEYF.HOTKEYF_SHIFT;
//                    }
//                    if (txtShortcut.Text.IndexOf("Ctrl") >= 0)
//                    {
//                        HKM |= HOTKEYF.HOTKEYF_CONTROL;
//                    }
//                    if (txtShortcut.Text.IndexOf("Alt") >= 0)
//                    {
//                        HKM |= HOTKEYF.HOTKEYF_ALT;
//                    }
//                }
//                int iconIdx;
//                if (!int.TryParse(txtIconIndex.Text, out iconIdx))
//                    iconIdx = 0;

//                string lnk = System.IO.Path.Combine(txtCreateInPath.Text, txtTitle.Text + ".lnk");
//                if (System.IO.File.Exists(lnk) && MessageBox.Show(this, "Link already exists. Do you want to overwrite ?", "Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No) return;

//                ShellLinksManager.CreateShortcut(txtTitle.Text, txtTarget.Text, txtCreateInPath.Text, txtWorkingDir.Text,
//                    txtIconPath.Text, iconIdx,
//                    HK, HKM, (SW)(cbShowCmd.SelectedIndex + 1), txtArgument.Text, txtDescription.Text);

//                MessageBox.Show(this, "Link created successfully !", "Create link", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(this, "Error:" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void btnClose_Click(Object eventSender, EventArgs eventArgs)
//        {
//            this.Close();
//        }

//        private void ShortCut_Load(Object eventSender, EventArgs eventArgs)
//        {
//        }

//        private string GetHK(byte HotKey, HOTKEYF Modifier)
//        {
//            string result = String.Empty;
//            if ((Modifier & HOTKEYF.HOTKEYF_ALT) == HOTKEYF.HOTKEYF_ALT)
//            {
//                result = "Alt + ";
//            }
//            if ((Modifier & HOTKEYF.HOTKEYF_CONTROL) == HOTKEYF.HOTKEYF_CONTROL)
//            {
//                result = result + "Ctrl + ";
//            }
//            if ((Modifier & HOTKEYF.HOTKEYF_SHIFT) == HOTKEYF.HOTKEYF_SHIFT)
//            {
//                result = result + "Shift + ";
//            }
//            return result + "KeyAscii(" + HotKey.ToString() + ")";
//        }

//        private void Raccourci_KeyUp(Object eventSender, KeyEventArgs eventArgs)
//        {
//            int KeyCode = (int)eventArgs.KeyCode;
//            if ((KeyCode == ((int)Keys.Left)) || (KeyCode == ((int)Keys.Right)) || (KeyCode == ((int)Keys.Up)) || (KeyCode == ((int)Keys.Down)) || (KeyCode == ((int)Keys.Escape)) || (KeyCode == ((int)Keys.Return)) || (KeyCode == ((int)Keys.Menu)) || (KeyCode == ((int)Keys.ControlKey)) || (KeyCode == ((int)Keys.ShiftKey)))
//            {
//                return;
//            }

//            txtShortcut.Text = "";
//            if ((KeyCode == ((int)Keys.Delete)) || (KeyCode == ((int)Keys.Back)))
//            {
//                return;
//            }
//            if ((eventArgs.Modifiers & Keys.Alt) == Keys.Alt)
//            {
//                txtShortcut.Text = "Alt + ";
//            }
//            if ((eventArgs.Modifiers & Keys.Control) == Keys.Control)
//            {
//                txtShortcut.Text = txtShortcut.Text + "Ctrl + ";
//            }
//            if ((eventArgs.Modifiers & Keys.Shift) == Keys.Shift)
//            {
//                txtShortcut.Text = txtShortcut.Text + "Shift + ";
//            }
//            txtShortcut.Text = txtShortcut.Text + "KeyAscii(" + KeyCode.ToString() + ")";
//        }

//        [STAThread]
//        static void Main()
//        {
//            Application.Run(new frmMain());
//        }
//    }
#endregion