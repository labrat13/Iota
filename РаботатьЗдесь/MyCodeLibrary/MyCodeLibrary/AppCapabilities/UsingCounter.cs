using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyCodeLibrary
{
    // Предполагаемая ситуация для использования: Необходимость определения используемости функций приложения.
 
    // Это класс для подсчета статистики использования функций приложения. Например, вызовов пунктов меню.
    // Для этого каждой функции должно быть назначено короткое символьное имя-тег. Например, Func012.
    // При инициализации приложения нужно создать глобальный объект класса конструктором. 
    // Конструктору нужно передать путь к файлу статистики. 
    // Этот файл должен располагаться в каталоге, где пользователь имеет права на запись. 
    // Типично в User/LocalSettings/AppName/... Не в каталоге приложения в ProgramFiles.
    // Затем надо загрузить счетчики из файла статистики вызовом Load().
    // Вызов Add("Func012") создает счетчик с таким тегом. Или увеличивает этот счетчик, если он уже существует.
    // Перед завершением приложения необходимо сохранить счетчики вызовом Store().
    // Доступ к значениям счетчиков осуществляется через проперти - ссылку на словарь счетчиков, использовать тег счетчика.

    /// <summary>
    /// Это класс для подсчета статистики использования функций приложения. Например, вызовов пунктов меню.
    /// Это позволяет выявить часто или редко используемые функции приложения и соответственно управлять видимостью пунктов меню.
    /// И отсылать статистику разработчику, чтобы выявить неиспользуемые функции.
    /// </summary>
    public class UsingCounter
    {
        /// <summary>
        /// Путь к файлу, хранящему счетчики. 
        /// Этот файл должен располагаться в каталоге, где пользователь имеет права на запись.
        /// Типично в User/LocalSettings/AppName/...
        /// Не в каталоге приложения в ProgramFiles.
        /// </summary>
        private String m_CounterFileName;
        /// <summary>
        /// Словарь счетчиков и их имен-тегов
        /// </summary>
        private Dictionary<String, UInt64> m_Counters;
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="counterFileName">Путь к файлу, хранящему счетчики. 
        /// Этот файл должен располагаться в каталоге, где пользователь имеет права на запись. 
        /// Типично в User/LocalSettings/AppName/...
        /// Не в каталоге приложения в ProgramFiles.
        /// </param>
        public UsingCounter(String counterFileName)
        {
            m_Counters = new Dictionary<string, UInt64>();
            m_CounterFileName = counterFileName;
        }
        /// <summary>
        /// Ссылка на словарь счетчиков
        /// </summary>
        public Dictionary<String, UInt64> Counters
        {
            get { return m_Counters; }
        }
        /// <summary>
        /// Загрузить статистику из файла статистики.
        /// </summary>
        public void Load()
        {
            //файл статистики может еще не существовать. 
            //Это может произойти при первом вызове всей системы, когда счетчики еще не создавались. 
            //Эту ситуацию нужно игнорировать.

            //формат файла:
            //funcTag001=123
            //functag002=456
            //functag003=789

            //очистить словарь счетчиков на случай если файл статистики недоступен.
            //чтобы перезагрузка статистики соответствовала реальному состоянию вещей.
            //а не использовались старые значения счетчиков из словаря.
            Clear();
            //пытаемся загрузить статистику из файла
            if (!File.Exists(m_CounterFileName))
                return;
            else
            {
                char[] separator = new char[] { '=', ' ' };
                StreamReader sr = new StreamReader(m_CounterFileName, Encoding.UTF8);
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] sar = line.Split(separator , StringSplitOptions.RemoveEmptyEntries);
                    String tag = sar[0];
                    UInt64 counter = UInt64.Parse(sar[1]);
                    m_Counters.Add(tag, counter);
                }
                sr.Close();
            }

            return;
        }
        /// <summary>
        /// Сохранить статистику в файл статистики
        /// </summary>
        public void Store()
        {
            //файл статистики может еще не существовать. 
            //Это может произойти при первом вызове всей системы, когда счетчики еще не создавались. 
            //Эту ситуацию нужно игнорировать.

            //если папки для файла статистики нет, ее надо создать.
            //иначе нижеследующий код не сможет создать там файл.
            String dir = Path.GetDirectoryName(m_CounterFileName);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            //формат файла:
            //funcTag001=123
            //functag002=456
            //functag003=789

            //вывод в файл
            StreamWriter sw = new StreamWriter(m_CounterFileName, false, Encoding.UTF8);
            foreach (KeyValuePair<String, UInt64> kvp in m_Counters)
            {
                sw.WriteLine("{0}={1}", kvp.Key, kvp.Value);
            }
            sw.Close();
            return;
        }
        /// <summary>
        /// Добавить одно использование функции
        /// </summary>
        /// <param name="tagname">
        /// Имя тега - аббревиатура функции, именующая счетчик вызовов этой функции приложения.
        /// Имя тега не должно содержать пробелы, переносы строк и знак =
        /// Имя тега не должно быть пустой строкой
        /// </param>
        /// <exception cref="ArgumentException">Имя тега не должно быть пустым или = null</exception>
        public void Use(String tagname)
        {
            //check tagname
            if (String.IsNullOrEmpty(tagname))
                throw new ArgumentException("Tagname cannot be null or empty");
            //add or increment counters
            if (m_Counters.ContainsKey(tagname))
                m_Counters[tagname]++;
            else
                m_Counters.Add(tagname, 1);
        }
        /// <summary>
        /// Очистка словаря счетчиков
        /// </summary>
        public void Clear()
        {
            this.m_Counters.Clear();
        }


    }


    #region *** Example of using ***
    //class Program
    //{
    //    static void Main(string[] args)
    //    {

    //        //инициализация. 
    //        //Тут получаем путь к папке локальных данных приложений запущенных под текущим пользователем
    //        //Так как именно в этой папке нужно хранить специфичные для пользователя настройки приложения.
    //        //И в этой папке у пользователя наверняка должны быть права на запись.
    //        String path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    //        //добавляем имя приложения и номер версии как подпапки
    //        //добавляем имя файла настроек
    //        path = Path.Combine(path, "AppName\\ver0\\text.txt");
    //        //C:\\Documents and Settings\\василий\\Application Data\\AppName\\ver0\\text.txt
    //        UsingCounter uc = new UsingCounter(path);

    //        //загрузка счетчиков
    //        uc.Load();
    //        //инкремент счетчиков как имитация использования
    //        uc.Use("tag001");
    //        uc.Use("tag002");
    //        uc.Use("tag003");
    //        uc.Use("tag001");
    //        uc.Use("tag002");
    //        uc.Use("tag003");

    //        //error: invalid tagnames
    //        //uc.Add("");
    //        //uc.Add(null);

    //        //выгрузка
    //        uc.Store();
    //        //загрузка
    //        uc.Load();
    //        //проверка
    //        foreach (KeyValuePair<String, UInt64> kvp in uc.Counters)
    //        {
    //            Console.WriteLine("{0}={1}", kvp.Key, kvp.Value);
    //        }

    //        Console.WriteLine("end");
    //    }
    //}

    #endregion

}
