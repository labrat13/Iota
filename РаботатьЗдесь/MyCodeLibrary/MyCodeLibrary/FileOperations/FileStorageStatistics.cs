using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary.FileOperations
{
    /// <summary>
    /// Подсчитывает статистику для каталога - размер каталога и число файлов и подкаталогов
    /// </summary>
    public class FileStorageStatistics
    {
        /// <summary>
        /// Число файлов
        /// </summary>
        private int m_FilesCount;
        
        /// <summary>
        /// Число подкаталогов
        /// </summary>
        private int m_FoldersCount;
        
        /// <summary>
        /// Размер каталога в байтах
        /// </summary>
        private long m_FolderSize;


        /// <summary>
        /// Создает данные статистики
        /// </summary>
        /// <param name="folderPath">Путь к обрабатываемому каталогу</param>
        public FileStorageStatistics(string folderPath)
        {
            m_FilesCount = 0;
            m_FoldersCount = 0;
            m_FolderSize = 0;
            DirectoryInfo d = new DirectoryInfo(folderPath);
            getDirStat(d);
        }



        /// <summary>
        /// Число подкаталогов
        /// </summary>
        public int FoldersCount
        {
            get { return m_FoldersCount; }
            set { m_FoldersCount = value; }
        }

        /// <summary>
        /// Число файлов
        /// </summary>
        public int FilesCount
        {
            get { return m_FilesCount; }
            set { m_FilesCount = value; }
        }
        /// <summary>
        /// Размер каталога в байтах
        /// </summary>
        public long FolderSize
        {
            get { return m_FolderSize; }
            set { m_FolderSize = value; }
        }

        /// <summary>
        /// NT-Рекурсивно проходим по каталогам и собираем статистику
        /// </summary>
        /// <param name="d">DirectoryInfo object of parent directory</param>
        private void getDirStat(DirectoryInfo d)
        {
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            m_FilesCount += fis.Length;
            foreach (FileInfo fi in fis)
            {
                m_FolderSize += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            m_FoldersCount += dis.Length;
            foreach (DirectoryInfo di in dis)
            {
                getDirStat(di); //считаем размеры в подкаталоах
            }
            
            return; 
        }

        public override string ToString()
        {
            return String.Format("Stat: Size={0}bytes; Folders={1}; Files={2};", m_FolderSize, m_FoldersCount, m_FilesCount);
        }

    }
}

