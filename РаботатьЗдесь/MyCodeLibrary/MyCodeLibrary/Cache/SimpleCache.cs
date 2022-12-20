using System;
using System.Collections.Generic;
using System.Text;

namespace MyCodeLibrary
{
    /// <summary>
    /// NT- Плохой класс кеша пар Int32-String. 
    /// Когда коллекция переполнилась, он ее просто очищает и все заново можно заполнять.
    /// Это применялось для кеширования запросов БД для получения имени пользователя по ид пользователя.
    /// И на размере кэша 1 млн и 5000 запросов в секунду это было вполне успешно.
    /// </summary>
    public class SimpleCache
    {
        /// <summary>
        /// Максимальный размер кеша
        /// </summary>
        private Int32 m_maxSize;
        /// <summary>
        /// словарь-хранилище данных
        /// </summary>
        private Dictionary<Int32, String> m_Dictionary;

        /// <summary>
        /// Конструктор
        /// </summary>
        public SimpleCache()
        {
            m_maxSize = 16;
            m_Dictionary = new Dictionary<int, string>(m_maxSize);
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="size">Максимальный размер кеша</param>
        public SimpleCache(int size)
        {
            m_maxSize = size;
            m_rnd = new Random();
            m_Dictionary = new Dictionary<int, string>(size / 4);
        }

        /// <summary>
        /// Максимальный размер кеша
        /// </summary>
        public Int32 MaxSize
        {
            get { return m_maxSize; }
            set { m_maxSize = value; }
        }


        public void Add(Int32 userId, string link)
        {
            //если словарь заполнен, выкинуть один случайный элемент из него.
            if (m_Dictionary.Count > this.m_maxSize)
            {
                //Int32 index = m_rnd.Next(CacheMaxSize);
                //Int32 key =  m_userLinkDictionary.Keys.//m_userLinkDictionary.Keys[index];
                //m_userLinkDictionary.Remove(key);
                m_Dictionary.Clear();//удалить все элементы, раз нельзя удалить какой-либо случайный элемент.
            }
            //добавить новый элемент в словарь
            m_Dictionary.Add(userId, link);
            return;
        }
        /// <summary>
        /// NT-Получить значение по идентификатору элемента
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string Get(Int32 userId)
        {
            return m_Dictionary[userId];
        }
        /// <summary>
        /// NT- Возвращает true если коллекция содержит указанный элемент
        /// </summary>
        /// <param name="userId">Идентификатор элемента</param>
        /// <returns></returns>
        public bool IsExists(Int32 userId)
        {
            return m_Dictionary.ContainsKey(userId);
        }
    }
}
