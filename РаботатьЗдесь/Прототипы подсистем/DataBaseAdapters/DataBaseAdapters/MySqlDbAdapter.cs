using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;

namespace DataBaseAdapters
{

    /*  Состояние шаблона:
    * 23 декабря 2019 г - начальная версия. 
    * ReadOnly проперти и переменная не устанавливаются и всегда возвращают значение false.
    * Компилируется, но не знаю, правильно ли работает.
    * 04 января 2020 - добавлена поддержка наследования для производных классов.
    * Производные классы позволяют сосредоточиться на новом коде. 
    */

    /* Если вставлять строки без открытой транзакции, они вставляются медленнее, чем внутри открытой транзакции.
    * Но после закрытия транзакции надо закрыть соединение с БД (для MsSqlServer). Иначе код выдает ошибку.
    */

    /// <summary>
    /// NR-Класс адаптера БД экспериментальный, поскольку БД самобытная 
    /// Наверняка тут куча ошибок просто из-за того что я устал эти классы собирать сегодня.
    /// </summary>
    public class MySqlDbAdapter
    {
        //TODO: Укажите здесь все таблицы БД как строковые константы. Это упростит работу с общими функциями таблиц.
        ///// <summary>
        ///// Константа название таблицы БД - для функций адаптера
        ///// </summary>
        //internal const string TableName1 = "TableName1";


        #region Fields
        /// <summary>
        /// database connection string
        /// </summary>
        protected String m_connectionString;
        /// <summary>
        /// database connection
        /// </summary>
        protected MySqlConnection m_connection;
        /// <summary>
        /// Transaction for current connection
        /// </summary>
        protected MySqlTransaction m_transaction;
        /// <summary>
        /// Timeout value for DB command, in seconds
        /// </summary>
        protected int m_Timeout;
        /// <summary>
        /// Database is read-only
        /// </summary>
        protected bool m_ReadOnly;


        //все объекты команд сбрасываются в нуль при отключении соединения с БД
        //TODO: Новые команды внести в ClearCommands()
        /// <summary>
        /// Команда без параметров, используемая во множестве функций
        /// </summary>
        protected MySqlCommand m_cmdWithoutArguments;
        //private MySqlCommand m_cmd1;
        //private MySqlCommand m_cmd2;
        //private MySqlCommand m_cmd3;

        #endregion

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        public MySqlDbAdapter()
        {
            m_connection = null;
            m_connectionString = String.Empty;
            m_Timeout = 60;
            m_transaction = null;
            m_ReadOnly = false;
            ClearCommands();
            return;
        }

        /// <summary>
        /// NT-Конструктор с созданием и открытием соединения
        /// </summary>
        /// <param name="connectionString">connection string</param>
        /// <param name="open">Open the connection</param>
        /// <exception cref="InvalidOperationException">Invalid connection string or connection already open</exception>
        /// <exception cref="SqlException">Error in opening</exception>
        public MySqlDbAdapter(string connectionString, bool open)
        {
            ClearCommands();
            m_Timeout = 60;
            m_ReadOnly = false;
            m_connectionString = connectionString;
            this.m_connection = new MySqlConnection(connectionString);

            if (open == true)
                this.m_connection.Open();

            return;
        }

        /// <summary>
        /// NT-Close and dispose connection
        /// </summary>
        ~MySqlDbAdapter()
        {
            this.Close();
        }

        #region Properties
        /// <summary>
        /// Database is read-only
        /// Не работает здесь.
        /// </summary>
        public bool ReadOnly
        {
            get { return m_ReadOnly; }
            //set { dbReadOnly = value; }
        }
        /// <summary>
        /// Get Set timeout value for all  new execute command
        /// </summary>
        public int Timeout
        {
            get
            {
                return m_Timeout;
            }
            set
            {
                m_Timeout = value;
            }
        }

