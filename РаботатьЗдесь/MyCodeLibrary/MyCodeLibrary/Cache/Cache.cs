using System;
using System.Collections.Generic;

namespace MyCodeLibrary
{   
    //Это пример как создавать дженерики коллекции разные
    
    /// <summary>
    /// Это класс кэша, подобный Dictionary, но:
    /// - не содержит KeyValuePair<> енумератора
    /// - не содержит функции удаления объектов
    /// - фиксированный размер кеша задается при создании коллекции и не может быть изменен позднее.
    /// - более новые элементы коллекции вытесняют более старые.
    /// - если предельный размер коллекции задать = 0, получается неограниченный размер коллекции, просто словарь Dictionary.
    /// </summary>
    /// <typeparam name="KeyType">Тип данных ключа</typeparam>
    /// <typeparam name="ValueType">Тип данных значения</typeparam>
    public class Cache<KeyType, ValueType>
    {
        /// <summary>
        /// Максимальное число элементов, хранящихся  в кеше
        /// </summary>
        private int _capacity;
        /// <summary>
        /// Очередь ключей
        /// </summary>
        private Queue<KeyType> _keyQ;
        /// <summary>
        /// Основное хранилище данных
        /// </summary>
        private Dictionary<KeyType, ValueType> _contents;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="initialCapacity">Начальная емкость коллекции</param>
        /// <param name="capacity">Предельная емкость коллекции. Если указать 0, емкость коллекции не ограничена.</param>
        public Cache(int initialCapacity, int capacity)
        {
            _capacity = capacity;
            _contents = new Dictionary<KeyType, ValueType>(initialCapacity);

            if (capacity > 0)
                _keyQ = new Queue<KeyType>(initialCapacity);
        }
        /// <summary>
        /// Енумератор[key]
        /// </summary>
        /// <param name="key">Значение ключа</param>
        /// <returns></returns>
        public ValueType this[KeyType key]
        {
            get
            {
                ValueType val;
                if (_contents.TryGetValue(key, out val))
                    return val;
                else
                    return default(ValueType);//This will be null for reference types and zero for value types.
            }
            set { InternalAdd(key, value); }
        }
        /// <summary>
        /// Добавить ключ и значение в коллекцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void Add(KeyType key, ValueType value)
        {
            InternalAdd(key, value);
        }
        /// <summary>
        /// Добавить ключ и значение в коллекцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        private void InternalAdd(KeyType key, ValueType value)
        {
            if (!_contents.ContainsKey(key))
            {

                if (_capacity > 0)
                {
                    _keyQ.Enqueue(key);//Добавить объект в конец очереди

                    if (_keyQ.Count > _capacity)
                        _contents.Remove(_keyQ.Dequeue());//Удалить из словаря объект и его ключ, находящийся в начале очереди.
                }
            }

            _contents[key] = value;
        }
    }
}
