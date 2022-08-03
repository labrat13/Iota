using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary.TextProcessing
{

    /// <summary>
    /// Определять кодировку текстового файла: UTF-8 или windows-1251
    /// </summary>
    public static class Utf8FileEncodingDetector
    {
        /// <summary>
        /// NFT- детектор кодировки: win1251 или UTF-8
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        /// <remarks>
        /// Символы в UTF-8 кодируются последовательностями длиной от 1 до 4 байт (октетов).
        /// Вот в таком формате:
        /// U+000000-U+00007F: 0xxxxxxx (ANSI)
        /// U+000080-U+0007FF: 110xxxxx 10xxxxxx (сюда входит кириллица)
        /// U+000800-U+00FFFF: 1110xxxx 10xxxxxx 10xxxxxx
        /// U+010000-U+10FFFF: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
        /// По маске первого октета определяется общее число октет в последовательности, 
        /// а затем они проверяются на соответствие маске 10xxxxxx. 
        /// Если какой-то байт не соответствует маске, значит кодировка отличная от UTF-8 (в моем случае win1251).
        /// Код, конечно, громоздкий, но для демонстрации самого алгоритма вполне достаточный
        /// </remarks>
        /// <example>
        /// <code>
        /// //работало на первом тесте
        /// Encoding e1 = Utf8Detector.getFileEncoding("C:\\Temp\\pitest.xml");
        /// Encoding e2 = Utf8Detector.getFileEncoding("C:\\Temp\\test.txt");
        /// </code>
        /// </example>
        public static Encoding getFileEncoding(string filepath)
        {
            BinaryReader instr = new BinaryReader(File.OpenRead(filepath));
            byte[] data = instr.ReadBytes(1024);
            instr.Close();

            // определяем BOM (EF BB BF)
            if (data.Length > 2 && data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf)
                return Encoding.UTF8;

            //выполняем анализ символов текста
            int i = 0;
            while (i < data.Length - 1)
            {
                if (data[i] > 0x7f)
                { // не ANSI-символ
                    if ((data[i] >> 5) == 6)
                    {
                        if ((i > data.Length - 2) || ((data[i + 1] >> 6) != 2))
                            return Encoding.GetEncoding(1251);
                        else
                            i++;
                    }
                    else if ((data[i] >> 4) == 14)
                    {
                        if ((i > data.Length - 3) || ((data[i + 1] >> 6) != 2) || ((data[i + 2] >> 6) != 2))
                            return Encoding.GetEncoding(1251);
                        else
                            i += 2;
                    }
                    else if ((data[i] >> 3) == 30)
                    {
                        if ((i > data.Length - 4) || ((data[i + 1] >> 6) != 2) || ((data[i + 2] >> 6) != 2) || ((data[i + 3] >> 6) != 2))
                            return Encoding.GetEncoding(1251);
                        else
                            i += 3;
                    }
                    else
                    {
                        return Encoding.GetEncoding(1251);
                    }
                }
                i++;
            }

            return Encoding.UTF8;
        }

    }//end class
}