        /// <summary>
        /// Get or Set connection string
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                this.m_connection = new MySqlConnection(m_connectionString);
            }
        }

        /// <summary>
        /// Is connection opened?
        /// </summary>
        public bool isConnectionActive
        {
            get { return ((this.m_connection != null) && (this.m_connection.State == ConnectionState.Open)); }
        }
        /// <summary>
        /// Is transaction active?
        /// </summary>
        public bool isTransactionActive
        {
            get { return (this.m_transaction != null); }
        }

        #endregion

        #region Service functions

        /// <summary>
        /// NT-все объекты команд класса сбросить в нуль
        /// </summary>
        protected virtual void ClearCommands()
        {
            m_cmdWithoutArguments = null;
            //m_cmd1 = null;
            //m_cmd2 = null;
            //m_cmd3 = null;

            return;
        }

        /// <summary>
        /// NT-Open connection if is closed
        /// </summary>
        /// <exception cref="InvalidOperationException">Invalid connection string or connection already open</exception>
        /// <exception cref="SqlException">Error in opening</exception>
        /// <remarks>Без лога, или проверять его существование!</remarks>
        public void Open()
        {
            if (this.m_connection.State == ConnectionState.Closed)
                this.m_connection.Open();
        }
        /// <summary>
        /// NT-Close connection if not closed
        /// </summary>
        /// <exception cref="SqlException">Error in connection</exception>
        /// <remarks>Без лога, или проверять его существование!</remarks>
        public void Close()
        {
            if (m_connection != null)
            {
                if (m_connection.State != ConnectionState.Closed)
                    m_connection.Close();
                m_connection = null;
            }

            //все объекты команд сбросить в нуль при отключении соединения с БД, чтобы ссылка на объект соединения при следующем подключении не оказалась устаревшей
            ClearCommands();

            return;
        }

        /// <summary>
        /// NT-Создать строку соединения с БД
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="user">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="database">Имя базы данных</param>
        /// <param name="timeout">Таймаут соединения в секундах</param>
        /// <returns></returns>
        public static String CreateConnectionString(string server, string user, string password, string database, Int32 timeout)
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = server;
            builder.UserID = user;
            builder.UseAffectedRows = true;
            builder.Password = password;
            builder.Database = database;
            builder.ConnectionTimeout = (UInt32)timeout;
            return builder.ConnectionString;
        }

        #endregion

        #region Transaction functions
        /// <summary>
        /// NT-Начать транзакцию. 
        /// </summary>
        public void TransactionBegin()
        {
            m_transaction = m_connection.BeginTransaction();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands();
        }
        /// <summary>
        /// NT-Подтвердить транзакцию Нужно закрыть соединение после этого!
        /// </summary>
        public void TransactionCommit()
        {
            m_transaction.Commit();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands();
            m_transaction = null;
        }
        /// <summary>
        /// NT-Отменить транзакцию. Нужно закрыть соединение после этого!
        /// </summary>
        public void TransactionRollback()
        {
            m_transaction.Rollback();
            //сбросить в нуль все объекты команд, чтобы они были пересозданы для новой транзакции
            ClearCommands();
            m_transaction = null;
        }

        #endregion

        #region Database functions

        //TODO: Работать здесь - тут я остановился сегодня. Устал и спать хочу.
        // Я копировал код из MsSql2005DbAdapter поскольку он наиболее похожий на здешний.
        //но вот недокопировал, устал. Теперь надо вручную копировать функции и править их.
        //Это все сможет скомпилироваться, но работать не будет, так как тут много мелких особенностей и ошибок.

       /// <summary>
        /// NT - Удалить таблицу из БД. Если таблица не существует, функция завершается успешно.
        /// </summary>
        /// <param name="tablename">Имя таблицы</param>
        /// <param name="timeout">Таймаут операции в секундах</param>
        public void TableDelete(String tablename, int timeout)
        {
            String q = String.Format("DROP TABLE IF EXISTS `{0}`;", tablename);
            this.ExecuteNonQuery(q, timeout);

            return;
        }
        /// <summary>
        /// NT - Создать индексы таблицы БД. 
        /// Индексы следует создавать после массовой загрузки строк, иначе загрузка будет медленной.
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="indexName">Имя создаваемого индекса</param>
        /// <param name="columnName">Имя индексируемого столбца</param>
        /// <param name="timeout">Таймаут операции в секундах</param>
        public void TableCreateIndex(String tableName, String indexName, String columnName, int timeout)
        {
            String q = String.Format("ALTER TABLE `{0}` ADD INDEX `{1}`  ( `{2}` )", tableName, indexName, columnName);
            this.ExecuteNonQuery(q, timeout);
        }
        /// <summary>
        /// NT-Удалить индекс таблицы БД
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="indexName">Имя удаляемого индекса</param>
        /// <param name="timeout">Таймаут операции в секундах</param>
        public void TableDeleteIndex(String tableName, String indexName, int timeout)
        {
            String q = String.Format("ALTER TABLE `{0}` DROP INDEX `{1}`", tableName, indexName);
            this.ExecuteNonQuery(q, timeout);
        }

        /// <summary>
        /// NT-Удалить все строки из указанной таблицы.
        /// Счетчик первичного ключа не сбрасывается - его отдельно надо сбрасывать.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        public void TableClear(string table)
        {
            //DELETE FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0};", table);
            m_cmdWithoutArguments.CommandText = query;
            m_cmdWithoutArguments.ExecuteNonQuery();

            return;
        }

        /// <summary>
        /// NT-Выполнить запрос к БД
        /// </summary>
        /// <param name="query"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Int32 ExecuteNonQuery(String query, int timeout)
        {
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection);
            cmd.CommandTimeout = timeout;
            Object ob = cmd.ExecuteScalar(); //Int32 type returned here
            if (ob == null) return 0;
            return Int32.Parse(ob.ToString());//convert to-from string
        }

        /// <summary>
        /// NT-Получить строку текста из ридера таблицы или пустую строку
        /// </summary>
        /// <param name="rdr">Объект ридера таблицы бд</param>
        /// <param name="p">Номер столбца в таблице и ридере</param>
        /// <returns>Возвращает строку текста из поля или пустую строку если в поле хранится значение DbNull</returns>
        public static string getDbString(MySqlDataReader rdr, int p)
        {
            if (rdr.IsDBNull(p))
                return String.Empty;
            else return rdr.GetString(p).Trim();
        }

        /// <summary>
        /// NT-get max of id from specified table
        /// </summary>
        /// <param name="table">table name</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Int64 getTableMaxInt64(string table, string column, int timeout)
        {
            string query = String.Format("SELECT MAX({0}) AS id FROM {1}", column, table);
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection);
            Object ob = cmd.ExecuteScalar(); //Int32 type returned here
            return Int64.Parse(ob.ToString());//convert to-from string
        }

        /// <summary>
        /// NT-get min of id from specified table
        /// </summary>
        /// <param name="table">table name</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Int64 getTableMinInt64(string table, string column, int timeout)
        {
            string query = String.Format("SELECT MIN({0}) AS id FROM {1}", column, table);
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection);
            Object ob = cmd.ExecuteScalar(); //Int32 type returned here
            return Int64.Parse(ob.ToString());//convert to-from string
        }

        /// <summary>
        /// NT- Получить максимальное значение ИД для столбца таблицы
        /// Обычно применяется для столбца первичного ключа, но можно и для других целочисленных столбцов.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns max value or -1 if no results</returns>
        public int getTableMaxInt32(string table, string column)
        {
            //SELECT MAX(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MAX({0}) AS Expr1 FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            MySqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            int result = -1;//check null values
            if(!rdr.IsDBNull(0))
                result = rdr.GetInt32(0);
            rdr.Close();

            return result;
        }

        /// <summary>
        /// NT-Получить минимальное значение ИД для столбца таблицы
        /// Обычно применяется для столбца первичного ключа, но можно и для других целочисленных столбцов.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns min value or -1 if no results</returns>
        public int getTableMinInt32(string table, string column)
        {
            //SELECT MIN(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MIN({0}) AS Expr1 FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            MySqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            int result = -1;//check null values
            if (!rdr.IsDBNull(0))
                result = rdr.GetInt32(0);
            rdr.Close();

            return result;
        }

        /// <summary>
        /// NT-Получить число записей в таблице
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <param name="timeout">Таймаут выборки, секунд</param>
        /// <param name="WhereClause">Условия выборки или пустая строка. Пример: WHERE (Id > 1000)</param>
        /// <returns>Возвращает число записей, или -1 при ошибке.</returns>
        public int GetRowCount(string table, string column, int timeout, string WhereClause)
        {
            //SELECT COUNT(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, this.m_transaction);
                m_cmdWithoutArguments.CommandTimeout = timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) AS Expr1 FROM {1} {2};", column, table, WhereClause);
            m_cmdWithoutArguments.CommandText = query;
            //get result
            MySqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            int result = -1;//check null values
            if (!rdr.IsDBNull(0))
                result = rdr.GetInt32(0);
            rdr.Close();

            return result;
        }

        /// <summary>
        /// NT-Получить число записей в таблице, с указанным числовым значением.
        /// </summary>
        /// <remarks>Применяется для столбца первичного ключа, проверяет что запись с этим ключом существует.
        /// Но может применяться и в других случаях.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца</param>
        /// <param name="timeout">Таймаут выборки, секунд</param>
        /// <param name="val">Числовое значение в столбце</param>
        /// <returns>Возвращает число записей с таким значением этого столбца, или -1 при ошибке.</returns>
        public int GetRowCount(string table, string column, int timeout, int val)
        {
            //SELECT column FROM table WHERE (column = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, this.m_transaction);
                m_cmdWithoutArguments.CommandTimeout = timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1} WHERE ({0} = {2});", column, table, val);
            m_cmdWithoutArguments.CommandText = query;
            //get result
            MySqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            int result = -1;//check null values
            if (!rdr.IsDBNull(0))
                result = rdr.GetInt32(0);
            rdr.Close();

            return result;
        }

        /// <summary>
        /// NT-Проверить существование записи с указанным Id номером
        /// </summary>
        /// <param name="tablename">Название таблицы</param>
        /// <param name="column">Название столбца идентификатора</param>
        /// <param name="idValue">Значение Id записи</param>
        /// <returns>Возвращает True если запись существует, иначе возвращает False.</returns>
        public bool IsRowExists(String tablename, string column, Int32 idValue)
        {
            String query = String.Format("SELECT {0} FROM {1} WHERE ({0} = {2})", column, tablename, idValue);
            if (this.m_cmdWithoutArguments == null)
            {
                //create command
                this.m_cmdWithoutArguments = new MySqlCommand(query, m_connection, m_transaction);
                this.m_cmdWithoutArguments.CommandTimeout = 600;
            }
            this.m_cmdWithoutArguments.CommandText = query;
            //get result
            MySqlDataReader rdr = this.m_cmdWithoutArguments.ExecuteReader();
            bool result = rdr.HasRows;
            rdr.Close();
            return result;
        }

        /// <summary>
        /// NT-Удалить запись(и) из таблицы по значению поля в столбце
        /// </summary>
        /// <remarks>Удаляет все строки с указанным значением параметра.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца</param>
        /// <param name="val">Значение столбца</param>
        /// <returns>Возвращает число затронутых (удаленных) строк таблицы.</returns>
        public int DeleteRow(string table, string column, int val)
        {
            //DELETE FROM table WHERE (column = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new MySqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE ({1} = {2});", table, column, val);
            m_cmdWithoutArguments.CommandText = query;
            return m_cmdWithoutArguments.ExecuteNonQuery(); //Тут могут быть исключения из-за другого типа данных
        }

        /// <summary>
        /// NT-получить значение автоинкремента для последней измененной таблицы в текущем сеансе БД
        /// НЕ РЕАЛИЗОВАНО! Выбрасывает исключение.
        /// </summary>
        /// <returns>Возвращает значение автоинкремента для последней измененной таблицы в текущем сеансе БД.</returns>
        public int GetLastAutonumber()
        {
            throw new NotImplementedException();//TODO: добавить код выборки значения автоинкремента таблицы
            //if (m_cmdWithoutArguments == null)
            //{
            //    m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
            //    m_cmdWithoutArguments.CommandTimeout = 60;
            //}
            ////execute command
            //m_cmdWithoutArguments.CommandText = "SELECT @@IDENTITY;";
            //return (int)m_cmdWithoutArguments.ExecuteScalar();
        }

        /// <summary>
        /// NT-Получить список ид записей таблицы
        /// </summary>
        /// <param name="tablename">Название таблицы</param>
        /// <param name="column">Название столбца ИД</param>
        /// <param name="where">текст условия отбора. Если отбор не требуется, то передать пустую строку или null. Пример: id > 100 </param>
        /// <returns>Возвращает список идентификаторов записей таблицы</returns>
        public List<Int32> getListOfInt32(String tablename, string column, string where)
        {
            String query;
            //create query text
            if (String.IsNullOrEmpty(where))
                query = String.Format("SELECT {0} FROM {1}", column, tablename );
            else
                query = String.Format("SELECT {0} FROM {1} WHERE ({2})", column, tablename, where);
            //create command
            MySqlCommand cmd = new MySqlCommand(query, m_connection, m_transaction);
            cmd.CommandTimeout = 600;
            //get result
            MySqlDataReader rdr = cmd.ExecuteReader();
            List<int> li = new List<int>();
            if (rdr.HasRows == true)
            {
                while (rdr.Read())
                {
                    li.Add(rdr.GetInt32(0));
                }
            }
            rdr.Close();

            return li;
        }

        /// <summary>
        /// NT-Minimal Sql Datetime
        /// Not implemented here!
        /// </summary>
        /// <returns></returns>
        public static DateTime getSqlMinDatetime()
        {
            throw new NotImplementedException();
            //return new DateTime(1980, 1, 1, 0, 0, 0);
        }

        /// <summary>
        /// NT-собирает список ИД в строку для WHERE
        /// Возвращает строку вида: (Id = 0) OR (Id = 1) OR (Id = 3)
        /// </summary>
        /// <param name="postIdList">Список ИД</param>
        /// <param name="columnName">Название столбца запроса</param>
        /// <returns>Возвращает строку вида: (Id = 0) OR (Id = 1) OR (Id = 3)</returns>
        /// <example>
        /// Это пример функции которая использует данную функцию для выборки нескольких строк за один запрос
        /// Такой прием ускоряет выборку множества строк из большой БД в 6..10 раз
        /// <code>
        /// public List<PostsObj> getPostsByIds(List<int> idList)
        /// {
        ///    List<PostsObj> result = new List<PostsObj>();
        ///    int len = idList.Count;
        ///    if (len == 0) return result;//сразу выйти если входной список пустой
        ///    //1 разделить массив ид на порции по 8 элементов, если там есть столько
        ///    List<List<Int32>> lar = SplitListInt32(idList, 8);
        ///    //для каждой порции:
        ///    foreach (List<Int32> li in lar)
        ///    {
        ///        //2 вызвать функцию выборки из БД по массиву из 8 ид
        ///        //3 собрать все в один выходной список 
        ///        result.AddRange(this.getPostsByIds_sub(li));
        ///    }
        ///    return result;
        /// }
        /// </code>
        /// </example>
        private string makeWhereText(List<int> rowIdList, string columnName)
        {
            //returns (Id = 0) OR (Id = 1) OR (Id = 3)

            int cnt = rowIdList.Count;
            String[] sar = new string[cnt];
            //
            for (int i = 0; i < cnt; i++)
                sar[i] = String.Format("({0} = {1})", columnName, rowIdList[i]);
            //
            return String.Join(" OR ", sar);
        }

        /// <summary>
        /// RT-разделить список на части по N элементов или менее
        /// </summary>
        /// <param name="idList">Исходный список</param>
        /// <param name="n">Размер каждой из частей, больше 0</param>
        /// <returns>Возвращает список списков, каждый из которых содержит части входного списка.</returns>
        public static List<List<int>> SplitListInt32(List<int> idList, int n)
        {
            //проверка аргументов
            if (n <= 0)
                throw new ArgumentException("Argument N must be greather than 0!", "n");

            List<List<Int32>> result = new List<List<int>>();
            int cnt = idList.Count;
            if (cnt == 0) return result;
            //если там меньше N, то весь список возвращаем как единственный элемент 
            if (cnt <= n)
            {
                result.Add(idList);
                return result;
            }
            //иначе
            int c = cnt / n; //полных кусков по n элементов
            int cs = cnt % n; //остаточная длина куска
            //целые куски добавим
            for (int i = 0; i < c; i++)
                result.Add(idList.GetRange(i * n, n));
            //остаток
            if (cs > 0)
                result.Add(idList.GetRange(c * n, cs));

            return result;
        }


        //TODO: Если нужна функция очистки всей БД, раскомментируйте код и измените его, вписав правильные имена таблиц.
        ///// <summary>
        ///// NFT-Очистить БД 
        ///// </summary>
        ///// <returns>True if Success, False otherwise</returns>
        //internal bool ClearDb()
        //{
        //    bool result = false;
        //    try
        //    {
        //        this.TransactionBegin();
        //        this.TableClear(DbAdapter.ContentTableName);
        //        this.TableClear(DbAdapter.DocumentTableName);
        //        this.TableClear(DbAdapter.PictureTableName);
        //        this.TransactionCommit();
        //        result = true;
        //    }
        //    catch (Exception)
        //    {
        //        this.TransactionRollback();
        //        result = false;
        //    }
        //    return result;
        //}

        #endregion


        #region *** Для таблицы свойств ключ-значение ***
        /// <summary>
        /// NT-Получить значения свойств из таблицы БД
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <returns>Словарь, содержащий все пары ключ-значение из таблицы БД</returns>
        public Dictionary<String, String> KeyValueReadDictionary(String table)
        {
            Dictionary<String, string> dict = new Dictionary<string, string>();
            //create command
            String query = String.Format("SELECT * FROM {0};", table);
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //execute command
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    //int id = rdr.GetInt32(0); //id not used
                    String param = rdr.GetString(1);
                    String val = rdr.GetString(2);
                    //store to dictionary 
                    dict.Add(param, val);
                }
            }
            //close reader
            rdr.Close();

            return dict;
        }

        /// <summary>
        /// NT - Перезаписать значения свойств в таблице БД.
        /// Все записи из таблицы будут удалены и заново вставлены.
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="dic">Словарь, содержащий пары ключ-значение</param>
        public void KeyValueStoreDictionary(String table, Dictionary<string, string> dic)
        {
            //1 - очистить таблицу
            String query = String.Format("DELETE * FROM {0};", table);
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            cmd.ExecuteNonQuery();
            //2 - записать новые значения
            query = String.Format("INSERT INTO {0} (param, val) VALUES (@p0, @p1);", table);
            cmd.CommandText = query;
            cmd.Parameters.Add("@p0", MySqlDbType.VarChar);//need 3 arg = size of string
            cmd.Parameters.Add("@p1", MySqlDbType.VarChar);
            //execute commands
            foreach (KeyValuePair<String, String> kvp in dic)
            {
                cmd.Parameters[0].Value = kvp.Key;
                cmd.Parameters[1].Value = kvp.Value;
                cmd.ExecuteNonQuery();
            }

            return;
        }

        /// <summary>
        /// NT-Получить один из параметров, не загружая весь набор
        /// </summary>
        /// <remarks>
        /// Это функция для таблицы Ключ-Значения. Ыункция универсальная, поэтому надо указывать имена таблиц и столбцов. 
        /// Структура таблицы:
        /// - id counter, primary key - первичный ключ, не читается.
        /// - p text - название параметра, ключ (строка), должно быть уникальным.
        /// - d text - значение параметра, значение (строка), допускаются повторы и пустые строки.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="columnName">Название столбца ключа</param>
        /// <param name="paramName">Название параметра (ключ)</param>
        /// <returns>Возвращает строку значения параметра</returns>
        public string KeyValueGetParameter(String table, String columnName, String paramName)
        {
            //create command
            String query = String.Format("SELECT * FROM {0} WHERE ({1} = '{2}' );", table, columnName, paramName);
            MySqlCommand cmd = new MySqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //execute command
            MySqlDataReader rdr = cmd.ExecuteReader();
            String result = String.Empty;
            if (rdr.HasRows)
            {
                while (rdr.Read())
                {
                    //int id = rdr.GetInt32(0); //id not used
                    //String param = rdr.GetString(1);//param not used
                    result = rdr.GetString(2);
                }
            }
            //close reader
            rdr.Close();
            return result;
        }

        #endregion

        #region Примеры функций

        ///// <summary>
        ///// NT-Обновить лексему, выбрав ее по id.
        ///// </summary>
        ///// <param name="tablename">Table name</param>
        ///// <param name="lex">Lexem object to update</param>
        ///// <param name="timeout">Command timeout</param>
        //public Int32 UpdateLexem(String tablename, Lexem lex, Int32 timeout)
        //{

        //    //create command
        //    if (m_UpdateLexemCmd == null)
        //    {
        //        //UPDATE `word` SET `lexid` = '1', `text` = 'ere', `freq` = '9', `typ` = '7',`uflag` = '3',`hash` = 'sd',`comment` = '456' WHERE `id` =641461;

        //        string query = String.Format("UPDATE `{0}` SET `lexid` = @lexid, `text` = @text,`freq` = @freq,`typ` = @typ,`uflag` = @uflag,`priform` = @priform,`rod` = @rod,`chislo` = @chislo,`padeg` = @padeg,`odushev` = @odushev,`sclontype` = @sclontype,`lico` = @lico,`vremja` = @vremja,`zalog` = @zalog,`vid` = @vid,`naklon` = @naklon,`perehod` = @perehod, `vozvrat` = @vozvrat, `sravn` = @sravn,`form` = @form,`gramval` = @gramval,`useful` = @useful,`hash` = @hash,`comment` = @comment WHERE (`id` = @id);", tablename);
        //        MySqlCommand cmd = new MySqlCommand(query, m_connection);
        //        cmd.Parameters.Add("@lexid", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@text", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@freq", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@typ", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@uflag", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@priform", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@rod", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@chislo", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@padeg", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@odushev", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@sclontype", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@lico", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vremja", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@zalog", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vid", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@naklon", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@perehod", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vozvrat", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@sravn", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@form", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@gramval", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@useful", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@hash", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@comment", MySqlDbType.VarChar, 32767);
        //        cmd.Parameters.Add("@id", MySqlDbType.Int32); //table id for WHERE
        //        //store to variable
        //        this.m_UpdateLexemCmd = cmd;
        //    }
        //    //execute command
        //    this.m_UpdateLexemCmd.CommandTimeout = timeout; //set command timeout
        //    MySqlCommand c = this.m_UpdateLexemCmd; //make alias for quick typing
        //    //put data to parameters
        //    c.Parameters[0].Value = lex.m_lexid;
        //    c.Parameters[1].Value = lex.m_text;
        //    c.Parameters[2].Value = lex.m_freq;
        //    c.Parameters[3].Value = lex.m_type;
        //    c.Parameters[4].Value = lex.m_userflag;
        //    c.Parameters[5].Value = lex.m_primaryForm;
        //    c.Parameters[6].Value = lex.m_rod;
        //    c.Parameters[7].Value = lex.m_chislo;
        //    c.Parameters[8].Value = lex.m_padeg;
        //    c.Parameters[9].Value = lex.m_odushev;
        //    c.Parameters[10].Value = lex.m_sklontype;
        //    c.Parameters[11].Value = lex.m_lico;
        //    c.Parameters[12].Value = lex.m_vremja;
        //    c.Parameters[13].Value = lex.m_zalog;
        //    c.Parameters[14].Value = lex.m_vid;
        //    c.Parameters[15].Value = lex.m_naklon;
        //    c.Parameters[16].Value = lex.m_perehod;
        //    c.Parameters[17].Value = lex.m_vozvrat;
        //    c.Parameters[18].Value = lex.m_sravnenie;
        //    c.Parameters[19].Value = lex.m_form;
        //    c.Parameters[20].Value = lex.m_gramval;
        //    c.Parameters[21].Value = lex.m_useful;
        //    c.Parameters[22].Value = lex.m_hash;
        //    c.Parameters[23].Value = lex.m_comment;
        //    c.Parameters[24].Value = lex.m_id; //table id for WHERE
        //    //execute command
        //    return c.ExecuteNonQuery();
        //}

        ///// <summary>
        ///// NT-Insert lexem to table word
        ///// </summary>
        ///// <param name="tablename">Table name</param>
        ///// <param name="lex">Lexem object to insert</param>
        ///// <param name="timeout">Command timeout</param>
        //public Int32 InsertLexem(String tablename, Lexem lex, Int32 timeout)
        //{
        //    //create command
        //    if (m_InsertToLexemCmd1 == null)
        //    {
        //        string query = String.Format("INSERT INTO `{0}` (`lexid`, `text`,`freq`,`typ`,`uflag`,`priform`,`rod`,`chislo`,`padeg`,`odushev`,`sclontype`,`lico`,`vremja`,`zalog`,`vid`,`naklon`,`perehod`, `vozvrat`, `sravn`,`form`,`gramval`,`useful`,`hash`,`comment`) VALUES (@lexid, @text, @freq, @typ, @uflag, @priform, @rod, @chislo, @padeg, @odushev, @sclontype, @lico, @vremja, @zalog, @vid, @naklon, @perehod, @vozvrat, @sravn, @form, @gramval, @useful, @hash, @comment);", tablename);
        //        MySqlCommand cmd = new MySqlCommand(query, m_connection);
        //        cmd.Parameters.Add("@lexid", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@text", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@freq", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@typ", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@uflag", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@priform", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@rod", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@chislo", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@padeg", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@odushev", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@sclontype", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@lico", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vremja", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@zalog", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vid", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@naklon", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@perehod", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@vozvrat", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@sravn", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@form", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@gramval", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@useful", MySqlDbType.Int32);
        //        cmd.Parameters.Add("@hash", MySqlDbType.VarChar, 64);
        //        cmd.Parameters.Add("@comment", MySqlDbType.VarChar, 32767);
        //        //store to variable
        //        this.m_InsertToLexemCmd1 = cmd;
        //    }
        //    //execute command
        //    this.m_InsertToLexemCmd1.CommandTimeout = timeout; //set command timeout
        //    MySqlCommand c = this.m_InsertToLexemCmd1; //make alias for quick typing
        //    //put data to parameters
        //    c.Parameters[0].Value = lex.m_lexid;
        //    c.Parameters[1].Value = lex.m_text;
        //    c.Parameters[2].Value = lex.m_freq;
        //    c.Parameters[3].Value = lex.m_type;
        //    c.Parameters[4].Value = lex.m_userflag;
        //    c.Parameters[5].Value = lex.m_primaryForm;
        //    c.Parameters[6].Value = lex.m_rod;
        //    c.Parameters[7].Value = lex.m_chislo;
        //    c.Parameters[8].Value = lex.m_padeg;
        //    c.Parameters[9].Value = lex.m_odushev;
        //    c.Parameters[10].Value = lex.m_sklontype;
        //    c.Parameters[11].Value = lex.m_lico;
        //    c.Parameters[12].Value = lex.m_vremja;
        //    c.Parameters[13].Value = lex.m_zalog;
        //    c.Parameters[14].Value = lex.m_vid;
        //    c.Parameters[15].Value = lex.m_naklon;
        //    c.Parameters[16].Value = lex.m_perehod;
        //    c.Parameters[17].Value = lex.m_vozvrat;
        //    c.Parameters[18].Value = lex.m_sravnenie;
        //    c.Parameters[19].Value = lex.m_form;
        //    c.Parameters[20].Value = lex.m_gramval;
        //    c.Parameters[21].Value = lex.m_useful;
        //    c.Parameters[22].Value = lex.m_hash;
        //    c.Parameters[23].Value = lex.m_comment;
        //    //execute command
        //    return c.ExecuteNonQuery();
        //}

        ///// <summary>
        ///// NR-Выбрать лексемы как часть общего диапазона  
        ///// </summary>
        ///// <param name="table">Имя таблицы</param>
        ///// <param name="fromId">Начальный id первичный ключ диапазона таблицы</param>
        ///// <param name="count">Число строк для чтения</param>
        ///// <param name="timeout">Таймаут в секундах</param>
        ///// <returns>Возвращает список лексем</returns>
        //public List<Lexem> GetLexems(string table, long fromId, int count, int timeout)
        //{
        //    //create command
        //    string query = String.Format("SELECT * FROM {0} WHERE ((`id` >= {1}) AND (`id` < {2}))", table, fromId, fromId + count);
        //    MySqlCommand cmd = new MySqlCommand(query, this.m_connection);
        //    cmd.CommandTimeout = timeout;//set command timeout
        //    //execute command
        //    MySqlDataReader rdr = cmd.ExecuteReader();
        //    List<Lexem> lx = readLexemList(rdr);
        //    //close and return;
        //    rdr.Close();
        //    return lx;
        //}

        ///// <summary>
        ///// NT-Выбрать лексемы по заданному WHERE
        ///// </summary>
        ///// <param name="table">Имя таблицы</param>
        ///// <param name="wherePart">часть SQL-запроса после WHERE</param>
        ///// <param name="timeout">Таймаут в секундах</param>
        ///// <returns>Возвращает список лексем</returns>
        //public List<Lexem> GetLexems(string table, string wherePart, int timeout)
        //{
        //    //create command
        //    string query = String.Format("SELECT * FROM `{0}` WHERE ({1})", table, wherePart);
        //    MySqlCommand cmd = new MySqlCommand(query, this.m_connection);
        //    cmd.CommandTimeout = timeout;//set command timeout
        //    //execute command
        //    MySqlDataReader rdr = cmd.ExecuteReader();
        //    List<Lexem> lx = readLexemList(rdr);
        //    //close and return;
        //    rdr.Close();
        //    return lx;



        //}

        ///// <summary>
        ///// NT-Собрать выборку из MySqlDataReader в список лексем. Не закрывает MySqlDataReader объект.
        ///// </summary>
        ///// <param name="rdr"></param>
        ///// <returns></returns>
        //private static List<Lexem> readLexemList(MySqlDataReader rdr)
        //{
        //    List<Lexem> lexemList = new List<Lexem>();
        //    if (rdr.HasRows)
        //    {
        //        while (rdr.Read())
        //        {
        //            //create lexem
        //            Lexem lex = new Lexem();
        //            //read fields
        //            lex.m_id = rdr.GetInt32(0);
        //            lex.m_lexid = rdr.GetInt32(1);
        //            lex.m_text = rdr.GetString(2);
        //            lex.m_freq = rdr.GetInt32(3);
        //            lex.m_type = (КлассЛексемы)rdr.GetInt32(4);
        //            lex.m_userflag = rdr.GetInt32(5);
        //            lex.m_primaryForm = rdr.GetString(6);
        //            lex.m_rod = (Род)rdr.GetInt32(7);
        //            lex.m_chislo = (Число)rdr.GetInt32(8);
        //            lex.m_padeg = (Падеж)rdr.GetInt32(9);
        //            lex.m_odushev = (Одушевленность)rdr.GetInt32(10);
        //            lex.m_sklontype = (ТипСклонения)rdr.GetInt32(11);
        //            lex.m_lico = (Лицо)rdr.GetInt32(12);
        //            lex.m_vremja = (Время)rdr.GetInt32(13);
        //            lex.m_zalog = (Залог)rdr.GetInt32(14);
        //            lex.m_vid = (Вид)rdr.GetInt32(15);
        //            lex.m_naklon = (Наклонение)rdr.GetInt32(16);
        //            lex.m_perehod = (Переходность)rdr.GetInt32(17);
        //            lex.m_vozvrat = (Возвратность)rdr.GetInt32(18);
        //            lex.m_sravnenie = (СтепеньСравнения)rdr.GetInt32(19);
        //            lex.m_form = (Полнота)rdr.GetInt32(20);
        //            lex.m_gramval = (Разряд)rdr.GetInt32(21);
        //            lex.m_useful = (Применение)rdr.GetInt32(22);
        //            lex.m_hash = rdr.GetString(23);//load hash from table
        //            lex.m_comment = rdr.GetString(24);
        //            //put lexem to output list
        //            lexemList.Add(lex);
        //        }
        //    }
        //    return lexemList;
        //}

        ///// <summary>
        ///// NT - Создать таблицу словоформ в БД. Исключение, если таблица уже существует.
        ///// Индексы надо создавать отдельно.
        ///// </summary>
        ///// <param name="tablename">Имя таблицы</param>
        ///// <param name="timeout">Таймаут операции в секундах</param>
        //public void CreateLexemTable(String tablename, int timeout)
        //{
        //    String f = "CREATE TABLE `{0}` ( `id` int(11) NOT NULL auto_increment, `lexid` int(11) default '0', `text` varchar(64) collate utf8_bin NOT NULL, `freq` int(11) default '0', `typ` int(11) default '0', `uflag` int(11) default '0', `priform` varchar(64) collate utf8_bin default NULL, `rod` int(11) default '0', `chislo` int(11) default '0', `padeg` int(11) default '0', `odushev` int(11) default '0', `sclontype` int(11) default '0', `lico` int(11) default '0', `vremja` int(11) default '0', `zalog` int(11) default '0', `vid` int(11) default '0', `naklon` int(11) default '0', `perehod` int(11) default '0', `vozvrat` int(11) default '0', `sravn` int(11) default '0', `form` int(11) default '0', `gramval` int(11) default '0', `useful` int(11) default '0', `hash` varchar(64) collate utf8_bin default NULL, `comment` mediumtext collate utf8_bin, PRIMARY KEY  (`id`)) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=utf8 COLLATE=utf8_bin PACK_KEYS=0 AUTO_INCREMENT=1 ;";
        //    String q = String.Format(f, tablename);//вставить имя таблицы
        //    this.ExecuteNonQuery(q, timeout);

        //    return;
        //}

        ///// <summary>
        ///// NT - Создать индексы таблицы словоформ в БД. 
        ///// Индексы следует создавать после массовой загрузки строк, иначе загрузка будет медленной.
        ///// Создание индексов на 1млн строк может занять более 100 секунд, поэтому задайте достаточный таймаут.
        ///// </summary>
        ///// <param name="tablename">Имя таблицы</param>
        ///// <param name="timeout">Таймаут операции в секундах</param>
        //public void CreateLexemTableIndex(String tablename, int timeout)
        //{
        //    TableCreateIndex(tablename, "Index_text", "text", timeout);
        //    TableCreateIndex(tablename, "Index_hash", "hash", timeout);
        //    TableCreateIndex(tablename, "Index_priform", "priform", timeout);
        //    return;
        //}

        ///// <summary>
        ///// NT - Удалить индексы таблицы словоформ в БД. 
        ///// </summary>
        ///// <param name="tablename">Имя таблицы</param>
        ///// <param name="timeout">Таймаут операции в секундах</param>
        //public void DeleteLexemTableIndex(String tablename, int timeout)
        //{
        //    TableDeleteIndex(tablename, "Index_text", timeout);
        //    TableDeleteIndex(tablename, "Index_hash", timeout);
        //    TableDeleteIndex(tablename, "Index_priform", timeout);
        //    return;
        //}


        //#endregion


        //internal void Open(string databaseName)
        //{
        //    MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder();
        //    csb.Database = databaseName;
        //    csb.Password = "";
        //    csb.Server = "localhost";
        //    csb.UserID = "root";
        //    m_connection = new MySqlConnection(csb.ConnectionString);

        //    m_connection.Open();

        //    MySqlCommand cmd = new MySqlCommand("SET max_allowed_packet=4194304", m_connection);
        //    cmd.ExecuteNonQuery();
        //    cmd = null;
        //}

        //internal void Close()
        //{
        //    m_connection.Close();
        //}



        ///// <summary>
        ///// NT-Получить максимальный первичный ключ в таблице. Возвращает -1 при ошибке.
        ///// </summary>
        //public Int64 GetMaxOfPageId()
        //{
        //    Int64 result = -1;
        //    MySqlCommand cmd = new MySqlCommand("SELECT MAX(`id`) FROM `pages` WHERE 1 ;", m_connection);
        //    cmd.CommandTimeout = 600;
        //    MySqlDataReader rdr = cmd.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        rdr.Read();
        //        result = rdr.GetInt64(0);
        //    }
        //    rdr.Close();
        //    return result;
        //}

        //internal String StorePageToDatabase(WikiPage p)
        //{
        //    String result = "";
        //    MySqlCommand cmd;
        //    if (m_InsertPageCmd == null)
        //    {
        //        String query = "INSERT INTO `pages` (`id`, `namespace`, `pageTitle`, `redirect`, `user`, `model`, `format`, `comment`, `text`) VALUES (@id, @ns, @pageTitle, @redir, @u, @mod, @form, @comment, @text);";
        //        cmd = new MySqlCommand(query, m_connection);
        //        //add parameters
        //        cmd.Parameters.Add("@id", MySqlDbType.Int64);
        //        cmd.Parameters.Add("@ns", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@pageTitle", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@redir", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@u", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@mod", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@form", MySqlDbType.VarChar);
        //        cmd.Parameters.Add("@comment", MySqlDbType.MediumText);
        //        cmd.Parameters.Add("@text", MySqlDbType.LongText);
        //        m_InsertPageCmd = cmd;
        //    }
        //    //execute commands
        //    try
        //    {
        //        cmd = m_InsertPageCmd;
        //        cmd.Parameters[0].Value = p.m_Id;
        //        cmd.Parameters[1].Value = p.m_Namespace;
        //        cmd.Parameters[2].Value = p.m_Title;
        //        cmd.Parameters[3].Value = p.m_RedirectTitle;
        //        cmd.Parameters[4].Value = p.m_UserName;
        //        cmd.Parameters[5].Value = p.m_Model;
        //        cmd.Parameters[6].Value = p.m_Format;
        //        cmd.Parameters[7].Value = p.m_Comment;
        //        cmd.Parameters[8].Value = p.m_Text;
        //        //execute
        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }


        //    return result;
        //}


        ///// <summary>
        ///// NT-Получить страницу по ее названию
        ///// </summary>
        ///// <param name="pageTitle">Название страницы</param>
        ///// <param name="withText">Загружать также текст страницы</param>
        //public List<WikiPage> GetPageByTitle(string title, bool withText)
        //{
        //    MySqlCommand cmd = m_SelectPageCmd;
        //    //create command
        //    if (cmd == null)
        //    {

        //        String query = "SELECT * FROM `pages` WHERE(`pageTitle` = @pageTitle);";
        //        cmd = new MySqlCommand(query, m_connection);
        //        cmd.CommandTimeout = 600;
        //        //add parameters
        //        cmd.Parameters.Add("@pageTitle", MySqlDbType.VarChar);

        //        m_SelectPageCmd = cmd;
        //    }
        //    //execute command
        //    cmd.Parameters[0].Value = title;
        //    MySqlDataReader rdr = cmd.ExecuteReader();

        //    //читаем ридер, закрываем его и возвращаем результат.
        //    return ProcessWikiPageReader(rdr, withText);
        //}

        ///// <summary>
        ///// NT-Получить список страниц для обработки
        ///// </summary>
        ///// <param name="start">Начальный идентификатор в выборке</param>
        ///// <param name="count">Размер выборки, строк</param>
        ///// <param name="withText">Загружать также текст страницы</param>
        //public List<WikiPage> GetPages(Int64 start, int count, bool withText)
        //{

        //    MySqlCommand cmd = m_SelectPageRangeCmd;
        //    //create command
        //    if (cmd == null)
        //    {
        //        String query = "SELECT * FROM `pages` WHERE((`id` >= @idfrom) AND (`id` < @idto));";
        //        cmd = new MySqlCommand(query, m_connection);
        //        cmd.CommandTimeout = 600;
        //        //add parameters
        //        cmd.Parameters.Add("@idfrom", MySqlDbType.Int64);
        //        cmd.Parameters.Add("@idto", MySqlDbType.Int64);
        //        m_SelectPageRangeCmd = cmd;
        //    }
        //    //execute command
        //    cmd.Parameters[0].Value = start;
        //    cmd.Parameters[1].Value = start + count;
        //    MySqlDataReader rdr = cmd.ExecuteReader();

        //    //читаем ридер, закрываем его и возвращаем результат.
        //    return ProcessWikiPageReader(rdr, withText);
        //}

        ///// <summary>
        ///// NT-собрать данные ридера в список страниц
        ///// </summary>
        ///// <param name="rdr"></param>
        ///// <returns></returns>
        //private List<WikiPage> ProcessWikiPageReader(MySqlDataReader rdr, bool withText)
        //{

        //    List<WikiPage> lwp = new List<WikiPage>();

        //    if (rdr.HasRows)
        //    {
        //        while (rdr.Read())
        //        {
        //            WikiPage p = new WikiPage();

        //            p.m_Id = rdr.GetInt32(0);
        //            p.m_Namespace = rdr.GetString(1);
        //            p.m_Title = rdr.GetString(2);
        //            p.m_RedirectTitle = rdr.GetString(3);
        //            p.m_UserName = rdr.GetString(4);
        //            p.m_Model = rdr.GetString(5);
        //            p.m_Format = rdr.GetString(6);
        //            p.m_Comment = rdr.GetString(7);
        //            //если нужен также текст страницы, то и его получаем
        //            if (withText)
        //                p.m_Text = rdr.GetString(8);

        //            lwp.Add(p);
        //        }
        //    }
        //    //закрываем ридер
        //    rdr.Close();

        //    return lwp;
        //}
        #endregion

        #region Функции пользовательские

        //TODO: Добавить код новых функций здесь, каждый комплект функций для таблицы поместить в отдельный region
        //новые команды для них обязательно внести в ClearCommands(), иначе транзакции будут работать неправильно. 

        #endregion

    }//end class
}
