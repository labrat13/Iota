﻿Этот проект - я собрал все имеющиеся у меня адаптеры БД чтобы свести их в один стандартный комплект.
Но оказалось, что такой комплект не будет компилироваться в dll - ему нужны сборки адаптеров СУБД, а без них эта сборка не запустится.
По этой же причине, хотя в классах есть общая функциональность, я не могу вынести ее в родительский класс.
 Приходится в каждом классе все эти функции повторять.
Думаю, в своих проектах я буду использовать эти классы самостоятельно, раздельно друг от друга.
- Хотя, для Тапп у меня есть идея: одна сборка для хранения интерфейса или базового класса адаптера. 
   Остальные классы адаптеров вынести в собственные длл. Загружать их кодом при необходимости.
 - Это будет 4 сборки dll: base+oledb, mssql, mysql, sqlite.
- такая интеграция была начата в проекте Тапп и застряла из-за этой же усталости и высокой сложности и нудности.

24 декабря 2019
- эти классы надо будет еще дополнять по мере работы над другими проектами.
  По мере выявления ошибок и написания новых полезных функций.
  Так что проект не расформировывать, держать целым.
- я эти 4 класса собрал, в них почти одинаковые наборы функций.
  Но в них теперь много бардака!
  - Много мелких ошибок, которые только на тестах можно выявить.
     Из-за разных форматов БД.
  - Много избыточных функций. Они ранее были реализованы в разных проектах для разных целей.
     А теперь если их все более-менее универсализировать, они дублируют друг друга по функциональности.
  - Путаница с транзакциями в командах - одни участвуют в текущей транзакции, другие - нет.
     Это даст кучку ошибок внутри транзакции в реальном проекте.     
  - Не все функции используют команды-члены класса вроде m_cmddWithoutArguments.
    Некоторые собственные команды создают в стеке, причем использующие транзакции.
    Вреда вроде бы нет, но бардак небольшой есть.
  - разный набор функций у серверных СУБД и файловых СУБД.
    Я пытаюсь его единообразить, но пока не очень получается.
    Единый интерфейс что-то не прижился пока. Хотя следует его внедрить.
    - единый интерфейс хорош внутри проекта, когда проекту нужна поддержка многих вариантов БД.
      Чтобы быстро перевести проект с одной БД на другую.
      А в одноразовых конвертерах моих - это лишняя работа. Все равно адаптер кастомизировать придется.
      Я даже не наследую эти классы - просто дописываю в них нужные функции.
    - СУБД используют разные типы данных, и их не удается согнать под один интерфейс.
      Аксцесс не поддерживает Int64, sqlite - поддерживает только Int64.
      Вот как это объединить? Да и не нужно - в проектах эта универсальность не востребована.  
  - функции недостаточно универсальны. Их еще надо дорабатывать.
  - названия функций и аргументов недостаточно универсальны. 
    Они все еще отражают их применение в прошлых проектах-источниках кода.       
 
 04 января 2020 
 - добавлена поддержка наследования для производных классов.
   Производные классы позволяют сосредоточиться на новом коде. 
   - все переменные объявлены protected
   - в производном классе нужно определить наследные конструкторы и переопределить ClearCommands()
   
           //все объекты команд сбрасываются в нуль при отключении соединения с БД
        //TODO: Новые команды внести в ClearCommands()
        /// <summary>
        /// Команда новая
        /// </summary>
        protected OleDbCommand m_cmdGetPatentRecords;

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        public PatentsAccessDb(): base()
        {
        }
        /// <summary>
        /// NT-Конструктор с созданием и открытием соединения
        /// </summary>
        /// <param name="connectionString">connection string</param>
        /// <param name="open">Open the connection</param>
        /// <exception cref="InvalidOperationException">Invalid connection string or connection already open</exception>
        /// <exception cref="SqlException">Error in opening</exception>
        public PatentsAccessDb(string connectionString, bool open): base(connectionString, open)
        {
        }

        /// <summary>
        /// NT-Close and dispose connection
        /// </summary>
        ~PatentsAccessDb()
        {
        }

        /// <summary>
        /// NT-все объекты команд класса сбросить в нуль
        /// </summary>
        protected override void ClearCommands()
        {
            this.m_cmdGetPatentRecords = null;

            base.ClearCommands();
        }