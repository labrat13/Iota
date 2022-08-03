using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary
{
    /// <summary>
    /// Кэш строк по инт32 уникальным идентификаторам
    /// </summary>
    /// <remarks>
    /// Это словарь-кеш строк по Int32 ключам. Когда число элементов превышает установленный предел, весь словарь очищается и может заполняться заново.
    /// Это ускоряет работу программы. Но надо выбирать размер кеша так, чтобы он был достаточно большим - примерно 10..25 процентов от числа уникальных объектов.
    /// Данный словарь использовался в начале функции выборки имени пользователя сайта из БД, поэтому он сильно ускорил работу.
    /// </remarks>
    public class StringIntCache
    {
        /// <summary>
        /// Максимальный размер кеша
        /// </summary>
        private Int32 m_CacheMaxSize;
        /// <summary>
        /// Словарь элементов кеша
        /// </summary>
        private Dictionary<Int32, String> m_userLinkDictionary;
        /// <summary>
        /// NT-Конструктор. Максимальное число элементов кеша = 1000.
        /// </summary>
        public StringIntCache()
        {
            m_CacheMaxSize = 1000;
            m_userLinkDictionary = new Dictionary<int, string>(m_CacheMaxSize / 4);
        }
        /// <summary>
        /// NT-Конструктор
        /// </summary>
        /// <param name="maxSize">Максимальное число элементов кеша</param>
        public StringIntCache(int maxSize)
        {
            m_CacheMaxSize = maxSize;
            m_userLinkDictionary = new Dictionary<int, string>(m_CacheMaxSize / 4);
        }
        /// <summary>
        /// Получить текущее количество элементов в кеше
        /// </summary>
        public int Count
        {
            get { return m_userLinkDictionary.Count; }
        }
        /// <summary>
        /// Получить или установить максимальный размер (емкость) кеша, после которой кеш будет очищен.
        /// </summary>
        public int Size
        {
            get { return this.m_CacheMaxSize; }
            set { this.m_CacheMaxSize = value; }
        }
        /// <summary>
        /// NT-Добавить элемент в кеш
        /// </summary>
        /// <param name="Id">Уникальный идентификатор элемента</param>
        /// <param name="item">Добавляемый элемент кеша</param>
        public void Add(Int32 Id, string item)
        {
            //если словарь заполнен, выкинуть один случайный элемент из него.
            if (m_userLinkDictionary.Count > this.m_CacheMaxSize)
            {
                //из словаря нельзя удалить один случайный элемент, поэтому удалим все элементы, а словарь пусть заполняется заново.
                m_userLinkDictionary.Clear();
            }
            //добавить новый элемент в словарь
            m_userLinkDictionary.Add(Id, item);
            return;
        }
        /// <summary>
        /// NT-Получить элемент кеша по его идентификатору
        /// </summary>
        /// <param name="Id">Уникальный идентификатор элемента</param>
        /// <returns></returns>
        public string Get(Int32 Id)
        {
            return m_userLinkDictionary[Id];
        }
        /// <summary>
        /// NT-Проверить существование в кеше элемента по идентификатору
        /// </summary>
        /// <param name="Id">Уникальный идентификатор элемента</param>
        /// <returns></returns>
        public bool IsExists(Int32 Id)
        {
            return m_userLinkDictionary.ContainsKey(Id);
        }
    }
}
