using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace MyCodeLibrary.FileOperations
{
    /// <summary>
    /// Использует Windows Shell подсистему для выполнения файловых операций с возможностью их отмены пользователем.
    /// + Функция очистки корзины 
    /// </summary>
    public class ShellFileOperations
    {
        #region Старая версия кода
        //private const int FO_MOVE = 1;
        //private const int FO_COPY = 2;
        //private const int FO_DELETE = 3;

        //private const int FOF_MULTIDESTFILES = 1;
        //private const int FOF_NOCONFIRMMKDIR = 512;
        //private const int FOF_NOERRORUI = 1024;
        //private const int FOF_SILENT = 4;
        //private const int FOF_WANTNUKEWARNING = 0x4000;     // Windows 2000 and later
        //private const int FOF_ALLOWUNDO = 0x0040;           // Preserve undo information, if possible. 
        //private const int FOF_NOCONFIRMATION = 0x0010;      // Show no confirmation dialog box to the user

        //// Struct which contains information that the SHFileOperation function uses to perform file operations. 
        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        //public struct SHFILEOPSTRUCT
        //{
        //    public IntPtr hwnd;
        //    [MarshalAs(UnmanagedType.U4)]
        //    public int wFunc;
        //    public string pFrom;
        //    public string pTo;
        //    public short fFlags;
        //    [MarshalAs(UnmanagedType.Bool)]
        //    public bool fAnyOperationsAborted;
        //    public IntPtr hNameMappings;
        //    public string lpszProgressTitle;
        //}

        ///// <summary>
        ///// Собственно файловая операция
        ///// </summary>
        ///// <param name="opcode">Код операции, один из: FO_COPY FO_MOVE FO_DELETE</param>
        ///// <param name="srcPath">Абсолютный путь к исходному файлу или каталогу</param>
        ///// <param name="dstPath">Асолютный путь к конечному файлу или каталогу, null если не нужен</param>
        //private static int shellFileOp(ref bool Cancelled, int opcode, string srcPath, string dstPath)
        //{
        //    //fileOp = SHFILEOPSTRUCTW()
        //    SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
        //    //srcPathWc = ctypes.c_wchar_p(srcPath + u"\0")
        //    //fileOp.hwnd = 0
        //    //fileOp.wFunc = opcode
        //    //fileOp.pFrom = srcPathWc
        //    fileop.wFunc = opcode;
        //    fileop.pFrom = srcPath + '\0' + '\0';
        //    //if dstPath is not None:
        //    //    dstDir = os.path.dirname(dstPath)
        //    if (!String.IsNullOrEmpty(dstPath))
        //    {
        //        String dstDir = Path.GetDirectoryName(dstPath);
        //        //    if not os.path.exists(pathEnc(dstDir)):
        //        //        os.makedirs(dstDir)
        //        //создать папку - тут надо повозиться - оно почему-то плохо тут работает
        //        DirectoryInfo di = new DirectoryInfo(dstDir);//не создает каталог почему-то
        //        if (!di.Exists)
        //        {
        //            //di.Create();//не работает
        //            Directory.CreateDirectory(di.FullName);
        //            di.Refresh();
        //            //установить атрибуты, запрещающие индексацию, архивацию и прочее в том же духе
        //            di.Attributes = (di.Attributes | FileAttributes.NotContentIndexed);
        //        }

        //        fileop.pTo = dstPath + '\0' + '\0';
        //    }
        //    else fileop.pTo = null;

        //    fileop.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_MULTIDESTFILES | FOF_NOCONFIRMMKDIR | FOF_WANTNUKEWARNING;

        //    int result = SHFileOperation(ref fileop);
        //    //вернуть флаг что операция была прервана пользователем через ГУЙ
        //    Cancelled = fileop.fAnyOperationsAborted;

        //    //Возвращаем код ошибки для последующей обработки
        //    return result;
        //}

        //Собственно, само объявление функции
        //[DllImport("shell32.dll", CharSet = CharSet.Auto)]
        //static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        #endregion


        /// <summary>
        /// Перечисление FileOperationType для функции определяет, что делать с файлом.
        /// </summary>
        public enum FileOperationType : uint
        {
            /// <summary>
            /// Переместить файл
            /// </summary>
            FO_MOVE = 0x0001,
            /// <summary>
            /// Копировать файл
            /// </summary>
            FO_COPY = 0x0002,
            /// <summary>
            /// Удалить (в корзину или безвозвратно) файл
            /// </summary>
            FO_DELETE = 0x0003,
            /// <summary>
            /// Переименовать файл
            /// </summary>
            FO_RENAME = 0x0004,
        }

        /// <summary>
        /// Необходимые флаги для функции SHFileOperation.
        /// </summary>
        [Flags]
        public enum FileOperationFlags : ushort
        {
            /// <summary>
            /// 
            /// </summary>
            FOF_MULTIDESTFILES = 0x0001,
            /// <summary>
            /// Не показывать диалог с индикатором прогресса в течение процесса удаления.
            /// </summary>
            FOF_SILENT = 0x0004,
            /// <summary>
            /// Не спрашивать у пользователя подтверждения удаления.
            /// Show no confirmation dialog box to the user.
            /// </summary>
            FOF_NOCONFIRMATION = 0x0010,
            /// <summary>
            /// Удалить файл в корзину. Этот флаг нужен для того, чтобы файл был удален именно в корзину.
            /// </summary>
            FOF_ALLOWUNDO = 0x0040,
            /// <summary>
            /// Не показывать, какие файлы и\или папки удаляются, в диалоге с индикатором прогресса.
            /// </summary>
            FOF_SIMPLEPROGRESS = 0x0100,
            /// <summary>
            /// 
            /// </summary>
            FOF_NOCONFIRMMKDIR = 0x0200,
            /// <summary>
            /// Не показывать сообщения об ошибках, которые могут возникнуть в течение процесса.
            /// </summary>
            FOF_NOERRORUI = 0x0400,
            /// <summary>
            /// Предупреждать, что удаляемые файлы слишком велики для помещения в корзину и поэтому
            /// будут удалены безвозвратно.
            /// </summary>
            /// <remarks>
            /// // Windows 2000 and later
            /// </remarks>
            FOF_WANTNUKEWARNING = 0x4000,
        }

        /// <summary>
        /// SHFILEOPSTRUCT для функции. Здесь два объявления самой функции и этой структуры,
        /// используемые в зависимости от разрядности системы
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        private struct SHFILEOPSTRUCT_x86
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT_x64
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }


                
        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
        private static extern int SHFileOperation_x86(ref SHFILEOPSTRUCT_x86 FileOp);
 
        [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "SHFileOperation")]
        private static extern int SHFileOperation_x64(ref SHFILEOPSTRUCT_x64 FileOp);
        
        /// <summary>
        /// 32-битная или 64-битная система?
        /// </summary>
        /// <returns></returns>
        public static bool IsWOW64Process
        {
            get { return (IntPtr.Size == 8); }
        }
        /// <summary>
        /// Собственно файловая операция 
        /// Тут можно указать много всяких параметров.
        /// </summary>
        /// <param name="Cancelled">Флаг что операция была отменена пользователем</param>
        /// <param name="opCode">Код операции, один из: FO_COPY FO_MOVE FO_DELETE</param>
        /// <param name="opFlags">Флаги поведения операции</param>
        /// <param name="srcPath">Абсолютный путь к исходному файлу или каталогу</param>
        /// <param name="dstPath">Асолютный путь к конечному файлу или каталогу, null если не нужен</param>
        public static int shellFileOp2(ref bool Cancelled, FileOperationType opCode, FileOperationFlags opFlags, String srcPath, String dstPath)
        {
            int result = 0;
            //подготовить конечную папку если она указана
            String destPathT = null;
            if (!String.IsNullOrEmpty(dstPath))
            {
                String dstDir = Path.GetDirectoryName(dstPath);
                //создать папку - тут надо повозиться
                DirectoryInfo di = new DirectoryInfo(dstDir);
                if (!di.Exists)
                {
                    //di.Create();//не работает
                    Directory.CreateDirectory(di.FullName);
                    di.Refresh();
                    //установить атрибуты, запрещающие индексацию, архивацию и прочее в том же духе
                    di.Attributes = (di.Attributes | FileAttributes.NotContentIndexed);
                }
                destPathT = dstPath + '\0' + '\0';
            }

            //собственно работать
            if (IsWOW64Process)
            {
                SHFILEOPSTRUCT_x64 fs = new SHFILEOPSTRUCT_x64();
                fs.wFunc = opCode;
                fs.fFlags = opFlags;
                fs.pFrom = srcPath + '\0' + '\0';
                fs.pTo = destPathT;
                //fs.hwnd = null;
                //fs.lpszProgressTitle = null;
                result = SHFileOperation_x64(ref fs);
                //вернуть флаг что операция была прервана пользователем через ГУЙ
                Cancelled = fs.fAnyOperationsAborted;
            }
            else
            {
                SHFILEOPSTRUCT_x86 fs = new SHFILEOPSTRUCT_x86();
                fs.wFunc = opCode;
                fs.fFlags = opFlags;
                fs.pFrom = srcPath + '\0' + '\0';
                fs.pTo = destPathT;
                //fs.hwnd = null;
                //fs.lpszProgressTitle = null;
                result = SHFileOperation_x86(ref fs);
                //вернуть флаг что операция была прервана пользователем через ГУЙ
                Cancelled = fs.fAnyOperationsAborted;
            }

            //Возвращаем код ошибки для последующей обработки
            return result;
        }

        /// <summary>
        /// NT-Копировать файл или каталог с показом диалога и возможностью отменить операцию.
        /// </summary>
        /// <param name="srcPath">Путь к исходному файлу</param>
        /// <param name="dstPath">Путь к конечному файлу</param>
        /// <returns>Функция возвращает true, если операция была выполнена успешно, false если операция прервана пользователем.</returns>
        /// <remarks>HTML файлы обрабатываются вместе с принадлежащими им папками ресурсов, если в Проводнике установлен соответствующий флаг.</remarks>
        public static bool CopyFile(string srcPath, string dstPath)
        {
            //Copy file from srcPath to dstPath. dstPath may be overwritten if
            //existing already. dstPath must point to a file, not a directory.
            //If some directories in dstPath do not exist, they are created.
            bool Cancelled = false;
            FileOperationFlags flags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_MULTIDESTFILES | FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_WANTNUKEWARNING;
            int result = shellFileOp2(ref Cancelled, FileOperationType.FO_COPY, flags, srcPath, dstPath);
            if (result != 0)
            {
                throw new IOException(String.Format("Copying from {0} to {1} failed. SHFileOperation returns {2}", srcPath, dstPath, result));
            }
            return !Cancelled;
        }


        /// <summary>
        /// NT-Переместить файл или каталог с показом диалога и возможностью отменить операцию.
        /// </summary>
        /// <param name="srcPath">Путь к исходному файлу</param>
        /// <param name="dstPath">Путь к конечному файлу</param>
        /// <returns>Функция возвращает true, если операция была выполнена успешно, false если операция прервана пользователем.</returns>
        /// <remarks>HTML файлы обрабатываются вместе с принадлежащими им папками ресурсов, если в Проводнике установлен соответствующий флаг.</remarks>
        public static bool MoveFile(string srcPath, string dstPath)
        {
            //Move file from srcPath to dstPath. dstPath may be overwritten if
            //existing already. dstPath must point to a file, not a directory.
            //If some directories in dstPath do not exist, they are created.
            bool Cancelled = false;
            FileOperationFlags flags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_MULTIDESTFILES | FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_WANTNUKEWARNING;
            int result = shellFileOp2(ref Cancelled, FileOperationType.FO_MOVE, flags, srcPath, dstPath);
            if (result != 0)
            {
                throw new IOException(String.Format("Moving from {0} to {1} failed. SHFileOperation returns {2}", srcPath, dstPath, result));
            }
            return !Cancelled;
        }
        /// <summary>
        /// NT-Удалить файл или каталог с показом диалога и возможностью отменить операцию.
        /// </summary>
        /// <param name="srcPath">Путь к удаляемому файлу</param>
        /// <returns>Функция возвращает true, если операция была выполнена успешно, false если операция прервана пользователем.</returns>
        /// <remarks>
        /// Файл удаляется в Корзину Windows.
        /// HTML файлы обрабатываются вместе с принадлежащими им папками ресурсов, если в Проводнике установлен соответствующий флаг.
        /// </remarks>
        public static bool DeleteFile(string srcPath)
        {
            //Delete file or directory  path.
            FileOperationFlags flags = FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_MULTIDESTFILES | FileOperationFlags.FOF_NOCONFIRMMKDIR | FileOperationFlags.FOF_WANTNUKEWARNING;
            return DeleteFile(srcPath, flags);
        }

        /// <summary>
        /// NT-Удалить файл или каталог с показом диалога и возможностью отменить операцию.
        /// </summary>
        /// <param name="srcPath">Путь к удаляемому файлу</param>
        /// <param name="flags">Флаги поведения операции</param>
        /// <returns>Функция возвращает true, если операция была выполнена успешно, false если операция прервана пользователем.</returns>
        /// <remarks>
        /// HTML файлы обрабатываются вместе с принадлежащими им папками ресурсов, если в Проводнике установлен соответствующий флаг.
        /// </remarks>
        public static bool DeleteFile(string srcPath, FileOperationFlags flags)
        {
            //Delete file or directory  path.
            bool Cancelled = false;
            int result = shellFileOp2(ref Cancelled, FileOperationType.FO_DELETE, flags, srcPath, null);
            if (result != 0)
            {
                throw new IOException(String.Format("Deleting {0} failed. SHFileOperation returns {1}", srcPath, result));
            }
            return !Cancelled;
        }

        /// <summary>
        /// Устаревшая функция!
        /// Переместить файл или папку в корзину
        /// </summary>
        /// <param name="path">Путь к папке или файлу</param>
        public static void DeleteFileOrFolder(string path)
        {
            //Возвращает 0 если успешно или если пользователь отменил операцию, не 0 при любой ошибке
            //Приложение нормально просто проверяет результат на 0  
            //Значение поля fAnyOperationsAborted в SHFILEOPSTRUCT сигнализирует, что пользователь отменил операцию. 
            //Если не проверять fAnyOperationsAborted, то нельзя узнать, выполнена ли операция, или пользователь ее отменил. 

            //SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
            //fileop.wFunc = FO_DELETE;
            //fileop.pFrom = path + '\0' + '\0';
            //fileop.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
            //SHFileOperation(ref fileop);

            DeleteFile(path, FileOperationFlags.FOF_ALLOWUNDO | FileOperationFlags.FOF_NOCONFIRMATION);
            return;
        }

