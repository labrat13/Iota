using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;

namespace DataBaseAdapters
{
    class Program
    { 

        
        static void Main(string[] args)
        {
            /* Это шаблонные классы для работы с БД разных типов.
             * Их затем можно импортировать в проект и быстро наполнить правильным кодом.
             * Ожидается, что это в целом будет и класс - пример кода, и инструкция по использованию БД.
             * Но пока это все же не работающий пример, а характерный набор функций, натасканных из различных моих проектов,
             * которые можно использовать как пример реализации.
             * TODO: все классы надо привести в единый состав функций.
             * И это все не компилируется, конечно. Так как там есть другие классы, а переписывать для них код мне некогда.
             * Я рассчитываю привести все это в порядок в процессе использования этого шаблона в своих проектах.
            */

            //Sqlite  database
        SqliteDbAdapter m_databaseSrc = null;
        try
        {
            //создаем строку подключения
            String constring = SqliteDbAdapter.createConnectionString("C:\\Temp\\1test.sqlite", false);

            //создаем объект адаптера БД
            m_databaseSrc = new SqliteDbAdapter(constring, false);
            //открываем БД
            m_databaseSrc.Open();
            //устанавливаем число занимаемых кэшем БД страниц в памяти.
            //одна страница размером около 4кб, точнее написано в описании БД
            m_databaseSrc.ExecuteNonQuery("PRAGMA cache_size = 50000;", 600);
            //Теперь перестроим индексы БД, если они были и были данные
            //в результате новые записи добавляются быстрее.
            m_databaseSrc.ExecuteNonQuery("ANALYZE", 600);
            //получим минимальный идентификатор записи
            Int32 minId = m_databaseSrc.getTableMinInt32("tablename", "id");
            //получим максимальный идентификатор записи
            Int32 maxId = m_databaseSrc.getTableMaxInt32("tablename", "id");
            //Теперь если мы должны что-то записать в БД, надо открыть транзакцию
            //Иначе БД будет работать медленно
            //При чтении транзакцию открывать не нужно
            m_databaseSrc.TransactionBegin();
            //Тут мы что-то пишем в БД
            //.....
            //Принять транзакцию
            m_databaseSrc.TransactionCommit();
            //Закрыть БД
        m_databaseSrc.Close();
        m_databaseSrc = null;
        }
        catch (Exception ex)
        {
            m_databaseSrc.TransactionRollback();
            m_databaseSrc.Close();
        }


        return;
        }
    }
}
