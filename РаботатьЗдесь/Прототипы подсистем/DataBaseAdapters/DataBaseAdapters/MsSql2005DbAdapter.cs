using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Globalization;
using System.Data.SqlTypes;
using System.Data;

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
    /// NR-Класс адаптера БД 
    /// </summary>
    public class MsSql2005DbAdapter
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
        protected string m_connectionString;
        /// <summary>
        /// database connection
        /// </summary>
        protected SqlConnection m_connection;
        /// <summary>
        /// Transaction for current connection
        /// </summary>
        protected SqlTransaction m_transaction;
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
        protected SqlCommand m_cmdWithoutArguments;
        //private SqlCommand m_cmd;
        //private SqlCommand m_cmd;
        //private SqlCommand m_cmd;

        //! Новые команды добавить в ClearCommands()!

        #endregion

        /// <summary>
        /// NT-Normal constructor
        /// </summary>
        public MsSql2005DbAdapter()
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
        public MsSql2005DbAdapter(string connectionString, bool open)
        {
            ClearCommands();
            m_Timeout = 60;
            m_ReadOnly = false;
            m_connectionString = connectionString;
            this.m_connection = new SqlConnection(connectionString);

            if (open == true)
                this.m_connection.Open();

            return;
        }

        /// <summary>
        /// NT-Close and dispose connection
        /// </summary>
        ~MsSql2005DbAdapter()
        {
            this.Close();
        }
        
        #region  Properties

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
                this.m_connection = new SqlConnection(m_connectionString);
            }
        }
        /// <summary>
        /// Is connection opened?
        /// </summary>
        public bool isConnectionActive
        {
            get { return ((this.m_connection != null) && (this.m_connection.State == ConnectionState.Open)) ; }
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
            //m_cmd4 = null;
            //m_cmd5 = null;
            //m_cmd6 = null;
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
        /// RT-Create connection string
        /// </summary>
        /// <param name="DatabaseServerPath"></param>
        /// <param name="DatabaseName"></param>
        /// <param name="UserPassword"></param>
        /// <param name="UserName"></param>
        /// <param name="Timeout"></param>
        /// <param name="IntegratedSecurity">True - Windows autentification. False - SQL Server autentification.</param>
        /// <returns></returns>
        /// <remarks>Без лога, или проверять его существование!</remarks>
        public static string createConnectionString(string DatabaseServerPath, string DatabaseName, string UserPassword, string UserName, int Timeout, bool IntegratedSecurity)
        {
                //create connection string
                SqlConnectionStringBuilder scb = new SqlConnectionStringBuilder();
                scb.ConnectTimeout = Timeout;
                scb.DataSource = DatabaseServerPath;
                scb.IntegratedSecurity = IntegratedSecurity;
                scb.InitialCatalog = DatabaseName;
                scb.Password = UserPassword;
                scb.UserID = UserName;

                return scb.ConnectionString;
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
            ClearCommands(); //надо ли сбросить m_transactions = null?
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

        /// <summary>
        /// NT-Исполнить командный запрос SQL
        /// Например, создать таблицу или индекс.
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="timeout">Таймаут команды в секундах</param>
        /// <returns>Возвращает число затронутых строк таблицы</returns>
        public int ExecuteNonQuery(string query, int timeout)
        {
            SqlCommand cmd = new SqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            return cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// NT- Исполнить запрос с целочисленным результатом.
        /// Например, MAX() или COUNT()
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="timeout">Таймаут команды в секундах</param>
        /// <returns>Возвращает результат - целое число, или -1 при ошибке.</returns>
        public int ExecuteScalar(string query, int timeout)
        {
            SqlCommand cmd = new SqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            SqlDataReader rdr = cmd.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            SqlInt32 res = rdr.GetSqlInt32(0);
            rdr.Close();
            if (res.IsNull) return -1;
            else return res.Value;
        }

        //стандартные функции
        /// <summary>
        /// NT-Получить строку текста из ридера таблицы или пустую строку
        /// </summary>
        /// <param name="rdr">Объект ридера таблицы бд</param>
        /// <param name="p">Номер столбца в таблице и ридере</param>
        /// <returns>Возвращает строку текста из поля или пустую строку если в поле хранится значение DbNull</returns>
        public static string getDbString(SqlDataReader rdr, int p)
        {
            if (rdr.IsDBNull(p))
                return String.Empty;
            else return rdr.GetString(p).Trim();
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, this.m_transaction);
                m_cmdWithoutArguments.CommandTimeout = timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) AS Expr1 FROM {1} {2};", column, table, WhereClause);
            m_cmdWithoutArguments.CommandText = query;
            //get result
            SqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            SqlInt32 res = rdr.GetSqlInt32(0);
            rdr.Close();
            if (res.IsNull) return -1;
            else return res.Value;
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, this.m_transaction);
                m_cmdWithoutArguments.CommandTimeout = timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1} WHERE ({0} = {2});", column, table, val);
            m_cmdWithoutArguments.CommandText = query;
            //get result
            SqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            SqlInt32 res = rdr.GetSqlInt32(0);
            rdr.Close();
            if (res.IsNull) return -1;
            else return res.Value;
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MAX({0}) AS Expr1 FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            SqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            SqlInt32 res = rdr.GetSqlInt32(0);
            rdr.Close();
            if (res.IsNull) return -1;
            else return res.Value;
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MIN({0}) AS Expr1 FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            SqlDataReader rdr = m_cmdWithoutArguments.ExecuteReader(); //Тут могут быть исключения из-за другого типа данных
            rdr.Read();
            SqlInt32 res = rdr.GetSqlInt32(0);
            rdr.Close();
            if (res.IsNull) return -1;
            else return res.Value;
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
                this.m_cmdWithoutArguments = new SqlCommand(query, m_connection, m_transaction);
                this.m_cmdWithoutArguments.CommandTimeout = 600;
            }
            this.m_cmdWithoutArguments.CommandText = query;
            //get result
            SqlDataReader rdr = this.m_cmdWithoutArguments.ExecuteReader();
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, m_transaction);
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
                query = String.Format("SELECT {0}.{1} FROM {0}", tablename, column);
            else
                query = String.Format("SELECT {0}.{1} FROM {0} WHERE ({2})", tablename, column, where);
            //create command
            SqlCommand cmd = new SqlCommand(query, m_connection, m_transaction);
            cmd.CommandTimeout = 600;
            //get result
            SqlDataReader rdr = cmd.ExecuteReader();
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
        /// </summary>
        /// <returns></returns>
        public static DateTime getSqlMinDatetime()
        {
            return new DateTime(1980, 1, 1, 0, 0, 0);
        }

        /// <summary>
        /// NT- собирает список ИД в строку для WHERE
        /// </summary>
        /// <param name="postIdList">Список ИД</param>
        /// <param name="columnName">Название столбца запроса</param>
        /// <returns></returns>
        /// <example>
        /// Это пример функции которая использует данную функцию для выборки нескольких строк за один запрос
        /// Такой прием ускоряет выборку множества строк из большой БД в 6..10 раз
        /// public List<PostsObj> getPostsByIds(List<int> idList)
        ///{
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
        ///}
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
                m_cmdWithoutArguments = new SqlCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0};", table);
            m_cmdWithoutArguments.CommandText = query;
            m_cmdWithoutArguments.ExecuteNonQuery();

            return;
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


        /// <summary>
        /// NT-Create database. User must be member of dbcreator role.
        /// </summary>
        /// <param name="serverPath">Path to SQL Server</param>
        /// <param name="userName">User login</param>
        /// <param name="userPassword">User password</param>
        /// <param name="dbName">New database name</param>
        /// <remarks>
        /// try create new database. 
        /// User must have "dbcreator" role of SqlServer. 
        /// In created database user is "dbo".
        ///  
        ///  </remarks>
        public static void DatabaseCreate(string serverPath, string userName, string userPassword, string dbName, int timeout)
        {
            //try create new database. User must have "dbcreator" role of SqlServer. In created database user is "dbo".
            string constr = createConnectionString(serverPath, "master", userPassword, userName, timeout, false);
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            SqlCommand cmd = new SqlCommand(String.Format("CREATE DATABASE {0}", dbName), con);
            cmd.ExecuteNonQuery();
            con.Close();
            return;
        }

        /// <summary>
        /// NFT-Delete database from SQL server
        /// </summary>
        /// <param name="srvpath">SQL server path</param>
        /// <param name="userName">User login</param>
        /// <param name="userPsw">User password</param>
        /// <param name="dbName">Database name</param>
        public static void DatabaseDelete(string srvpath, string userName, string userPsw, string dbName, int timeout)
        {
            string constr = createConnectionString(srvpath, "master", userPsw, userName, 300, false);
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            //disconnect any users and remove database from server
            SqlCommand cmd = new SqlCommand(String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {0};", dbName), con);
            cmd.ExecuteNonQuery();
            con.Close();
        }

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
            SqlCommand cmd = new SqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //execute command
            SqlDataReader rdr = cmd.ExecuteReader();
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
            SqlCommand cmd = new SqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            cmd.ExecuteNonQuery();
            //2 - записать новые значения
            query = String.Format("INSERT INTO {0} (param, val) VALUES (?, ?);", table);
            cmd.CommandText = query;
            cmd.Parameters.Add(new SqlParameter("@p0", SqlDbType.NVarChar));
            cmd.Parameters.Add(new SqlParameter("@p1", SqlDbType.NVarChar));
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
            SqlCommand cmd = new SqlCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //execute command
            SqlDataReader rdr = cmd.ExecuteReader();
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
        ///// NT-Create database with tables with indexes
        ///// </summary>
        //internal static void CreateDatabaseTables(string srvpath, string userName, string userPsw, string newDbName)
        //{
        //    //try create new database. User must have "dbcreator" role of SqlServer. In created database user is "dbo".
        //    string constr = MDbLayer.createConnectionString(srvpath, "master", userPsw, userName, 30, false);
        //    SqlConnection con = new SqlConnection(constr);
        //    con.Open();
            
        //    SqlCommand cmd = new SqlCommand(String.Format("CREATE DATABASE {0}", newDbName), con);
        //    cmd.ExecuteNonQuery();
        //    //wait for ready ?
        //    con.ChangeDatabase(newDbName);

        //    //create tables and more
        //    string tt0 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[CellTable](	[id] [int] IDENTITY(1,1) NOT NULL, 	[name] [nvarchar](440) NOT NULL, [descr] [ntext] NULL,	[active] [bit] NOT NULL, [type] [int] NOT NULL, [creatime] [datetime] NOT NULL, [moditime] [datetime] NOT NULL, [ronly] [bit] NOT NULL, [state] [int] NOT NULL, [sflag] [int] NOT NULL, [val] [varbinary](max) NULL, [valtype] [int] NOT NULL, [cellid] [int] NOT NULL, CONSTRAINT [PK_CellTable] PRIMARY KEY CLUSTERED ( [id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END;";
        //    cmd.CommandText = tt0;
        //    cmd.ExecuteNonQuery();
        //    /****** Объект:  Index [IX_CellTable_cellid]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt1 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND name = N'IX_CellTable_cellid') CREATE UNIQUE NONCLUSTERED INDEX [IX_CellTable_cellid] ON [dbo].[CellTable] ([cellid] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt1;
        //    cmd.ExecuteNonQuery();
        //    /****** Объект:  Index [IX_CellTable_name]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt2 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND name = N'IX_CellTable_name') CREATE NONCLUSTERED INDEX [IX_CellTable_name] ON [dbo].[CellTable] ([name] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt2;
        //    cmd.ExecuteNonQuery();

        //    //linktable
        //    string tt3 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[LinkTable](	[id] [int] IDENTITY(1,1) NOT NULL,	[downID] [int] NOT NULL,	[upID] [int] NOT NULL,	[axis] [int] NOT NULL,	[state] [int] NOT NULL,	[active] [bit] NOT NULL, [sflag] [int] NOT NULL, [descr] [ntext] NULL, [moditime] [datetime] NOT NULL, CONSTRAINT [PK_LinkTable] PRIMARY KEY CLUSTERED ([id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END";
        //    cmd.CommandText = tt3;
        //    cmd.ExecuteNonQuery();
        //    /****** Объект:  Index [IX_LinkTable_downID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt4 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_downID') CREATE NONCLUSTERED INDEX [IX_LinkTable_downID] ON [dbo].[LinkTable] ([downID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt4;
        //    cmd.ExecuteNonQuery();
        //    /****** Объект:  Index [IX_LinkTable_updownID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt5 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_updownID') CREATE NONCLUSTERED INDEX [IX_LinkTable_updownID] ON [dbo].[LinkTable] ([upID] ASC, [downID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt5;
        //    cmd.ExecuteNonQuery();
        //    /****** Объект:  Index [IX_LinkTable_upID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt6 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_upID') CREATE NONCLUSTERED INDEX [IX_LinkTable_upID] ON [dbo].[LinkTable] ([upID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt6;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Table [dbo].[EngineTable]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt7 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EngineTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[EngineTable]([id] [int] IDENTITY(1,1) NOT NULL, [version] [int] NOT NULL CONSTRAINT [DF_EngineTable_version]  DEFAULT ((23000)), [step] [int] NOT NULL CONSTRAINT [DF_EngineTable_step]  DEFAULT ((0)), [lognum] [int] NOT NULL CONSTRAINT [DF_EngineTable_lognum]  DEFAULT ((0)), [loglevel] [int] NOT NULL CONSTRAINT [DF_EngineTable_loglevel]  DEFAULT ((0)), [descr] [ntext] NULL, [name] [nvarchar](max) NULL, [sflag] [int] NOT NULL, [state] [int] NOT NULL, [cellmode] [int] NOT NULL CONSTRAINT [DF_EngineTable_cellmode]  DEFAULT ((0)), [idcon] [int] NOT NULL CONSTRAINT [DF_EngineTable_idcon]  DEFAULT ((0)), CONSTRAINT [PK_EngineTable] PRIMARY KEY CLUSTERED ([id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END";
        //    cmd.CommandText = tt7;
        //    cmd.ExecuteNonQuery();

        //    con.Close();

        //}

        /////<summary>
        /////NT-Create tables and indexes in existing database
        ///// </summary>
        /////<param name="serverPath">Path to SQL Server</param>
        /////<param name="userName">User login</param>
        /////<param name="userPassword">User password</param>
        /////<param name="dbName">Existing database name</param>
        //internal static void CreateTablesIndexes(string serverPath, string userName, string userPassword, string dbName)
        //{
        //    string constr = MDbLayer.createConnectionString(serverPath, dbName, userPassword, userName, 30, false);
        //    SqlConnection con = new SqlConnection(constr);
        //    con.Open();
          
        //    CreateTablesIndexes(con);

        //    con.Close();
        //    return;
        //}

        ///// <summary>
        ///// NT-Create Tables and Indexes. For static or object operations
        ///// </summary>
        ///// <param name="con">Opened Sql connection</param>
        //private static void CreateTablesIndexes(SqlConnection con)
        //{
        //    SqlCommand cmd = new SqlCommand("", con);

        //    string tt0 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[CellTable](	[id] [int] IDENTITY(1,1) NOT NULL, 	[name] [nvarchar](440) NOT NULL, [descr] [ntext] NULL,	[active] [bit] NOT NULL, [type] [int] NOT NULL, [creatime] [datetime] NOT NULL, [moditime] [datetime] NOT NULL, [ronly] [bit] NOT NULL, [state] [int] NOT NULL, [sflag] [int] NOT NULL, [val] [varbinary](max) NULL, [valtype] [int] NOT NULL, [cellid] [int] NOT NULL, CONSTRAINT [PK_CellTable] PRIMARY KEY CLUSTERED ( [id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END;";
        //    cmd.CommandText = tt0;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Index [IX_CellTable_cellid]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt1 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND name = N'IX_CellTable_cellid') CREATE UNIQUE NONCLUSTERED INDEX [IX_CellTable_cellid] ON [dbo].[CellTable] ([cellid] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt1;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Index [IX_CellTable_name]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt2 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CellTable]') AND name = N'IX_CellTable_name') CREATE NONCLUSTERED INDEX [IX_CellTable_name] ON [dbo].[CellTable] ([name] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt2;
        //    cmd.ExecuteNonQuery();

        //    //linktable
        //    string tt3 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[LinkTable](	[id] [int] IDENTITY(1,1) NOT NULL,	[downID] [int] NOT NULL,	[upID] [int] NOT NULL,	[axis] [int] NOT NULL,	[state] [int] NOT NULL,	[active] [bit] NOT NULL, [sflag] [int] NOT NULL, [descr] [ntext] NULL, [moditime] [datetime] NOT NULL, CONSTRAINT [PK_LinkTable] PRIMARY KEY CLUSTERED ([id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END";
        //    cmd.CommandText = tt3;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Index [IX_LinkTable_downID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt4 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_downID') CREATE NONCLUSTERED INDEX [IX_LinkTable_downID] ON [dbo].[LinkTable] ([downID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt4;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Index [IX_LinkTable_updownID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt5 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_updownID') CREATE NONCLUSTERED INDEX [IX_LinkTable_updownID] ON [dbo].[LinkTable] ([upID] ASC, [downID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt5;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Index [IX_LinkTable_upID]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt6 = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LinkTable]') AND name = N'IX_LinkTable_upID') CREATE NONCLUSTERED INDEX [IX_LinkTable_upID] ON [dbo].[LinkTable] ([upID] ASC) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]";
        //    cmd.CommandText = tt6;
        //    cmd.ExecuteNonQuery();

        //    /****** Объект:  Table [dbo].[EngineTable]    Дата сценария: 06/01/2012 20:33:25 ******/
        //    string tt7 = "SET ANSI_NULLS ON; SET QUOTED_IDENTIFIER ON; IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EngineTable]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[EngineTable]([id] [int] IDENTITY(1,1) NOT NULL, [version] [int] NOT NULL CONSTRAINT [DF_EngineTable_version]  DEFAULT ((23000)), [step] [int] NOT NULL CONSTRAINT [DF_EngineTable_step]  DEFAULT ((0)), [lognum] [int] NOT NULL CONSTRAINT [DF_EngineTable_lognum]  DEFAULT ((0)), [loglevel] [int] NOT NULL CONSTRAINT [DF_EngineTable_loglevel]  DEFAULT ((0)), [descr] [ntext] NULL, [name] [nvarchar](max) NULL, [sflag] [int] NOT NULL, [state] [int] NOT NULL, [cellmode] [int] NOT NULL CONSTRAINT [DF_EngineTable_cellmode]  DEFAULT ((0)), [idcon] [int] NOT NULL CONSTRAINT [DF_EngineTable_idcon]  DEFAULT ((0)), CONSTRAINT [PK_EngineTable] PRIMARY KEY CLUSTERED ([id] ASC)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END";
        //    cmd.CommandText = tt7;
        //    cmd.ExecuteNonQuery();

        //    return;
        //}

        ///// <summary>
        ///// Delete CellTable LinkTable from database
        ///// </summary>
        //private void DeleteTablesCellLink()
        //{
        //    string query = "DROP TABLE CellTable";
        //    SqlCommand t = new SqlCommand(query, m_connection);
        //    t.CommandTimeout = 60;
        //    t.ExecuteNonQuery();
        //    //linktable
        //    t.CommandText = "DROP TABLE LinkTable";
        //    t.ExecuteNonQuery();
        //    return;
        //}

        ///// <summary>
        ///// NT-Remove all from celltable and linktable
        ///// </summary>
        //internal void ClearCellAndLinkTables()
        //{
        //    //delete celltable and linktable from database
        //    DeleteTablesCellLink();

        //    //recreate tables and indexes
        //    CreateTablesIndexes(m_connection);
        //    return;
        //}
#endregion


        #region Функции пользовательские

        //TODO: Добавить код новых функций здесь, каждый комплект функций для таблицы поместить в отдельный region
        //новые команды для них обязательно внести в ClearCommands(), иначе транзакции будут работать неправильно. 

        #endregion


    }//end class
}
