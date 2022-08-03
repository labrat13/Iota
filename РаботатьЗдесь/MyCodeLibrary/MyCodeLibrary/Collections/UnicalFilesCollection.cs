using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace MyCodeLibrary.Collections
{
    /// <summary>
    /// Коллекция, в которой нет одинаковых файлов. Дубликаты не добавляются в коллекцию.
    /// </summary>
    public class UnicalFilesCollection
    {
        private Dictionary<String, UnicalFileInfo> m_namesDict;

        private List<UnicalFileInfo> m_listUfi;

        public UnicalFilesCollection()
        {
            m_namesDict = new Dictionary<string, UnicalFileInfo>();
            m_listUfi = new List<UnicalFileInfo>();
        }

        /// <summary>
        /// NT-add new file to collection
        /// </summary>
        /// <param name="filepath"></param>
        public void AddFile(String filepath)
        {
            //1 find file by pathname. If found, no actions more
            if (m_namesDict.ContainsKey(filepath))
                return;
            //2 find file by file info. If found, add link to old file only
            UnicalFileInfo ufi = new UnicalFileInfo(filepath);
            foreach (UnicalFileInfo u in m_namesDict.Values)
            {
                if (ufi.isEqual(u))
                {
                    m_namesDict.Add(filepath, u);
                    return;
                }
            }
            //3 add file to dictionary with source pathname and increment ufiCount
            m_namesDict.Add(filepath, ufi);
            m_listUfi.Add(ufi);
            return;
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public int getUnicalFileCount()
        {
            return m_listUfi.Count;
        }
        /// <summary>
        /// NT-
        /// </summary>
        /// <returns></returns>
        public List<String> getUnicalFiles()
        {
            List<String> files = new List<string>();
            foreach (UnicalFileInfo ufi in m_listUfi)
            {
                files.Add(ufi.m_PathName);
            }
            return files;
        }
    }

    /// <summary>
    /// Информация о уникальном файле коллекции
    /// </summary>
    internal class UnicalFileInfo
    {
        public String m_PathName;
        public String m_Hash;
        public Int64 m_Size;
        /// <summary>
        /// NT- сравнить файлы по содержимому
        /// </summary>
        /// <param name="anotherFilePath"></param>
        /// <returns></returns>
        public bool IsEqualContent(String anotherFilePath)
        {
            FileStream fs1 = new FileStream(m_PathName, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream fs2 = new FileStream(anotherFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return CompareStreams(fs1, fs2);
        }
        /// <summary>
        /// NT-Проверить что записи тождественны
        /// </summary>
        /// <param name="ufi"></param>
        /// <returns></returns>
        public bool isEqual(UnicalFileInfo ufi)
        {
            //если тот же файл, возвращаем труе
            if (String.Equals(m_PathName, ufi.m_PathName))
                return true;
            if (m_Size != ufi.m_Size)
                return false;
            if (String.Equals(m_Hash, ufi.m_Hash))
                return true;
            else return false;
        }

        /// <summary>
        /// NT- Конструктор
        /// </summary>
        /// <param name="filepath"></param>
        public UnicalFileInfo(String filepath)
        {
            m_PathName = filepath;
            m_Hash = calculateHash(filepath);
            FileInfo fi = new FileInfo(filepath);
            m_Size = fi.Length;
            fi = null;
            return;
        }

        /// <summary>
        /// Вычислить MD5 хэш файла
        /// </summary>
        /// <param name="filepath">путь к файлу</param>
        /// <returns></returns>
        private string calculateHash(string filepath)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read));
            //make string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// NT-Проверить что содержимое потоков одинаковое
        /// </summary>
        /// <param name="zs">Поток 1</param>
        /// <param name="fs">Поток 2</param>
        /// <returns></returns>
        private static bool CompareStreams(Stream zs, Stream fs)
        {
            Byte[] buf = new byte[4096];
            Byte[] buz = new byte[4096];
            int rdf = 0;
            int rdz = 0;
            while (true)
            {
                rdf = fs.Read(buf, 0, 4096);
                rdz = zs.Read(buz, 0, 4096);
                //если длины не равны, то и файлы не равны
                if (rdf != rdz) return false;
                //content
                for (int i = 0; i < rdf; i++)
                    if (buf[i] != buz[i])
                        return false;

                //конец файла
                if (rdf != 4096) break;
            }
            // вернуть результат
            return true;
        }
    }



}


