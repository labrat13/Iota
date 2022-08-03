using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Globalization;
using System.Data;
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

    /* Если вставлять строки без открытой транзакции, они вставляются медленнее, чем внутри открытой транзакции.
     * Но после закрытия транзакции надо закрыть соединение с БД (для MsSqlServer). Иначе код выдает ошибку.
     * SQLITE не выдает ошибку при двух последовательных транзакциях!
     */

    /// <summary>
    /// NR-Класс адаптера БД экспериментальный, поскольку БД самобытная 
    /// Наверняка тут куча ошибок просто из-за того что я устал эти классы собирать сегодня.
    /// </summary>
    public class SqliteDbAdapter
    {

        /// <summary>
        /// Расширение файла базы данных
        /// </summary>
        public const string DatabaseFileExtension = ".sqlite";

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
        /// Объект соединения с БД
        /// </summary>
        protected SQLiteConnection m_connection;
        /// <summary>
        /// Transaction for current connection
        /// </summary>
        protected SQLiteTransaction m_transaction;
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
        protected SQLiteCommand m_cmdWithoutArguments;
        //private SQLiteCommand m_cmd1;
        //private SQLiteCommand m_cmd2;
        //private SQLiteCommand m_cmd3;

        #endregion

        /// <summary>
        /// NT-Конструктор
        /// </summary>
        /// <param name="con">Объект соединения с БД</param>
        public SqliteDbAdapter()
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
        /// NT-Конструктор
        /// </summary>
        /// <param name="con">Объект соединения с БД</param>
        public SqliteDbAdapter(string connectionString, bool open)
        {
            ClearCommands();
            m_Timeout = 60;
            m_ReadOnly = false;
            m_connectionString = connectionString;
            this.m_connection = new SQLiteConnection(connectionString);

            if (open == true)
                this.m_connection.Open();

            return;
        }

        /// <summary>
        /// NT-Close and dispose connection
        /// </summary>
        ~SqliteDbAdapter()
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
                this.m_connection = new SQLiteConnection(m_connectionString);
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
        /// NT-Закрыть соединение с БД
        /// </summary>
        /// <summary>
        /// Close connection if not closed
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
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            //open dummy file
            builder.DataSource = dbFile;
            builder.ReadOnly = readOnly;
            builder.FailIfMissing = true;//чтобы выбрасывать исключение если БД не существует. Иначе она будет создаваться новая.
            //password can specify here - но я не уверен что шифрование тут поддерживается
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

        /// <summary>
        /// NT- Создать новый пустой файл для базы данных
        /// </summary>
        /// <param name="filename">Путь к файлу БД</param>
        public static void DatabaseCreate(string filename)
        {
            SQLiteConnection.CreateFile(filename);
            return;
        }

        /// <summary>
        /// NT-Исполнить командный запрос SQL
        /// Например, создать таблицу или индекс.
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="timeout">Таймаут команды в секундах</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string query, int timeout)
        {
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            return cmd.ExecuteNonQuery();
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

        //рассмотреть возможность реализовать эти функции:

        /// <summary>
        /// NT- Исполнить запрос с целочисленным результатом.
        /// Например, MAX() или COUNT()
        /// </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="timeout">Таймаут команды в секундах</param>
        /// <returns>Возвращает результат - целое число, или -1 при ошибке.</returns>
        public int ExecuteScalar(string query, int timeout)
        {
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            //read one result
            Int32 result = -1;
            SQLiteDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                result = rdr.GetInt32(0);
            }
            rdr.Close();
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
        public static string getDbString(SQLiteDataReader rdr, int p)
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
            //SELECT MAX("id") FROM "table";
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, this.m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MAX(\"{0}\") FROM \"{1}\";", column, table);
            m_cmdWithoutArguments.CommandText = query;
            //read one result
            Int32 result = -1;
            SQLiteDataReader rdr = m_cmdWithoutArguments.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                result = rdr.GetInt32(0);
            }
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
            //SELECT MIN("id") FROM "table";
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT MIN(\"{0}\") FROM \"{1}\";", column, table);
            m_cmdWithoutArguments.CommandText = query;
            //read one result
            Int32 result = -1;
            SQLiteDataReader rdr = m_cmdWithoutArguments.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                result = rdr.GetInt32(0);
            }
            rdr.Close();
            return result;
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
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT(\"{0}\") FROM \"{1}\";", column, table);
            m_cmdWithoutArguments.CommandText = query;
            //read one result
            Int32 result = -1;
            SQLiteDataReader rdr = m_cmdWithoutArguments.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                result = rdr.GetInt32(0);
            }
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
        /// <param name="val">Числовое значение в столбце</param>
        /// <returns>Возвращает число записей с таким значением этого столбца, или -1 при ошибке.</returns>
        public int GetRowCount(string table, string column, int val)
        {
            //SELECT "column" FROM "table" WHERE (\"column\" = value);
            if (m_cmdWithoutArguments == null)
            {
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = this.m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "SELECT COUNT(\"{0}\") FROM \"{1}\" WHERE (\"{0}\" = {2});", column, table, val);
            m_cmdWithoutArguments.CommandText = query;
            //read one result
            Int32 result = -1;
            SQLiteDataReader rdr = m_cmdWithoutArguments.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                result = rdr.GetInt32(0);
            }
            rdr.Close();
            return result;
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
            int result = this.GetRowCount(tablename, column, idValue);
            return (result > 0);
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
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = m_Timeout;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM \"{0}\" WHERE (\"{1}\" = {2});", table, column, val);
            m_cmdWithoutArguments.CommandText = query;
            return m_cmdWithoutArguments.ExecuteNonQuery(); //Тут могут быть исключения из-за другого типа данных
        }

        /// <summary>
        /// NT-получить значение автоинкремента для последней измененной таблицы в текущем сеансе БД
        /// </summary>
        /// <returns></returns>
        public int GetLastAutonumber()
        {
            throw new NotImplementedException(); //TODO: Add code here
            
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
        /// <param name="column">Название столбца ИД типа Int32</param>
        /// <param name="where">текст условия отбора. Если отбор не требуется, то передать пустую строку или null. Пример: id > 100 </param>
        /// <returns>Возвращает список идентификаторов записей таблицы</returns>
        public List<Int32> getListOfIds(String tablename, string column, string where)
        {
            String query;
            //create query text
            if (String.IsNullOrEmpty(where))
                query = String.Format("SELECT \"{1}\" FROM \"{0}\"", tablename, column);
            else
                query = String.Format("SELECT \"{1}\" FROM \"{0}\" WHERE ({2})", tablename, column, where);
            //create command
            SQLiteCommand cmd = new SQLiteCommand(query, m_connection, m_transaction);
            cmd.CommandTimeout = m_Timeout;
            //get result
            SQLiteDataReader rdr = cmd.ExecuteReader();
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
                m_cmdWithoutArguments = new SQLiteCommand(String.Empty, this.m_connection, m_transaction);
                m_cmdWithoutArguments.CommandTimeout = 600;
            }
            //execute command
            string query = String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0};", table);
            m_cmdWithoutArguments.CommandText = query;
            m_cmdWithoutArguments.ExecuteNonQuery();

            return;
        }

        ////TODO: Если нужна функция очистки всей БД, раскомментируйте код и измените его, вписав правильные имена таблиц.
        /////// <summary>
        /////// NFT-Очистить БД 
        /////// </summary>
        /////// <returns>True if Success, False otherwise</returns>
        ////internal bool ClearDb()
        ////{
        ////    bool result = false;
        ////    try
        ////    {
        ////        this.TransactionBegin();
        ////        this.TableClear(DbAdapter.ContentTableName);
        ////        this.TableClear(DbAdapter.DocumentTableName);
        ////        this.TableClear(DbAdapter.PictureTableName);
        ////        this.TransactionCommit();
        ////        result = true;
        ////    }
        ////    catch (Exception)
        ////    {
        ////        this.TransactionRollback();
        ////        result = false;
        ////    }
        ////    return result;
        ////}

        #endregion

        #region *** Для таблицы свойств ключ-значение ***

        //рассмотреть возможность реализовать эти функции:

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
            String query = String.Format("SELECT * FROM \"{0}\";", table);
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection);
            cmd.CommandTimeout = this.m_Timeout;
            //execute command
            SQLiteDataReader rdr = cmd.ExecuteReader();
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
            String query = String.Format("DELETE * FROM \"{0}\";", table);
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection);
            cmd.CommandTimeout = this.m_Timeout;
            cmd.ExecuteNonQuery();
            //2 - записать новые значения
            query = String.Format("INSERT INTO \"{0}\" (\"param\", \"val\") VALUES (?, ?);", table);
            cmd.CommandText = query;
            cmd.Parameters.Add("@p0", DbType.String);
            cmd.Parameters.Add("@p1", DbType.String);
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
            String query = String.Format("SELECT * FROM \"{0}\" WHERE (\"{1}\" = '{2}' );", table, columnName, paramName);
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection);
            cmd.CommandTimeout = this.m_Timeout;
            //execute command
            SQLiteDataReader rdr = cmd.ExecuteReader();
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

        // --------------------------------------------------
        // Старый код
        // --------------------------------------

        #region Примеры функций

        //#region *** Symbol database tables ***
        ///// <summary>
        ///// NT-Создать в новой БД таблицу для символов
        ///// </summary>
        ///// <returns></returns>
        //public bool CreateSymbolTable()
        //{
        //    using (SQLiteTransaction mytransaction = m_connection.BeginTransaction())
        //    {
        //        using (SQLiteCommand mycommand = new SQLiteCommand(m_connection))
        //        {
        //            mycommand.CommandText = "CREATE TABLE \"sym\"(\"id\" Integer Primary Key Autoincrement, \"txt\" Text, \"cnt\" Integer DEFAULT(0))";
        //            mycommand.ExecuteNonQuery();
        //            //index
        //            mycommand.CommandText = "CREATE UNIQUE INDEX ind_txt ON sym(txt ASC);";
        //            mycommand.ExecuteNonQuery();

        //            mycommand.CommandText = "CREATE UNIQUE INDEX ind_id ON sym(id ASC);";
        //            mycommand.ExecuteNonQuery();
        //        }
        //        mytransaction.Commit();
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// NT-Получить минимальный ИД таблицы
        ///// </summary>
        ///// <returns></returns>
        //public Int32 getSymbolsMinId()
        //{
        //    Int32 result = 0;
        //    //create command
        //    SQLiteCommand cmd = new SQLiteCommand(m_connection);
        //    cmd.CommandText = "SELECT MIN(\"id\") FROM \"sym\";";
        //    cmd.CommandTimeout = 600;

        //    //read one result
        //    SQLiteDataReader rdr = cmd.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        rdr.Read();
        //        result = rdr.GetInt32(0);
        //    }
        //    rdr.Close();
        //    return result;
        //}

        ///// <summary>
        ///// NT-Получить максимальный ИД таблицы
        ///// </summary>
        ///// <returns></returns>
        //public Int32 getSymbolsMaxId()
        //{
        //    Int32 result = 0;
        //    //create command
        //    SQLiteCommand cmd = new SQLiteCommand(m_connection);
        //    cmd.CommandText = "SELECT MAX(\"id\") FROM \"sym\";";
        //    cmd.CommandTimeout = 600;

        //    //read one result
        //    SQLiteDataReader rdr = cmd.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        rdr.Read();
        //        result = rdr.GetInt32(0);
        //    }
        //    rdr.Close();
        //    return result;
        //}

        ///// <summary>
        ///// NT-запрос есть ли в таблице строка такого текста
        ///// </summary>
        ///// <param name="text">строка текста</param>
        ///// <returns>идентификатор строки таблицы или 0 если строки нет</returns>
        //public Int32 getIdBySymbol(string text)
        //{
        //    Int32 result = 0;
        //    //create command
        //    if (this.m_cmd1 == null)
        //    {
        //        this.m_cmd1 = new SQLiteCommand(m_connection);
        //        this.m_cmd1.CommandText = "SELECT \"id\" FROM \"sym\" WHERE (\"txt\"=?);";
        //        this.m_cmd1.CommandTimeout = 600;
        //        //parameters
        //        this.m_cmd1.Parameters.Add("p0", System.Data.DbType.String);
        //    }
        //    //read one result
        //    this.m_cmd1.Parameters[0].Value = text;
        //    SQLiteDataReader rdr = this.m_cmd1.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        rdr.Read();
        //        result = rdr.GetInt32(0);
        //    }
        //    rdr.Close();
        //    return result;
        //}

        ////запрос добавления текста в таблицу
        ////аргументы: строка текста
        ////возвращает: ничего
        //public void addSymbol(string text)
        //{
        //    //SQLiteTransaction mytransaction = this.m_connection.BeginTransaction();

        //    //create command
        //    if (this.m_cmd3 == null)
        //    {
        //        this.m_cmd3 = new SQLiteCommand(m_connection);
        //        this.m_cmd3.CommandText = "INSERT INTO \"sym\"(\"txt\", \"cnt\") VALUES (?, 1);";
        //        this.m_cmd3.CommandTimeout = 600;
        //        //parameters
        //        this.m_cmd3.Parameters.Add("p0", System.Data.DbType.String);
        //    }
        //    //insert one result
        //    this.m_cmd3.Parameters[0].Value = text;
        //    this.m_cmd3.ExecuteNonQuery();

        //    //mytransaction.Commit();

        //    return;
        //}

        ///// <summary>
        ///// NT-запрос получения текста из таблицы по идентификатору
        ///// </summary>
        ///// <param name="id">идентификатор строки</param>
        ///// <returns>строка текста или нуль</returns>
        //public string getSymbolById(Int32 id)
        //{
        //    String result = null;
        //    //create command
        //    if (this.m_cmd2 == null)
        //    {
        //        this.m_cmd2 = new SQLiteCommand(m_connection);
        //        this.m_cmd2.CommandText = "SELECT \"txt\" FROM \"sym\" WHERE (\"id\"=?);";
        //        this.m_cmd2.CommandTimeout = 600;
        //        //parameters
        //        this.m_cmd2.Parameters.Add("p0", System.Data.DbType.Int32);
        //    }
        //    //read one result
        //    this.m_cmd2.Parameters[0].Value = id;
        //    SQLiteDataReader rdr = this.m_cmd2.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        rdr.Read();
        //        result = rdr.GetString(0);
        //    }
        //    rdr.Close();
        //    return result;
        //}

        ///// <summary>
        ///// запрос инкремента счетчика строки таблицы
        ///// </summary>
        ///// <param name="id"></param>
        //public void incrementSymbolCount(int id)
        //{
        //    //create command
        //    if (this.m_cmd4 == null)
        //    {
        //        this.m_cmd4 = new SQLiteCommand(m_connection);
        //        this.m_cmd4.CommandText = "UPDATE \"sym\" SET \"cnt\"=\"cnt\" + 1 WHERE(\"id\"=?);";
        //        this.m_cmd4.CommandTimeout = 600;
        //        //parameters
        //        this.m_cmd4.Parameters.Add("p0", System.Data.DbType.Int32);
        //    }
        //    //insert one result
        //    this.m_cmd4.Parameters[0].Value = id;
        //    this.m_cmd4.ExecuteNonQuery();

        //    return;
        //}

        ///// <summary>
        ///// NT-запрос получения лексем из таблицы по идентификатору
        ///// </summary>
        ///// <param name="idFrom">Идентификатор первого элемента</param>
        ///// <param name="count">Количество элементов</param>
        ///// <returns>Список объектов строк таблицы</returns>
        //public List<TableDictItem> getSymbols(Int32 idFrom, Int32 count)
        //{
        //    List<TableDictItem> result = new List<TableDictItem>(count);
        //    Int32 idTo = idFrom + count;
        //    //create command
        //    if (this.m_cmd5 == null)
        //    {
        //        this.m_cmd5 = new SQLiteCommand(m_connection);
        //        this.m_cmd5.CommandText = "SELECT * FROM \"sym\" WHERE ((\"id\">=?) AND (\"id\"<?));";
        //        this.m_cmd5.CommandTimeout = 600;
        //        //parameters
        //        this.m_cmd5.Parameters.Add("p0", System.Data.DbType.Int32);
        //        this.m_cmd5.Parameters.Add("p1", System.Data.DbType.Int32);
        //    }
        //    //read one result
        //    this.m_cmd5.Parameters[0].Value = idFrom;
        //    this.m_cmd5.Parameters[1].Value = idFrom + count;
        //    SQLiteDataReader rdr = this.m_cmd5.ExecuteReader();
        //    if (rdr.HasRows)
        //    {
        //        while (rdr.Read())
        //        {
        //            TableDictItem td = new TableDictItem();
        //            td.m_id = rdr.GetInt32(0);
        //            td.m_text = rdr.GetString(1);
        //            td.m_count = rdr.GetInt32(2);
        //            result.Add(td);
        //        }
        //    }
        //    rdr.Close();
        //    return result;
        //}

        ///// <summary>
        ///// NT-удалить запись из таблицы по ее ROWID
        ///// </summary>
        ///// <param name="id"></param>
        //public void seqDeleteById(long id)
        //{
        //    //create command
        //    if (this.m_seqCmdDelete == null)
        //    {
        //        this.m_seqCmdDelete = new SQLiteCommand(m_connection);
        //        this.m_seqCmdDelete.CommandText = "DELETE FROM \"seq2\" WHERE(\"ROWID\"=?);";
        //        this.m_seqCmdDelete.CommandTimeout = 6000;
        //        //parameters
        //        this.m_seqCmdDelete.Parameters.Add("p0", System.Data.DbType.Int64);
        //    }
        //    //insert one result
        //    this.m_seqCmdDelete.Parameters[0].Value = id;
        //    this.m_seqCmdDelete.ExecuteNonQuery();

        //    return;
        //}

        ///// <summary>
        ///// NT-добавить запись в БД
        ///// </summary>
        ///// <param name="codes">массив кодов значений</param>
        ///// <param name="cnt">значение для счетчика</param>
        //public void seqAddSequence(Int32[] codes, int cnt)
        //{
        //    //тут надо или увеличить значение счетчика в таблице или создать новую запись.
        //    Int64 id = seqGetRecordId(codes);
        //    //если записи нет, то применяется INSERT
        //    //если запись есть, применяется UPDATE
        //    if (id == -1)
        //        seqInsert(codes, cnt);
        //    else
        //        Seq2Update(id, (UInt32)cnt);
        //    return;
        //}

        #endregion

        #region Функции пользовательские

        //TODO: Добавить код новых функций здесь, каждый комплект функций для таблицы поместить в отдельный region
        //новые команды для них обязательно внести в ClearCommands(), иначе транзакции будут работать неправильно. 

        #endregion

        /// <summary>
        /// NT-Выполнить запрос без аргументов и вернуть ридер с результатами
        /// </summary>
        /// <param name="query">текст запроса</param>
        /// <param name="timeout">таймаут запроса</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string query, int timeout)
        {
            SQLiteCommand cmd = new SQLiteCommand(query, this.m_connection, this.m_transaction);
            cmd.CommandTimeout = timeout;
            SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr;
        }

    }//end class
}
