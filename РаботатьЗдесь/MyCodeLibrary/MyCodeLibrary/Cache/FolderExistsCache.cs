using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary
{
    /// <summary>
    /// Класс кеширует функцию проверки существования каталога с указанным путем.
    /// Применять только для проверки наличия каталога, 
    /// когда он только может создаваться, но не удаляться в процессе существования объекта класса.
    /// </summary>
    /// <remarks>
    /// Это небольшая оптимизация для случая, когда надо создавать много подкаталогов и файлов в них.
    /// Повышает скорость за счет расхода памяти. 
    /// </remarks>
    public class FolderExistsCache
    {
        /// <summary>
        /// Словарь путей каталогов
        /// </summary>
        private Dictionary<String, bool> m_dict;
        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderExistsCache()
        {
            m_dict = new Dictionary<string, bool>();
        }
        /// <summary>
        /// RT-Проверить существование каталога
        /// </summary>
        /// <param name="folderPath">Путь к каталогу</param>
        /// <returns>Возвращает True или False</returns>
        public bool isFolderExists(String folderPath)
        {
            //если папка есть в словаре, значит, она существует
            if (m_dict.ContainsKey(folderPath))
                return true;
            else //check folder exists
            {
                if (Directory.Exists(folderPath))
                {
                    m_dict.Add(folderPath, true);
                    return true;
                }
                else return false;
            }
        }
        /// <summary>
        /// RT-Создать каталог если он не существует
        /// </summary>
        /// <param name="folderPath">Путь к каталогу</param>
        public void CreateIfNotExists(String folderPath)
        {
            //если папка есть в словаре, значит она существует или впервые запрашивается
            if (m_dict.ContainsKey(folderPath))
                return;
            else //check folder exists
            {
                if (!Directory.Exists(folderPath))
                {
                    //создать каталог без индексации
                    DirectoryInfo di = Directory.CreateDirectory(folderPath);
                    di.Attributes = di.Attributes & FileAttributes.NotContentIndexed;
                    //добавить путь в словарь
                    m_dict.Add(folderPath, true);
                    return;
                }
                else
                {
                    //add existing directory to dictionary
                    m_dict.Add(folderPath, true);
                    return;
                } 
            }
        }

    }
}
