using System;
using System.Runtime.InteropServices;

namespace MyCodeLibrary
{

    /// <summary>
    /// NT-Запретить засыпание компьютера, гашение дисплея и запуск скринсейвера для Windows 7 и более поздних ОС.
    /// </summary>
    /// <remarks>
    /// Не тестировался!!!
    /// </remarks>
    public static class PreventSleepWin7
    {
        #region prevent screensaver, display dimming and automatically sleeping

        // Availablity Request Structures
        // Note:  Windows defines the POWER_REQUEST_CONTEXT structure with an
        // internal union of SimpleReasonString and Detailed information.
        // To avoid runtime interop issues, this version of 
        // POWER_REQUEST_CONTEXT only supports SimpleReasonString.  
        // To use the detailed information,
        // define the PowerCreateRequest function with the first 
        // parameter of type POWER_REQUEST_CONTEXT_DETAILED.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT
        {
            public UInt32 Version;
            public UInt32 Flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string SimpleReasonString;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PowerRequestContextDetailedInformation
        {
            public IntPtr LocalizedReasonModule;
            public UInt32 LocalizedReasonId;
            public UInt32 ReasonStringCount;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string[] ReasonStrings;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT_DETAILED
        {
            public UInt32 Version;
            public UInt32 Flags;
            public PowerRequestContextDetailedInformation DetailedInformation;
        }

        // Availablity Request Enumerations and Constants
        enum PowerRequestType
        {
            PowerRequestDisplayRequired = 0,
            PowerRequestSystemRequired,
            PowerRequestAwayModeRequired,
            PowerRequestMaximum
        }

        const int POWER_REQUEST_CONTEXT_VERSION = 0;
        const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;
        const int POWER_REQUEST_CONTEXT_DETAILED_STRING = 0x2;



        private static POWER_REQUEST_CONTEXT _PowerRequestContext;
        private static IntPtr _PowerRequest; //HANDLE

        // Availability Request Functions
        [DllImport("kernel32.dll")]
        static extern IntPtr PowerCreateRequest(ref POWER_REQUEST_CONTEXT Context);

        [DllImport("kernel32.dll")]
        static extern bool PowerSetRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        [DllImport("kernel32.dll")]
        static extern bool PowerClearRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern int CloseHandle(IntPtr hObject);

        #endregion


        /// <summary>
        /// Prevent screensaver, display dimming and power saving. This function wraps PInvokes on Win32 API. 
        /// </summary>
        /// <param name="enableConstantDisplayAndPower">True to get a constant display and power - False to clear the settings</param>
        /// <param name="reasonString">your reason for changing the power settings;</param>
        public static void EnableConstantDisplayAndPower(bool enableConstantDisplayAndPower, string reasonString)
        {
            if (enableConstantDisplayAndPower)
            {
                // Set up the diagnostic string
                _PowerRequestContext.Version = POWER_REQUEST_CONTEXT_VERSION;
                _PowerRequestContext.Flags = POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                _PowerRequestContext.SimpleReasonString = reasonString;

                // Create the request, get a handle
                _PowerRequest = PowerCreateRequest(ref _PowerRequestContext);
                //  if (_PowerRequest..IsInvalid)
                //{
                //    throw new InvalidOperationException(
                //       $"Could not create power availability request: {Win32Error.GetLastError()}");
                //}
                // Set the request
                PowerSetRequest(_PowerRequest, PowerRequestType.PowerRequestSystemRequired);//TODO: надо бы обработать возвращаемое значение функции!
                PowerSetRequest(_PowerRequest, PowerRequestType.PowerRequestDisplayRequired);//TODO: надо бы обработать возвращаемое значение функции!
            }
            else
            {
                // Clear the request
                PowerClearRequest(_PowerRequest, PowerRequestType.PowerRequestSystemRequired);//TODO: надо бы обработать возвращаемое значение функции!
                PowerClearRequest(_PowerRequest, PowerRequestType.PowerRequestDisplayRequired);//TODO: надо бы обработать возвращаемое значение функции!

                CloseHandle(_PowerRequest);
            }
        }




    }
}
