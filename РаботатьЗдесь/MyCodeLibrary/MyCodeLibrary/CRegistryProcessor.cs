using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using MyCodeLibrary.SystemInfo;

namespace MyCodeLibrary
{
    /// <summary>
    /// NT-
    /// </summary>
    public class CRegistryProcessor
    {
        /// <summary>
        /// HKEY
        /// </summary>
        public enum HKEY
        {
            /// <summary>
            /// CLASSES_ROOT
            /// </summary>
            CLASSES_ROOT,
            /// <summary>
            /// CLASSES_USER
            /// </summary>
            CURRENT_USER,
            /// <summary>
            /// LOCAL_MACHINE
            /// </summary>
            LOCAL_MACHINE,
            /// <summary>
            /// USERS
            /// </summary>
            USERS,
            /// <summary>
            /// PERFORMANCE_DATA
            /// </summary>
            PERFORMANCE_DATA,
            /// <summary>
            /// CURRENT_CONFIG
            /// </summary>
            CURRENT_CONFIG
        }

        /// <summary>
        /// NT-Gets string value of a value in the registry.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String getStringValue(HKEY hkey, String key, String value)
        {
            String text;
            if (SystemInfoProcessor.Is64BitOS)
            {
                text = getKeyValue(getBaseKey(hkey, true), key, value).ToString();
                if (String.IsNullOrEmpty(text))
                {
                    text = getKeyValue(getBaseKey(hkey, false), key, value).ToString();
                }
            }
            else
            {
                text = getKeyValue(getBaseKey(hkey, false), key, value).ToString();
            }
            return text;
        }

        /// <summary>
        /// NT-Gets byte value of a value in the registry.
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] getByteValue(HKEY hkey, String key, String value)
        {
            var byteobj = getKeyValue(getBaseKey(hkey, false), key, value) as byte[];
            if ((byteobj == null) || (byteobj.Length == 0))
            {
                byteobj = getKeyValue(getBaseKey(hkey, true), key, value) as byte[];
            }
            return byteobj;
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static object getKeyValue(RegistryKey hkey, String key, String value)
        {
            if (key == null) return null;
            return hkey.OpenSubKey(key).GetValue(value);
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <param name="hkey"></param>
        /// <param name="get64"></param>
        /// <returns></returns>
        static RegistryKey getBaseKey(HKEY hkey, Boolean get64)
        {
            switch (hkey)
            {
                case HKEY.CLASSES_ROOT:
                    if (get64) return getBaseKey64(RegistryHive.ClassesRoot);
                    return getBaseKey32(RegistryHive.ClassesRoot);
                case HKEY.CURRENT_USER:
                    if (get64) return getBaseKey64(RegistryHive.CurrentUser);
                    return getBaseKey32(RegistryHive.CurrentUser);
                case HKEY.LOCAL_MACHINE:
                    if (get64) return getBaseKey64(RegistryHive.LocalMachine);
                    return getBaseKey32(RegistryHive.LocalMachine);
                case HKEY.USERS:
                    if (get64) return getBaseKey64(RegistryHive.Users);
                    return getBaseKey32(RegistryHive.Users);
                case HKEY.PERFORMANCE_DATA:
                    if (get64) return getBaseKey64(RegistryHive.PerformanceData);
                    return getBaseKey32(RegistryHive.PerformanceData);
                case HKEY.CURRENT_CONFIG:
                    if (get64) return getBaseKey64(RegistryHive.CurrentConfig);
                    return getBaseKey32(RegistryHive.CurrentConfig);
            }

            return null;
        }
        /// <summary>
        /// NR-
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        static RegistryKey getBaseKey32(RegistryHive hive)
        {
            return RegistryKey.OpenRemoteBaseKey(hive, String.Empty);
            //return RegistryKey.OpenBaseKey(hive, RegistryView.Registry32);
        }

        /// <summary>
        /// NR-
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        static RegistryKey getBaseKey64(RegistryHive hive)
        {

            return RegistryKey.OpenRemoteBaseKey(hive, String.Empty);
            //return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64); 
        }


    }
}