#region RecycleBin functions

        /// <summary>
        /// Флаги для API-функции SHEmptyRecycleBin
        /// </summary>
        [Flags]
        public enum EmptyRecycleBinFlags : uint
        {
            /// <summary>
            /// Нет флагов (=0)
            /// </summary>
            None = 0,
            /// <summary>
            /// Не отображать диалог с уведомлением об удалении объектов
            /// </summary>
            SHERB_NOCONFIRMATION = 0x00000001,
            /// <summary>
            /// Не отображать диалог с индикатором прогресса
            /// </summary>
            SHERB_NOPROGRESSUI = 0x00000002,
            /// <summary>
            /// Когда операция закончится, не пригрывать звук
            /// </summary>
            SHERB_NOSOUND = 0x00000004,
        }

        //API-функция очистки корзины
        [DllImport("shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hWnd, string pszRootPath, uint dwFlags);

        /// <summary>
        /// NFT-Очистить корзину
        /// </summary>
        /// <param name="hWnd"> handle to the parent window of any dialog boxes that might be displayed during the operation. This parameter can be NULL.</param>
        /// <param name="volumeRootPath">
        /// The address of a null-terminated string of maximum length MAX_PATH that 
        /// contains the path of the root drive on which the Recycle Bin is located. 
        /// This parameter can contain the address of a string formatted with the drive, 
        /// folder, and subfolder names, for example c:\windows\system\, etc. 
        /// It can also contain an empty string or NULL. 
        /// If this value is an empty string or NULL, all Recycle Bins on all drives will be emptied.</param>
        /// <param name="flags">Флаги поведения операции</param>
        /// <returns>
        /// Возвращает True при успехе или False при ошибке.
        /// </returns>
        public static bool EmptyRecycleBin(IntPtr hWnd, String volumeRootPath, EmptyRecycleBinFlags flags)
        {
            bool result = true; 
            try
            {
                int res = SHEmptyRecycleBin(hWnd, volumeRootPath, (uint)flags);
                result = (res == 0);   
            }
            catch
            {
                result = false;
            }
            return result;
        }

#endregion

    }
}
