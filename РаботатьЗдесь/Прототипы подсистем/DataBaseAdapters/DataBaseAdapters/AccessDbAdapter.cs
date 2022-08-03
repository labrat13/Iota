using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Globalization;
using System.IO;

namespace DataBaseAdapters
{
    /*  Состояние шаблона:
     * 23 декабря 2019 г - начальная версия. 
     * ReadOnly проперти и переменная не устанавливаются и всегда возвращают значение false.
     * Компилируется, но не знаю, правильно ли работает.
     * 04 января 2020 - добавлена поддержка наследования для производных классов.
     * Производные классы позволяют сосредоточиться на новом коде. 
    */

    /* Зарезервированные слова, которые надо заключать в квадратные скобки в запросах 
     * нежелательные имена для столбцов и таблиц, в любом регистре:
     *  text module session 
    */

    /* Если вставлять строки без открытой транзакции, они вставляются медленнее, чем внутри открытой транзакции.
     * Но после закрытия транзакции надо закрыть соединение с БД (для MsSqlServer). Иначе код выдает ошибку.
     */

    /* В проектах следует наследовать класс адаптера, чтобы не блуждать в функциях базового класса.
     * Это позволит сосредоточиться на функционале проекта. Меньше текста - легче думать.
     * Пример наследного класса:
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
    */

    /// <summary>
    /// NT-Класс адаптера БД 
    /// </summary>
    public class AccessDbAdapter
    {
        /// <summary>
        /// Расширение файла базы данных
        /// </summary>
        public const string DatabaseFileExtension = ".mdb";


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
        protected OleDbConnection m_connection;
        /// <summary>
        /// Transaction for current connection
        /// </summary>
        protected OleDbTransaction m_transaction;
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
        protected OleDbCommand m_cmdWithoutArguments;
        //private OleDbCommand m_cmd1;
        //private OleDbCommand m_cmd2;
        //private OleDbCommand m_cmd3;
        //private OleDbCommand m_cmd4;
        //private OleDbCommand m_cmd5;
        //private OleDbCommand m_cmd6;

        #endregion

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        public AccessDbAdapter()
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
        public AccessDbAdapter(string connectionString, bool open)
        {
            ClearCommands();
            m_Timeout = 60;
            m_ReadOnly = false;
            m_connectionString = connectionString;
            this.m_connection = new OleDbConnection(connectionString);

            if (open == true)
                this.m_connection.Open();

            return;
        }

