using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace MyCodeLibrary
{
    /// <summary>
    /// Класс реализует операции с окнами
    /// </summary>
    public class CWindowProcessor
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);
     
        private const int WM_COMMAND = 0x111;
        private const int MIN_ALL = 419;
        private const int MIN_ALL_UNDO = 416;

        /// <summary>
        /// NT-Свернуть все окна и показать рабочий стол.
        /// </summary>
        public static void СвернутьВсеОкна()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
            
            return;
        }

        /// <summary>
        /// NT-Развернуть все окна на рабочий стол.
        /// </summary>
        public static void РазвернутьВсеОкна()
        {
            IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
            SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL_UNDO, IntPtr.Zero);
            
            return;
        }

        #region Console titlebar CloseButton disable

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, int bRevert);

        [DllImport("user32.dll")]
        private static extern Boolean DeleteMenu(IntPtr hMenu, int uPosition, int uFlags);

        /// <summary>
        /// Выключить кнопку Close на титлебаре окна консольного приложения
        /// </summary>
        /// <remarks>
        /// Это выключает кнопку c крестиком в заголовке окна, и пункт системного меню.
        /// </remarks>
        public static void DisableConsoleCloseButton()
        {
            //1 получить описатель окна приложения
            Process p = Process.GetCurrentProcess();
            IntPtr hwnd = p.MainWindowHandle;
            //2 получить служебное меню окна
            IntPtr hMenu = GetSystemMenu(hwnd, 0);
            //3 выключить пункт Закрыть
            DeleteMenu(hMenu, 6, 1024);

            return;
        }
        #endregion

    }
}

