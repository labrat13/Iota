using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace MyCodeLibrary.Hash
{
    /// <summary>
    /// Все для вычисления контрольной суммы
    /// </summary>
    public static class MD5Hash
    {
        /// <summary>
        /// Вычислить контрольную сумму
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <returns>Возвращает контрольную сумму как массив байт.</returns>
        public static byte[] Hash(byte[] data)
        {
            System.Security.Cryptography.MD5 m = MD5.Create();
            byte[] result = m.ComputeHash(data);
            return result;
        }
        /// <summary>
        /// Вычислить контрольную сумму
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <returns>Возвращает контрольную сумму как шестнадцатиричную строку </returns>
        public static String HashS(byte[] data)
        {
            byte[] res = Hash(data);

            return BytesToString(res);
        }
        /// <summary>
        /// Вычислить контрольную сумму
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns>Возвращает контрольную сумму как шестнадцатиричную строку</returns>
        public static String HashS(String text)
        {
            byte[] databytes = StringToBytes(text);
            byte[] res = Hash(databytes);
            return BytesToString(res);
        }
        /// <summary>
        /// Конвертировать текст в массив байт
        /// </summary>
        /// <param name="text">Строка текста</param>
        /// <returns>Возвращает массив байт</returns>
        public static byte[] StringToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        /// <summary>
        /// Конвертировать массив байт в текст
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <returns>Строка текста</returns>
        public static string BytesToString(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
        /// <summary>
        /// Конвертировать массив байт в шестнадцатиричную строку
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <returns>Шестнадцатиричная строка длиной две длины входного массива.</returns>
        public static string ToHexString(byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }



    }
}