        /// <summary>
        /// NT-Close and dispose connection
        /// </summary>
        ~AccessDbAdapter()
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
                this.m_connection = new OleDbConnection(m_connectionString);
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
        /// NT-Создать строку соединения с БД
        /// </summary>
        /// <param name="dbFile">Путь к файлу БД</param>
        public static string createConnectionString(string dbFile, bool readOnly)
        {
            //Provider=Microsoft.Jet.OLEDB.4.0;Data Source="C:\Documents and Settings\salomatin\Мои документы\Visual Studio 2008\Projects\RadioBase\радиодетали.mdb"
            OleDbConnectionStringBuilder b = new OleDbConnectionStringBuilder();
            b.Provider = "Microsoft.Jet.OLEDB.4.0";
            b.DataSource = dbFile;
            //это только для БД на незаписываемых дисках
            if (readOnly)
            {
                b.Add("Mode", "Share Deny Write");
            }
            //user id and password can specify here
            return b.ConnectionString;
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

        /// <summary>
        /// NT-Исполнить командный запрос SQL
        /// Например, создать таблицу или индекс.
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="timeout">Таймаут команды в секундах</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string query, int timeout)
        {
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection, this.m_transaction);
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
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            Object ob = cmd.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
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
        private string makeWhereText(List<int> postIdList, string columnName)
        {
            //returns (Id = 0) OR (Id = 1) OR (Id = 3)

            int cnt = postIdList.Count;
            String[] sar = new string[cnt];
            //
            for (int i = 0; i < cnt; i++)
                sar[i] = String.Format("({0} = {1})", columnName, postIdList[i]);
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
        /// NT-Извлечь файл шаблона базы данных из ресурсов сборки
        /// </summary>
        /// <remarks>
        /// Файл БД должен быть помещен в ресурсы сборки в VisualStudio2008.
        /// Окно Свойства проекта - вкладка Ресурсы - кнопка-комбо Добавить ресурс - Добавить существующий файл. Там выбрать файл БД.
        /// При этом он помещается также в дерево файлов проекта, и при компиляции берется оттуда и помещается в сборку как двоичный массив байт.
        /// После этого можно изменять этот файл проекта, изменения в ресурс сборки будут внесены после компиляции
        /// Эта функция извлекает файл БД в указанный путь файла.
        /// </remarks>
        /// <param name="filepath">Путь к итоговому файлу *.mdb</param>
        /// <param name="resourceStream">содержимое ресурса</param>
        /// <example>
        /// dbAdapter.extractDbFile("C:\\db.mdb", Properties.Resources.db);
        /// </example>
        public static void extractDbFile(string filepath, byte[] resourceBytes)
        {
            FileStream fs = new FileStream(filepath, FileMode.Create);
            fs.Write(resourceBytes, 0, resourceBytes.Length);
            fs.Close();

            return;
        }

        /// <summary>
        /// NT-Получить строку текста из ридера таблицы или пустую строку
        /// </summary>
        /// <param name="rdr">Объект ридера таблицы бд</param>
        /// <param name="p">Номер столбца в таблице и ридере</param>
        /// <returns>Возвращает строку текста из поля или пустую строку если в поле хранится значение DbNull</returns>
        public static string getDbString(OleDbDataReader rdr, int p)
        {
            if (rdr.IsDBNull(p))
                return String.Empty;
            else return rdr.GetString(p).Trim();
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
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MAX({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
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
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MIN({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

        /// <summary>
        /// NT-Получить число записей в таблице
        /// </summary>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца первичного ключа</param>
        /// <returns>Returns row count or -1 if no results</returns>
        public int GetRowCount(string table, string column)
        {
            //SELECT COUNT(id) FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1};", column, table);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

        /// <summary>
        /// NT-Получить число записей в таблице, с указанным числовым значением.
        /// </summary>
        /// <remarks>Применяется для столбца первичного ключа, проверяет что запись с этим ключом существует.
        /// Но может применяться и в других случаях.
        /// </remarks>
        /// <param name="table">Название таблицы</param>
        /// <param name="column">Название столбца</param>
        /// <param name="val">Числовое значение в столбце</param>
        /// <returns>Возвращает число записей с таким значением этого столбца, или -1 при ошибке.</returns>
        public int GetRowCount(string table, string column, int val)
        {
            //SELECT column FROM table WHERE (column = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 120;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT({0}) FROM {1} WHERE ({0} = {2});", column, table, val);
            m_cmdWithoutArguments.CommandText = query;
            Object ob = m_cmdWithoutArguments.ExecuteScalar(); //Тут могут быть исключения из-за другого типа данных
            String s = ob.ToString();
            if (String.IsNullOrEmpty(s))
                return -1;
            else return Int32.Parse(s);
        }

        /// <summary>
        /// NT-Проверить существование записи с указанным Id номером
        /// </summary>
        /// <param name="tablename">Название таблицы</param>
        /// <param name="column">Название столбца идентификатора</param>
        /// <param name="idValue">Значение идентификатора записи</param>
        /// <returns>Возвращает True если запись существует, иначе возвращает False.</returns>
        public bool IsRowExists(String tablename, string column, Int32 idValue)
        {
            String query = String.Format("SELECT {0} FROM {1} WHERE ({0} = {2})", column, tablename, idValue);
            if (this.m_cmdWithoutArguments == null)
            {
                //create command
                this.m_cmdWithoutArguments = new OleDbCommand(query, m_connection, m_transaction);
                this.m_cmdWithoutArguments.CommandTimeout = 600;
            }
            this.m_cmdWithoutArguments.CommandText = query;
            //get result
            OleDbDataReader rdr = this.m_cmdWithoutArguments.ExecuteReader();
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
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 120;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE ({0}.{1} = {2});", table, column, val);
            m_cmdWithoutArguments.CommandText = query;
            return m_cmdWithoutArguments.ExecuteNonQuery(); //Тут могут быть исключения из-за другого типа данных
        }

        /// <summary>
        /// NT-получить значение автоинкремента для последней измененной таблицы в текущем сеансе БД
        /// </summary>
        /// <returns></returns>
        public int GetLastAutonumber()
        {
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 60;
            }
            //execute command
            m_cmdWithoutArguments.CommandText = "SELECT @@IDENTITY;";
            return (int)m_cmdWithoutArguments.ExecuteScalar();
        }

        /// <summary>
        /// NT-Получить список ид записей таблицы
        /// </summary>
        /// <param name="tablename">Название таблицы</param>
        /// <param name="column">Название столбца ИД типа Int32</param>
        /// <param name="where">текст условия отбора. Если отбор не требуется, то передать пустую строку или null. Пример: id > 100 </param>
        /// <returns>Возвращает список идентификаторов записей таблицы</returns>
        public List<Int32> getListOfIds(String tablename, string column, string where)
        {
            String query;
            //create query text
            if (String.IsNullOrEmpty(where))
                query = String.Format("SELECT {1} FROM {0}", tablename, column);
            else
                query = String.Format("SELECT {1} FROM {0} WHERE ({2})", tablename, column, where);
            //create command
            OleDbCommand cmd = new OleDbCommand(query, m_connection, m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //get result
            OleDbDataReader rdr = cmd.ExecuteReader();
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
        /// NT-Удалить все строки из указанной таблицы.
        /// Счетчик первичного ключа не сбрасывается - его отдельно надо сбрасывать.
        /// </summary>
        /// <param name="table">Название таблицы</param>
        public void TableClear(string table)
        {
            //DELETE FROM table;
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new OleDbCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 600;
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
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            //execute command
            OleDbDataReader rdr = cmd.ExecuteReader();
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
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            cmd.ExecuteNonQuery();
            //2 - записать новые значения
            query = String.Format("INSERT INTO {0} (param, val) VALUES (?, ?);", table);
            cmd.CommandText = query;
            cmd.Parameters.Add(new OleDbParameter("@p0", OleDbType.VarWChar));
            cmd.Parameters.Add(new OleDbParameter("@p1", OleDbType.VarWChar));
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
            OleDbCommand cmd = new OleDbCommand(query, this.m_connection);
            cmd.CommandTimeout = 120;
            //execute command
            OleDbDataReader rdr = cmd.ExecuteReader();
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
        ///// NT-прочитать одну строку таблицы в объект
        ///// </summary>
        ///// <param name="rdr">OleDbDataReader объект, готовый к чтению строки таблицы</param>
        ///// <param name="readAll">True - загружать текст заметок, False - не загружать.</param>
        ///// <returns>Объект категории</returns>
        ///// <example>
        ///// //создали SqlCommand, заполнили и вызвали ее. Теперь читаем выборку.
        /////    if (rdr.HasRows)
        /////    {
        /////        while (rdr.Read())
        /////        {
        /////            CDbObject c = (CDbObject)ReadItemRow(rdr, readAll);
        /////            li.Add(c);
        /////            //limit output to specified "count" 
        /////            if (li.Count >= count) break;
        /////        }
        /////    }
        /////    //close reader
        /////    rdr.Close();
        /////    return li;
        ///// </example>
        //private static CCategory ReadCategoryRow(OleDbDataReader rdr, bool readAll)
        //{
        //    CCategory c = new CCategory();
        //    c.ObjectId.ElementId = rdr.GetInt32(0); //id
        //    c.Title = rdr.GetString(1);//title
        //    c.WebTitle = rdr.GetString(2);//shtitle
        //    c.Deleted = rdr.GetBoolean(3);//deleted
        //    //read icon here
        //    c.Picture = createImageFromReader(rdr, 4);//icon
        //    c.Description = rdr.GetString(5);//descr
        //    c.ObjectId.ParentCategoryId = rdr.GetInt32(6);//parent
        //    if (readAll == true)
        //    {
        //        c.Notes = rdr.GetString(7);//notes
        //    }
        //    return c;
        //}
        
        ///// <summary>
        ///// NT-Создать картинку из записи БД
        ///// </summary>
        ///// <param name="rdr">ридер строки таблицы</param>
        ///// <param name="index">номер столбца таблицы, с картинкой</param>
        ///// <returns>Объект Image или null, если нет в БД картинки</returns>
        //private static Image createImageFromReader(OleDbDataReader rdr, int index)
        //{
        //    //надо обрабатывать NULL в столбце с картинкой - возвращать null вместо картинки
        //    //get size of data
        //    //проверить, что размер данных соответствует фактическому
        //    Int64 len = 0;
        //    len = rdr.GetBytes(index, 0, null, 0, 12345678);
        //    if (len == 0) return null;
        //    else
        //    {
        //        //create buffer and read bytes
        //        Byte[] buff = new Byte[len];
        //        rdr.GetBytes(index, 0, buff, 0, (Int32)len);
        //        //make image
        //        Image img = CImageProcessor.ImageFromBytes(buff);
        //        return img;
        //    }
        //}

        ///// <summary>
        ///// NT-собрать строку запроса для поиска элементов
        ///// </summary>
        ///// <param name="tablename"></param>
        ///// <param name="pattern"></param>
        ///// <param name="sf"></param>
        ///// <returns></returns>
        //private String makeSearchQuery(String tablename, String pattern, SearchFields sf)
        //{
        //    //привести строку запроса из общепринятого к специфичному для БД виду
        //    //и удалить % с начала и конца, если они там есть
        //    String patternDb = pattern.Replace('*', '%');
        //    char[] trimchars = new char[] { '%' };
        //    patternDb = patternDb.TrimEnd(trimchars);
        //    patternDb = patternDb.TrimStart(trimchars);

        //    StringBuilder sb = new StringBuilder(256);
        //    String pat = " LIKE '%" + patternDb + "%') OR ";
        //    sb.Append("SELECT * FROM ");
        //    sb.Append(tablename);
        //    sb.Append(" WHERE (");
        //    //check search flags
        //    if ((sf & SearchFields.Title) != 0) { sb.Append(" (title "); sb.Append(pat); }
        //    if ((sf & SearchFields.Webtitle) != 0) { sb.Append(" (shtitle "); sb.Append(pat); }
        //    if ((sf & SearchFields.Description) != 0) { sb.Append(" (descr "); sb.Append(pat); }
        //    if ((sf & SearchFields.Notes) != 0) { sb.Append(" (notes "); sb.Append(pat); }
        //    sb.Append("(0=1));");

        //    return sb.ToString();
        //}

        ///// <summary>
        ///// NT-Поиск элементов по паттерну
        ///// </summary>
        ///// <param name="pattern">Паттерн для поиска</param>
        ///// <param name="sf">Флаги полей, в которых искать</param>
        ///// <param name="count">Лимит выборки</param>
        ///// <returns>Список найденных элементов</returns>
        //internal List<CDbObject> FindItems(string pattern, SearchFields sf, int count)
        //{
        //    //тут надо создать строку запроса с параметрами, причем число параметров должно соответствовать числу полей.
        //    //Это такой универсальный код для всех типов поиска, его надо как-то вынести в общую функцию.
        //    string query = this.makeSearchQuery("Items", pattern, sf);
        //    //create command
        //    OleDbCommand cmd = new OleDbCommand(query, this.m_connection, m_transaction);
        //    cmd.CommandTimeout = 600;
        //    ////execute command
        //    //read results
        //    OleDbDataReader rdr = cmd.ExecuteReader();
        //    List<CDbObject> li = new List<CDbObject>();
        //    //если флаг поля заметок указан, загружаем текст заметок
        //    bool readAll = ((sf & SearchFields.Notes) != 0);
        //    //читаем записи
        //    if (rdr.HasRows)
        //    {
        //        while (rdr.Read())
        //        {
        //            CDbObject c = (CDbObject)ReadItemRow(rdr, readAll);
        //            li.Add(c);
        //            //limit output to specified "count" 
        //            if (li.Count >= count) break;
        //        }
        //    }
        //    //close reader
        //    rdr.Close();
        //    return li;
        //}
        #endregion

        #region Функции пользовательские

        //TODO: Добавить код новых функций здесь, каждый комплект функций для таблицы поместить в отдельный region
        //новые команды для них обязательно внести в ClearCommands(), иначе транзакции будут работать неправильно. 

        #endregion

    }//end class
}
